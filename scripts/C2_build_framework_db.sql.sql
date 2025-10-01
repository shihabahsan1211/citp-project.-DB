-- STEP 0: framework schema + search path
CREATE SCHEMA IF NOT EXISTS framework;
SET search_path = framework, movie, public;

-- STEP 1: Users of the application
CREATE TABLE IF NOT EXISTS app_user (
    user_id       BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username      TEXT NOT NULL UNIQUE,
    email         TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    CONSTRAINT email_has_at CHECK (position('@' in email) > 1)
);
-- STEP 2: Per-user search history
CREATE TABLE IF NOT EXISTS user_search_history (
    search_id   BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id     BIGINT NOT NULL REFERENCES app_user(user_id) ON DELETE CASCADE,
    query_text  TEXT NOT NULL,
    performed_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- STEP 3A: Rating history for titles
CREATE TABLE IF NOT EXISTS user_title_rating (
    rating_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id   BIGINT NOT NULL REFERENCES app_user(user_id) ON DELETE CASCADE,
    tconst    TEXT   NOT NULL REFERENCES movie.title(tconst) ON DELETE CASCADE,
    rating    SMALLINT NOT NULL,
    rated_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT chk_title_rating_range CHECK (rating BETWEEN 1 AND 10)
);
-- STEP 3B: Rating history for people (names)
CREATE TABLE IF NOT EXISTS user_person_rating (
    rating_id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id   BIGINT NOT NULL REFERENCES app_user(user_id) ON DELETE CASCADE,
    nconst    TEXT   NOT NULL REFERENCES movie.person(nconst) ON DELETE CASCADE,
    rating    SMALLINT NOT NULL,
    rated_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT chk_person_rating_range CHECK (rating BETWEEN 1 AND 10)
);
-- STEP 4A: Bookmarks for titles (minimal)
CREATE TABLE IF NOT EXISTS user_bookmark_title (
    user_id BIGINT NOT NULL REFERENCES app_user(user_id) ON DELETE CASCADE,
    tconst  TEXT   NOT NULL REFERENCES movie.title(tconst) ON DELETE CASCADE,
    PRIMARY KEY (user_id, tconst)
);
-- STEP 4B: Bookmarks for people (minimal)
CREATE TABLE IF NOT EXISTS user_bookmark_person (
    user_id BIGINT NOT NULL REFERENCES app_user(user_id) ON DELETE CASCADE,
    nconst  TEXT   NOT NULL REFERENCES movie.person(nconst) ON DELETE CASCADE,
    PRIMARY KEY (user_id, nconst)
);


-- 5A.1: Safe insert (won't error if 'shibz' exists) 
INSERT INTO app_user (username, email, password_hash)
VALUES ('shibz', 'shibz@example.com', 'pw_hash_123')
ON CONFLICT (username) DO NOTHING;

-- 5A.2: Get the user_id we’ll use for all tests
SELECT user_id FROM app_user WHERE username = 'shibz';

-- 5B: add one search for user 1
INSERT INTO user_search_history (user_id, query_text)
VALUES (1, 'inception')
RETURNING *;

-- 5C: rate one title for user 1 (pick first title)
INSERT INTO user_title_rating (user_id, tconst, rating)
SELECT 1, t.tconst, 9
FROM movie.title t
LIMIT 1
RETURNING *;
-- 5D: rate one person for user 1 (pick first person)
INSERT INTO user_person_rating (user_id, nconst, rating)
SELECT 1, p.nconst, 7
FROM movie.person p
LIMIT 1
RETURNING *;

