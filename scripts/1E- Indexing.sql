-- Step 1: Initial query plan without indexes
EXPLAIN ANALYZE
SELECT t.tconst, t.primary_title
FROM movie.title t
LEFT JOIN movie.omdb_extra o USING (tconst)
WHERE t.primary_title ILIKE '%matrix%'
   OR o.plot ILIKE '%matrix%'
LIMIT 30;

-- Step 2: Enable the pg_trgm extension if not already enabled
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- Step 3: Create a trigram index on the primary_title column
CREATE INDEX IF NOT EXISTS ix_title_primary_title_trgm
  ON movie.title USING gin (primary_title gin_trgm_ops);

EXPLAIN ANALYZE
SELECT t.tconst, t.primary_title
FROM movie.title t
LEFT JOIN movie.omdb_extra o USING (tconst)
WHERE t.primary_title ILIKE '%matrix%'
-- OR o.plot ILIKE '%matrix%'   -- keep this commented for now
LIMIT 30;

-- step 4: make plot searches faster
-- this index helps when we search inside movie plots using ILIKE
-- it’s used by the search functions in the 1D part
CREATE INDEX IF NOT EXISTS ix_omdb_plot_trgm
  ON movie.omdb_extra USING gin (plot gin_trgm_ops);


-- test it out
-- this checks both title and plot together
EXPLAIN ANALYZE
SELECT t.tconst, t.primary_title
FROM movie.title t
LEFT JOIN movie.omdb_extra o USING (tconst)
WHERE t.primary_title ILIKE '%matrix%'
   OR o.plot ILIKE '%matrix%'
LIMIT 30;

-- step 5: refresh postgres stats so it knows about the new index
ANALYZE movie.omdb_extra;

EXPLAIN ANALYZE
SELECT t.tconst, t.primary_title
FROM movie.title t
LEFT JOIN movie.omdb_extra o USING (tconst)
WHERE t.primary_title ILIKE '%matrix%'
   OR o.plot ILIKE '%matrix%'
LIMIT 30;

-- step 6: make co-player and character lookups faster
-- this helps functions like find_coplayers and structured_string_search
CREATE INDEX IF NOT EXISTS ix_credit_tconst
  ON movie.credit (tconst);

CREATE INDEX IF NOT EXISTS ix_credit_nconst
  ON movie.credit (nconst);

-- step 6 test: co-players pattern (raw query version)
-- idea: “who worked with Tom Hanks?” (nm0000158)
-- this should use the tconst/nconst indexes we added
EXPLAIN ANALYZE
SELECT c2.nconst, COUNT(*) AS cnt
FROM movie.credit c1
JOIN movie.credit c2 ON c1.tconst = c2.tconst
WHERE c1.nconst = 'nm0000158'
  AND c2.nconst <> c1.nconst
GROUP BY c2.nconst
ORDER BY cnt DESC
LIMIT 15;

-- step 7: make actor/character lookups faster
-- helps when we filter by category or search characters by name
CREATE INDEX IF NOT EXISTS ix_credit_category_tconst
  ON movie.credit (category, tconst);

-- makes ILIKE searches on characters faster
CREATE INDEX IF NOT EXISTS ix_credit_characters_trgm
  ON movie.credit USING gin (characters gin_trgm_ops);

-- step 8: refresh stats for the credit table (so planner “sees” new indexes)
ANALYZE movie.credit;

-- step 8 test A: character search should be faster now (uses trigram on characters)
EXPLAIN ANALYZE
SELECT tconst
FROM movie.credit
WHERE characters ILIKE '%Neo%'
LIMIT 30;

-- step 8 test B: actors-only filter should be faster (uses category,tconst index)
EXPLAIN ANALYZE
SELECT COUNT(*)
FROM movie.credit
WHERE category IN ('actor','actress');

-- step 9: make person name searches faster
-- used when we search people by name or profession
CREATE INDEX IF NOT EXISTS ix_person_primary_name_trgm
  ON movie.person USING gin (primary_name gin_trgm_ops);

CREATE INDEX IF NOT EXISTS ix_person_primary_profession_trgm
  ON movie.person USING gin (primary_profession gin_trgm_ops);

-- step 9 test A: search people by name (uses trigram on primary_name)
EXPLAIN ANALYZE
SELECT nconst, primary_name
FROM movie.person
WHERE primary_name ILIKE '%tom%'
LIMIT 20;

-- step 9 test B: search by profession (uses trigram on primary_profession)
EXPLAIN ANALYZE
SELECT nconst, primary_name, primary_profession
FROM movie.person
WHERE primary_profession ILIKE '%director%'
LIMIT 20;

-- step 10: make keyword lookups faster
-- speeds up best/exact match functions
CREATE INDEX IF NOT EXISTS ix_wi_word
  ON movie.wi (word);

CREATE INDEX IF NOT EXISTS ix_wi_tconst
  ON movie.wi (tconst);

-- step 10 test A: quick word lookup (should use ix_wi_word)
EXPLAIN ANALYZE
SELECT tconst
FROM movie.wi
WHERE word = 'love'
LIMIT 30;

-- step 10 test B: simulate “titles that have BOTH words”
-- (this is like your exact-match logic)
EXPLAIN ANALYZE
SELECT w1.tconst
FROM movie.wi w1
WHERE w1.word = 'love'
  AND EXISTS (
    SELECT 1 FROM movie.wi w2
    WHERE w2.tconst = w1.tconst
      AND w2.word = 'story'
  )
LIMIT 30;

