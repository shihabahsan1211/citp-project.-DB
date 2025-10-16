--1D.1

-- STEP 0: Always work in these schemas first
SET search_path = framework, movie, public;
-- STEP 1: Create a user and return their id
CREATE OR REPLACE FUNCTION framework.create_user(
  p_username TEXT,
  p_email TEXT,
  p_password_hash TEXT
)
RETURNS BIGINT
LANGUAGE plpgsql AS $$
DECLARE v_user_id BIGINT;
BEGIN
  INSERT INTO framework.app_user(username, email, password_hash)
  VALUES (p_username, p_email, p_password_hash)
  RETURNING user_id INTO v_user_id;

  RETURN v_user_id;
END $$;
-- STEP 2: Get a user's id by username
CREATE OR REPLACE FUNCTION framework.get_user_id(p_username TEXT)
RETURNS BIGINT
LANGUAGE sql AS $$
  SELECT user_id
  FROM framework.app_user
  WHERE username = p_username;
$$;
-- STEP 3: Bookmark a title
CREATE OR REPLACE FUNCTION framework.bookmark_title(p_user_id BIGINT, p_tconst TEXT)
RETURNS VOID
LANGUAGE sql AS $$
  INSERT INTO framework.user_bookmark_title(user_id, tconst)
  VALUES (p_user_id, p_tconst)
  ON CONFLICT DO NOTHING;
$$;
-- STEP 4: Remove a title bookmark
CREATE OR REPLACE FUNCTION framework.unbookmark_title(p_user_id BIGINT, p_tconst TEXT)
RETURNS VOID
LANGUAGE sql AS $$
  DELETE FROM framework.user_bookmark_title
  WHERE user_id = p_user_id
    AND tconst  = p_tconst;
$$;
-- STEP 5: Bookmark a person
CREATE OR REPLACE FUNCTION framework.bookmark_person(p_user_id BIGINT, p_nconst TEXT)
RETURNS VOID
LANGUAGE sql AS $$
  INSERT INTO framework.user_bookmark_person(user_id, nconst)
  VALUES (p_user_id, p_nconst)
  ON CONFLICT DO NOTHING;
$$;

-- STEP 6: Remove a person bookmark
CREATE OR REPLACE FUNCTION framework.unbookmark_person(p_user_id BIGINT, p_nconst TEXT)
RETURNS VOID
LANGUAGE sql AS $$
  DELETE FROM framework.user_bookmark_person
  WHERE user_id = p_user_id
    AND nconst  = p_nconst;
$$;
-- STEP 7: List bookmarked titles for a user
CREATE OR REPLACE FUNCTION framework.list_bookmarked_titles(p_user_id BIGINT)
RETURNS TABLE(tconst TEXT, primary_title TEXT)
LANGUAGE sql AS $$
  SELECT ub.tconst, t.primary_title
  FROM framework.user_bookmark_title ub
  JOIN movie.title t USING (tconst)
  WHERE ub.user_id = p_user_id
  ORDER BY t.primary_title;
$$;
-- STEP 8: List bookmarked people for a user
CREATE OR REPLACE FUNCTION framework.list_bookmarked_people(p_user_id BIGINT)
RETURNS TABLE(nconst TEXT, primary_name TEXT)
LANGUAGE sql AS $$
  SELECT ub.nconst, p.primary_name
  FROM framework.user_bookmark_person ub
  JOIN movie.person p USING (nconst)
  WHERE ub.user_id = p_user_id
  ORDER BY p.primary_name;
$$;
-- STEP 9 (Framework): Get a user's search history
CREATE OR REPLACE FUNCTION framework.get_search_history(p_user_id BIGINT)
RETURNS TABLE(search_id BIGINT, query_text TEXT, performed_at TIMESTAMPTZ)
LANGUAGE sql AS $$
  SELECT search_id, query_text, performed_at
  FROM framework.user_search_history
  WHERE user_id = p_user_id
  ORDER BY performed_at DESC;
$$;

-- 1-D.2
CREATE OR REPLACE FUNCTION framework.string_search(p_user_id BIGINT, p_query TEXT)
RETURNS TABLE (tconst TEXT, primary_title TEXT)
LANGUAGE sql AS $$
  WITH _log AS (
    INSERT INTO framework.user_search_history(user_id, query_text)
    VALUES (p_user_id, p_query)
    RETURNING 1
  )
  SELECT t.tconst, t.primary_title
  FROM movie.title t
  LEFT JOIN movie.omdb_extra o USING (tconst)
  WHERE (t.primary_title ILIKE '%' || p_query || '%')
     OR (o.plot         ILIKE '%' || p_query || '%')
  ORDER BY t.primary_title;
