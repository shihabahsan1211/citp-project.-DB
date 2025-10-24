--Testing--
--1D1--
--1D.1 step1 (create a test user)
SELECT framework.create_user('ama_1', 'ama_1@example.com', '123456');

--1D.1 step2 (Get the user id)
SELECT framework.get_user_id('ama_1') AS uid;

--sample title/person to bookmark
SELECT tconst FROM movie.title  LIMIT 1;      
SELECT nconst FROM movie.person LIMIT 1;   

--1D.1 step 3 (bookmark a title)
SELECT  framework.bookmark_title(framework.get_user_id('ama_1'), 'tt0052520');

-- step 3 and 7 check title bookmark or not!
SELECT * FROM framework.list_bookmarked_titles(framework.get_user_id('ama_1'));

--1D.1 step 4 remove title bookmark 
SELECT framework.unbookmark_title(framework.get_user_id('ama_1'), 'tt0052520');

-- check title bookmark remove or not!
SELECT * FROM framework.list_bookmarked_titles(framework.get_user_id('ama_1'));

--1D.1 step 5 Bookmark a people
SELECT framework.bookmark_person(framework.get_user_id('ama_1'), 'nm7823096');

--step 8 check people bookmark or not!
SELECT * FROM framework.list_bookmarked_people(framework.get_user_id('ama_1'));

--1D.1 step 6 remove people bookmark
SELECT framework.unbookmark_person(framework.get_user_id('ama_1'), 'nm7823096');

-- check  bookmark people remove or not!
SELECT * FROM framework.list_bookmarked_people(framework.get_user_id('ama_1'));

-- 1D step 9 Search history 

-- insert some search-history data manually for testing
INSERT INTO framework.user_search_history(user_id, query_text, performed_at)
VALUES 
  (framework.get_user_id('ama_1'), 'find sci-fi movies', now() - interval '2 day'),
  (framework.get_user_id('ama_1'), 'top horror movies',  now() - interval '1 hour');

-- check the search history
SELECT * FROM framework.get_search_history(framework.get_user_id('ama_1'));


--1D2--
--Simple title search
SELECT * 
FROM framework.string_search(framework.get_user_id('ama_1'), 'Matrix')
LIMIT 10;

--Another generic keyword (common across many titles)
SELECT * 
FROM framework.string_search(framework.get_user_id('ama_1'), 'love')
LIMIT 5;

--Case-insensitivity check
SELECT COUNT(*) AS results_lower
FROM framework.string_search(framework.get_user_id('ama_1'), 'matrix');

SELECT COUNT(*) AS results_upper
FROM framework.string_search(framework.get_user_id('ama_1'), 'MATRIX');

--plot search

SELECT t.tconst, t.primary_title, LEFT(o.plot, 200) AS plot_snippet
FROM movie.title t
JOIN movie.omdb_extra o USING (tconst)
WHERE o.plot ILIKE '%mission%'
ORDER BY t.primary_title
LIMIT 1;


SELECT * 
FROM framework.string_search(framework.get_user_id('ama_1'), 'mission')
LIMIT 10;

--Verify that every call logged into user_search_history (newest first)
SELECT user_id, query_text, performed_at
FROM framework.user_search_history
WHERE user_id = framework.get_user_id('ama_1')
ORDER BY performed_at DESC
LIMIT 10;

--D4--
-- Make sure we look in the right schemas
SET search_path = framework, movie, public;

-- Create a test user
INSERT INTO framework.app_user(username, email, password_hash)
VALUES ('mah1', 'd4@test.example', '2288')
ON CONFLICT (username) DO NOTHING;

SELECT framework.get_user_id('mah1') AS user_id;

--Title test (pick a random title token and search by title)
SELECT *
FROM framework.structured_string_search(
  framework.get_user_id('mah1'),
  (SELECT split_part(t.primary_title,' ',1)
     FROM movie.title t
     ORDER BY random() LIMIT 1),       -- p_title
  NULL,                                -- p_plot
  NULL,                                -- p_characters
  NULL                                 -- p_person_name
)
LIMIT 10;

--Plot test: grab a word from a random plot and search by plot
SELECT *
FROM framework.structured_string_search(
  framework.get_user_id('ama_1'),
  NULL,
  (SELECT w
     FROM (
        SELECT regexp_split_to_array(lower(o.plot),'[^a-z0-9]+') AS arr
        FROM movie.omdb_extra o
        WHERE o.plot IS NOT NULL
        ORDER BY random() LIMIT 1
     ) s
     CROSS JOIN LATERAL unnest(s.arr) AS w
     WHERE length(w) >= 5
     LIMIT 1),
  NULL,
  NULL
)
LIMIT 10;

--Verify logging (newest first)
SELECT user_id, query_text, performed_at
FROM framework.user_search_history
WHERE user_id = framework.get_user_id('ama_1')
ORDER BY performed_at DESC
LIMIT 10;


