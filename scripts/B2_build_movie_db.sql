-- BLOCK 1: 
-- wipe & scaffold a clean schema for our model
-- Re-run safe: drop then recreate 'movie' schema. This way we have a clean slate.
-- CASCADE means "drop everything inside too"

DROP SCHEMA IF EXISTS movie CASCADE;
CREATE SCHEMA movie;

-- keep search_path explicit so we don't accidentally target 'public'
SET search_path = movie, public;

-- BLOCK 2: Create clean tables in 'movie' schema

-- Titles
CREATE TABLE title (
    tconst TEXT PRIMARY KEY,
    title_type TEXT,
    primary_title TEXT,
    original_title TEXT,
    is_adult BOOLEAN,
    start_year INT,
    end_year INT,
    runtime_minutes INT
);

-- People
CREATE TABLE person (
    nconst TEXT PRIMARY KEY,
    primary_name TEXT,
    birth_year INT,
    death_year INT,
    primary_profession TEXT
);

-- Credits (actors, crew, etc.)
CREATE TABLE credit (
    tconst TEXT REFERENCES title(tconst),
    nconst TEXT REFERENCES person(nconst),
    category TEXT,
    job TEXT,
    characters TEXT
);

-- Genres
CREATE TABLE title_genre (
    tconst TEXT REFERENCES title(tconst),
    genre TEXT,
    PRIMARY KEY (tconst, genre)
);

-- Ratings
CREATE TABLE title_rating (
    tconst TEXT PRIMARY KEY REFERENCES title(tconst),
    average_rating NUMERIC(3,1),
    num_votes INT
);

-- Alternate titles
CREATE TABLE title_aka (
    tconst TEXT REFERENCES title(tconst),
    ordering INT,
    aka_title TEXT,
    region TEXT,
    language TEXT,
    types TEXT,
    attributes TEXT,
    is_original_title BOOLEAN,
    PRIMARY KEY (tconst, ordering)
);

-- Episodes
CREATE TABLE title_episode (
    tconst TEXT PRIMARY KEY REFERENCES title(tconst),
    parent_tconst TEXT REFERENCES title(tconst),
    season_number INT,
    episode_number INT
);

-- OMDb extra info
CREATE TABLE omdb_extra (
    tconst TEXT PRIMARY KEY REFERENCES title(tconst),
    plot TEXT,
    poster TEXT
);


-- BLOCK Create 'wi' table in movie schema
DROP TABLE IF EXISTS wi CASCADE;

CREATE TABLE wi (
  tconst TEXT NOT NULL REFERENCES title(tconst) ON DELETE CASCADE,
  word   TEXT NOT NULL,
  PRIMARY KEY (tconst, word)
);



-- BLOCK 3A: migrate Titles and People (robust across text/int + \N)

-- Titles
INSERT INTO movie.title (
  tconst, title_type, primary_title, original_title, is_adult,
  start_year, end_year, runtime_minutes
)
SELECT
  tb.tconst,
  tb.titletype,
  tb.primarytitle,
  tb.originaltitle,
  CASE
    WHEN lower(coalesce(nullif(tb.isadult::text, '\N'), '0')) IN ('1','t','true','y','yes')
      THEN true
    ELSE false
  END AS is_adult,
  CASE WHEN trim(tb.startyear::text)       ~ '^\d+$' THEN trim(tb.startyear::text)::int       ELSE NULL END AS start_year,
  CASE WHEN trim(tb.endyear::text)         ~ '^\d+$' THEN trim(tb.endyear::text)::int         ELSE NULL END AS end_year,
  CASE WHEN trim(tb.runtimeminutes::text)  ~ '^\d+$' THEN trim(tb.runtimeminutes::text)::int  ELSE NULL END AS runtime_minutes
FROM public.title_basics tb;

-- People
INSERT INTO movie.person (
  nconst, primary_name, birth_year, death_year, primary_profession
)
SELECT
  nb.nconst,
  nb.primaryname,
  CASE WHEN trim(nb.birthyear::text) ~ '^\d+$' THEN trim(nb.birthyear::text)::int ELSE NULL END AS birth_year,
  CASE WHEN trim(nb.deathyear::text) ~ '^\d+$' THEN trim(nb.deathyear::text)::int ELSE NULL END AS death_year,
  nb.primaryprofession
FROM public.name_basics nb;

SELECT COUNT(*) FROM movie.title;
SELECT COUNT(*) FROM movie.person;
SELECT * FROM movie.title  LIMIT 5;
SELECT * FROM movie.person LIMIT 5;

-- BLOCK 3B: migrate Genres and Ratings into movie schema

---------- Genres ---------- 
-- Split comma-separated genres into rows; ignore NULLs and '\N'
INSERT INTO movie.title_genre (tconst, genre)
SELECT
  tb.tconst,
  trim(g) AS genre
FROM public.title_basics tb
CROSS JOIN LATERAL regexp_split_to_table(tb.genres::text, ',') AS g
WHERE tb.genres IS NOT NULL
  AND tb.genres::text <> '\N'
  AND trim(g) <> '';

---------- Ratings ---------- 
-- Copy ratings only for titles we actually kept in movie.title
INSERT INTO movie.title_rating (tconst, average_rating, num_votes)
SELECT
  tr.tconst,
  tr.averagerating::numeric(3,1),
  tr.numvotes::int
FROM public.title_ratings tr
JOIN movie.title t ON t.tconst = tr.tconst;  -- safety join

SELECT COUNT(*) AS genres_rows  FROM movie.title_genre;
SELECT COUNT(*) AS ratings_rows FROM movie.title_rating;

SELECT * FROM movie.title_genre  LIMIT 10;
SELECT * FROM movie.title_rating LIMIT 10;

-- BLOCK: Populate from public.wi (columns: token, tconst)
INSERT INTO movie.wi (tconst, word)
SELECT DISTINCT tb.tconst, w
FROM public.title_basics tb
CROSS JOIN LATERAL regexp_split_to_table(lower(tb.primarytitle), '[^a-z0-9]+') AS w
WHERE w <> '' AND length(w) >= 2;
-- (A) PLOT -> wi  (from movie.omdb_extra.plot)
INSERT INTO movie.wi (tconst, word)
SELECT e.tconst, w
FROM movie.omdb_extra e
CROSS JOIN LATERAL regexp_split_to_table(lower(coalesce(e.plot,'')), '[^a-z0-9]+') AS w
WHERE w <> '' AND length(w) >= 2
ON CONFLICT DO NOTHING;

-- (B) CHARACTERS -> wi  (from movie.credit.characters)
INSERT INTO movie.wi (tconst, word)
SELECT c.tconst, w
FROM movie.credit c
CROSS JOIN LATERAL regexp_split_to_table(lower(coalesce(c.characters,'')), '[^a-z0-9]+') AS w
WHERE w <> '' AND length(w) >= 2
ON CONFLICT DO NOTHING;

-- (C) PERSON NAMES -> wi  (names of people in that title)
INSERT INTO movie.wi (tconst, word)
SELECT c.tconst, w
FROM movie.credit c
JOIN movie.person p ON p.nconst = c.nconst
CROSS JOIN LATERAL regexp_split_to_table(lower(coalesce(p.primary_name,'')), '[^a-z0-9]+') AS w
WHERE w <> '' AND length(w) >= 2
ON CONFLICT DO NOTHING;
\echo '--- END: WI PLOT ---'