$$;
-- 1-D.3
-- STEP 1A: See if duplicates exist
SELECT user_id, tconst, COUNT(*) AS how_many
FROM framework.user_title_rating
GROUP BY user_id, tconst
HAVING COUNT(*) > 1;
-- STEP 1B (Rating): Remove duplicate ratings, keep the newest per (user_id, tconst)
WITH ranked AS (
  SELECT
    rating_id,
    ROW_NUMBER() OVER (
      PARTITION BY user_id, tconst
      ORDER BY rated_at DESC, rating_id DESC
    ) AS rn
  FROM framework.user_title_rating
)
DELETE FROM framework.user_title_rating utr
USING ranked r
WHERE utr.rating_id = r.rating_id
  AND r.rn > 1;

-- STEP 1C (Rating): Verify no duplicates remain
SELECT user_id, tconst, COUNT(*) AS how_many
FROM framework.user_title_rating
GROUP BY user_id, tconst
HAVING COUNT(*) > 1;

-- STEP 1D (Rating): Enforce one rating per user per title
CREATE UNIQUE INDEX IF NOT EXISTS uq_user_title_unique
ON framework.user_title_rating(user_id, tconst);

-- STEP 2 (Rating): Core rate() function with proper averaging 
CREATE OR REPLACE FUNCTION framework.rate(
  p_user_id BIGINT,
  p_tconst  TEXT,
  p_rating  SMALLINT
)
RETURNS TABLE (average_rating NUMERIC(3,1), num_votes INT)
LANGUAGE plpgsql AS $$
DECLARE
  v_prev_rating  SMALLINT;
  v_had_prev     BOOLEAN := FALSE;
  v_old_avg      NUMERIC;
  v_old_votes    INT;
  v_new_avg      NUMERIC;
  v_dummy        INT;
BEGIN
  -- 1) Validate rating range
  IF p_rating < 1 OR p_rating > 10 THEN
    RAISE EXCEPTION 'Rating must be between 1 and 10';
  END IF;

  -- 2) Ensure title exists
  PERFORM 1 FROM movie.title WHERE tconst = p_tconst;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Unknown title (tconst=%)', p_tconst;
  END IF;

  -- 3) Check if user has rated before
  SELECT rating INTO v_prev_rating
  FROM framework.user_title_rating
  WHERE user_id = p_user_id AND tconst = p_tconst;
  IF FOUND THEN
    v_had_prev := TRUE;
  END IF;

  -- 4) Upsert user rating (history table as the source of truth per user/title)
  IF v_had_prev THEN
    UPDATE framework.user_title_rating
    SET rating = p_rating, rated_at = now()
    WHERE user_id = p_user_id AND tconst = p_tconst;
  ELSE
    INSERT INTO framework.user_title_rating(user_id, tconst, rating)
    VALUES (p_user_id, p_tconst, p_rating);
  END IF;

  -- 5) Lock/read current aggregate rating row for this title
  SELECT average_rating::NUMERIC, num_votes
  INTO v_old_avg, v_old_votes
  FROM movie.title_rating
  WHERE tconst = p_tconst
  FOR UPDATE;

  -- 6) Insert or update aggregate with correct math
  IF NOT FOUND THEN
    -- First ever vote for this title
    INSERT INTO movie.title_rating(tconst, average_rating, num_votes)
    VALUES (p_tconst, p_rating, 1);
  ELSE
    IF v_had_prev THEN
      -- Replace user's previous rating (votes unchanged)
      v_new_avg := (v_old_avg * v_old_votes - v_prev_rating + p_rating) / NULLIF(v_old_votes, 0);
      UPDATE movie.title_rating
      SET average_rating = ROUND(v_new_avg::NUMERIC, 1)
      WHERE tconst = p_tconst;
    ELSE
      -- First time this user rates (add one vote)
      v_new_avg := (v_old_avg * v_old_votes + p_rating) / (v_old_votes + 1);
      UPDATE movie.title_rating
      SET average_rating = ROUND(v_new_avg::NUMERIC, 1),
          num_votes      = v_old_votes + 1
      WHERE tconst = p_tconst;
    END IF;
  END IF;

  -- 7) Return the fresh numbers
  RETURN QUERY
  SELECT tr.average_rating, tr.num_votes
  FROM movie.title_rating tr
  WHERE tr.tconst = p_tconst;