--D5
--Finding names
SELECT * 
FROM framework.find_names(
  framework.get_user_id('mah1'),  -- user_id
  'Tom'                            -- query string
);

--D6

-- Look up an actor's ID (example)
SELECT nconst, primary_name
FROM movie.person
WHERE primary_name ILIKE '%Hanks%';

-- Test the by-ID core function
SELECT * FROM movie.find_coplayers('nm0000158')  -- Tom Hanks nconst
LIMIT 15;

-- Test the by-name + logging function
SELECT *
FROM framework.find_coplayers_by_name(
  framework.get_user_id('ama_1'),
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

--D7

--using the movie schema
SET search_path = movie, public;

SELECT movie.refresh_all_name_ratings();

--Verify the data
SELECT *
FROM movie.name_rating
ORDER BY weighted_avg DESC
LIMIT 10;

-- Pick one known person
SELECT c.nconst, p.primary_name, COUNT(*), 
       SUM(tr.average_rating * tr.num_votes) / SUM(tr.num_votes) AS manual_avg,
       SUM(tr.num_votes) AS total_votes
FROM movie.credit c
JOIN movie.title_rating tr ON tr.tconst = c.tconst
JOIN movie.person p ON p.nconst = c.nconst
WHERE p.primary_name ILIKE '%Tom Hanks%'  
GROUP BY c.nconst, p.primary_name;

--Compare with name_rating table
SELECT *
FROM movie.name_rating nr
JOIN movie.person p ON nr.nconst = p.nconst
WHERE p.primary_name ILIKE '%Tom Hanks%';

--D8

-- Find a sample tconst
SELECT tconst, primary_title FROM movie.title
WHERE primary_title ILIKE '%Matrix%' LIMIT 1;

-- Then:
SELECT *
FROM framework.popular_cast_for_title(
  framework.get_user_id('ama_1'),
  'tt13773598',   
  15
);

--1D9

SELECT * FROM movie.similar_by_genre('tt0052520', 10);
--D-10--
-- default top 10
SELECT * FROM framework.person_words(1, 'Tom Hanks');

-- different limit
SELECT * FROM framework.person_words(1, 'Tom Hanks', 20);

-- case-insensitive & partial match
SELECT * FROM framework.person_words(1, 'tom hAn');

--D10
--Check that the wi table contains data (not empty)
SELECT COUNT(*) FROM movie.wi;
-- Verify that wi contains words from movie titles
SELECT COUNT(*)>0 AS title_words_present
FROM movie.wi wi
JOIN movie.title t ON t.tconst = wi.tconst
WHERE wi.word IN (SELECT lower(split_part(t.primary_title,' ',1)) FROM movie.title LIMIT 10);

----Verify that wi contains words from plot summaries (omdb_extra table)
SELECT COUNT(*)>0 AS plot_words_present
FROM movie.wi wi
JOIN movie.omdb_extra o ON o.tconst = wi.tconst;

---Verify that wi contains words from characters in movie.credit
SELECT COUNT(*)>0 AS character_words_present
FROM movie.wi wi
JOIN movie.credit c ON c.tconst = wi.tconst
WHERE c.characters IS NOT NULL;

---- Verify that wi contains words derived from person names
SELECT COUNT(*) > 0 AS person_name_words_present
FROM movie.wi wi
JOIN movie.credit c ON c.tconst = wi.tconst
JOIN movie.person p ON p.nconst = c.nconst;

---Test the person_words() function to check if it returns
-- the top 10 most frequent characteristic words for the person 'Tom Hanks'.
SELECT * FROM framework.person_words(1, 'Tom Hanks', 10);


--1D11

-- A: single keyword (should return titles containing "matrix")
SELECT * 
FROM framework.exact_match(framework.get_user_id('shibz'), 'matrix', 10);

-- B: multi-keyword exact match (both words must exist in the same title)
SELECT * 
FROM framework.exact_match(framework.get_user_id('shibz'), 'lord rings', 20);

-- C: another 2-word test
SELECT * 
FROM framework.exact_match(framework.get_user_id('shibz'), 'harry potter', 20);

--D12

-- A. Exact match: BOTH words must exist in the same title
SELECT * 
FROM framework.exact_match(framework.get_user_id('shibz'), 'lord rings', 15);

-- B. Best match: titles matching more words rank higher
SELECT *
FROM framework.best_match(framework.get_user_id('shibz'), 'lord rings king', 15);

-- C. Single-word queries should work in both
SELECT * FROM framework.exact_match(framework.get_user_id('shibz'), 'batman', 10);
SELECT * FROM framework.best_match (framework.get_user_id('shibz'), 'batman', 10);

--D13

SELECT * FROM movie.word_to_words('John wick', 10);