END $$;

--1-D.4

-- STEP 1 (Structured search): create the function
CREATE OR REPLACE FUNCTION framework.structured_string_search(
  p_user_id BIGINT,
  p_title TEXT,
  p_plot TEXT,
  p_characters TEXT,
  p_person_name TEXT
)
RETURNS TABLE (tconst TEXT, primary_title TEXT)
LANGUAGE sql AS $$
  WITH params AS (
    SELECT
      NULLIF(btrim(p_title), '')       AS q_title,
      NULLIF(btrim(p_plot), '')        AS q_plot,
      NULLIF(btrim(p_characters), '')  AS q_char,
      NULLIF(btrim(p_person_name), '') AS q_person
  ), _log AS (
    INSERT INTO framework.user_search_history(user_id, query_text)
    SELECT p_user_id,
           'title=' || COALESCE((SELECT q_title  FROM params), '∅') || '; ' ||
           'plot='  || COALESCE((SELECT q_plot   FROM params), '∅') || '; ' ||
           'char='  || COALESCE((SELECT q_char   FROM params), '∅') || '; ' ||
           'person='|| COALESCE((SELECT q_person FROM params), '∅')
    RETURNING 1
  )
  SELECT t.tconst, t.primary_title
  FROM movie.title t
  LEFT JOIN movie.omdb_extra o USING (tconst)
  WHERE
    ((SELECT q_title FROM params) IS NULL OR t.primary_title ILIKE '%' || (SELECT q_title FROM params) || '%')
    AND ((SELECT q_plot  FROM params) IS NULL OR o.plot ILIKE '%' || (SELECT q_plot FROM params) || '%')
    AND ((SELECT q_char  FROM params) IS NULL OR EXISTS (
          SELECT 1 FROM movie.credit c
          WHERE c.tconst = t.tconst
            AND c.characters ILIKE '%' || (SELECT q_char FROM params) || '%'))
    AND ((SELECT q_person FROM params) IS NULL OR EXISTS (
          SELECT 1 FROM movie.credit c
          JOIN movie.person p ON p.nconst = c.nconst
          WHERE c.tconst = t.tconst
            AND p.primary_name ILIKE '%' || (SELECT q_person FROM params) || '%'))
  ORDER BY t.primary_title;
$$;

--Some testing queries
-- STEP 2 (Structured search): Try a test call
SELECT * 
FROM framework.structured_string_search(
  framework.get_user_id('shibz'),  -- user_id
  'Matrix',                        -- title_q
  NULL,                            -- plot_q
  NULL,                            -- characters_q
  NULL                             -- person_q
);
-- STEP 3 (Structured search): Confirm logging worked
SELECT * 
FROM framework.user_search_history
WHERE user_id = framework.get_user_id('shibz')
ORDER BY performed_at DESC
LIMIT 5;

--1-D.5
-- STEP 1 (Finding names): Simple name search
CREATE OR REPLACE FUNCTION framework.find_names(
  p_user_id BIGINT,
  p_query TEXT
)
RETURNS TABLE (nconst TEXT, primary_name TEXT, primary_profession TEXT)
LANGUAGE sql AS $$
  WITH _log AS (
    INSERT INTO framework.user_search_history(user_id, query_text)
    VALUES (p_user_id, 'find_names=' || p_query)
    RETURNING 1
  )
  SELECT p.nconst, p.primary_name, p.primary_profession
  FROM movie.person p
  WHERE p.primary_name ILIKE '%' || p_query || '%'
     OR p.primary_profession ILIKE '%' || p_query || '%'
  ORDER BY p.primary_name
  LIMIT 50;
$$;

-- STEP 2 (Finding names): Test the function
SELECT * 
FROM framework.find_names(
  framework.get_user_id('shibz'),  -- user_id
  'Tom'                            -- query string
);

--1-D.6

-- 1-D.6: Finding co-players (by actor name)

-- =========================================
-- 1-D.6  Finding co-players
-- =========================================


-- ---------- A) Core function (by person ID) ----------
-- Drops older version if any, then creates a clean, typed function.
DROP FUNCTION IF EXISTS movie.find_coplayers(TEXT);

CREATE OR REPLACE FUNCTION movie.find_coplayers(p_nconst TEXT)
RETURNS TABLE (co_nconst TEXT, co_name TEXT, movies_together BIGINT)
LANGUAGE plpgsql AS $$
BEGIN
  RETURN QUERY
  SELECT
      c2.nconst                           AS co_nconst,
      p.primary_name                      AS co_name,
      COUNT(DISTINCT c1.tconst)           AS movies_together
  FROM movie.credit AS c1
  JOIN movie.credit AS c2
    ON c1.tconst = c2.tconst
  JOIN movie.person AS p
    ON p.nconst = c2.nconst
  WHERE c1.nconst = p_nconst
    AND c2.nconst <> p_nconst
    AND c1.category IN ('actor','actress')
    AND c2.category IN ('actor','actress')
  GROUP BY c2.nconst, p.primary_name
  ORDER BY movies_together DESC, co_name;
END $$;

-- ---------- B) Convenience function (by actor name + logs to framework) ----------
-- Drop old if return type changed
DROP FUNCTION IF EXISTS framework.find_coplayers_by_name(BIGINT, TEXT, INT);

CREATE OR REPLACE FUNCTION framework.find_coplayers_by_name(
  p_user_id    BIGINT,
  p_actor_name TEXT,
  p_top_n      INT DEFAULT 20
)
RETURNS TABLE (nconst TEXT, primary_name TEXT, freq BIGINT)
LANGUAGE plpgsql AS $$
BEGIN
  -- Log the search (framework in play)
  INSERT INTO framework.user_search_history(user_id, query_text)
  VALUES (p_user_id, 'coplayers_by_name=' || p_actor_name);

  -- Resolve target actors by name, collect their titles, then count co-actors
  RETURN QUERY
  WITH targets AS (
    SELECT p.nconst AS target_nconst
    FROM movie.person AS p
    WHERE p.primary_name ILIKE '%' || p_actor_name || '%'
  ),
  target_titles AS (
    SELECT DISTINCT c1.tconst
    FROM movie.credit AS c1
    JOIN targets AS tg ON c1.nconst = tg.target_nconst
    WHERE c1.category IN ('actor','actress')
  )
  SELECT
      c2.nconst                               AS nconst,
      p.primary_name                           AS primary_name,
      COUNT(DISTINCT c2.tconst)                AS freq
  FROM target_titles AS tt
  JOIN movie.credit AS c1 ON c1.tconst = tt.tconst
  JOIN movie.credit AS c2 ON c2.tconst = tt.tconst
  JOIN movie.person AS p  ON p.nconst = c2.nconst
  WHERE c2.nconst <> c1.nconst
    AND c1.category IN ('actor','actress')
    AND c2.category IN ('actor','actress')
  GROUP BY c2.nconst, p.primary_name
  ORDER BY freq DESC, primary_name
  LIMIT p_top_n;
END $$;



-- Look up an actor's ID (example)
SELECT nconst, primary_name
FROM movie.person
WHERE primary_name ILIKE '%Hanks%';

-- Test the by-ID core function
SELECT * FROM movie.find_coplayers('nm0000158')  -- Tom Hanks nconst, adjust to your result
LIMIT 15;

-- Test the by-name + logging function
SELECT *
FROM framework.find_coplayers_by_name(
  framework.get_user_id('shibz'),
  'Tom Hanks',
  15
);

-- STEP 1 (load credits): populate movie.credit from public.title_principals
INSERT INTO movie.credit (tconst, nconst, category, job, characters)
SELECT
  tp.tconst,
  tp.nconst,
  tp.category,
  NULLIF(tp.job, '\N')        AS job,
  NULLIF(tp.characters, '\N') AS characters
FROM public.title_principals AS tp
JOIN movie.title  AS t ON t.tconst  = tp.tconst
JOIN movie.person AS p ON p.nconst  = tp.nconst;


-- sanity: do Keanu Reeves now have credits?
SELECT DISTINCT c.tconst, c.category
FROM movie.credit c
WHERE c.nconst = 'nm0000206'
LIMIT 15;

-- run co-players by ID
SELECT *
FROM movie.find_coplayers('nm0000206')
LIMIT 15;






