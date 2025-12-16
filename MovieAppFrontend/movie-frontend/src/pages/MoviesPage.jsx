import React, { useState, useEffect } from "react";
import { useAuth } from "../AuthContext";
import {
  apiSearchTitles,
  apiBookmarkTitle,
  apiRateTitle,
  apiPostSearchHistory,
} from "../api";

function MoviesPage() {
  const { user } = useAuth();

  const [searchTerm, setSearchTerm] = useState("");
  const [movies, setMovies] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [selectedTconst, setSelectedTconst] = useState("");
  const [rating, setRating] = useState("");

  const selectedMovie =
    movies.find((m) => (m.tconst ?? m.Tconst) === selectedTconst) || null;

  useEffect(() => {
    const term = searchTerm.trim();

    if (!term) {
      setMovies([]);
      setError("");
      setSelectedTconst("");
      return;
    }

    let cancelled = false;

    async function load() {
      try {
        setLoading(true);
        setError("");

        const data = await apiSearchTitles(term);

        if (cancelled) return;

        setMovies(data || []);

        // Save search history (best effort; ignore failures)
        if (user?.id) {
          apiPostSearchHistory(user.id, term).catch(() => {});
        }
      } catch (err) {
        console.error(err);
        if (!cancelled) setError("Failed to fetch titles");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [searchTerm, user]);

  const handleDetails = (tconst) => {
    if (!tconst) return;
    setSelectedTconst(tconst);
    setRating("");

    // scroll to details
    setTimeout(() => {
      document.getElementById("movie-details")?.scrollIntoView({
        behavior: "smooth",
        block: "start",
      });
    }, 50);
  };

  const handleBookmark = async (tconst) => {
    if (!user) {
      alert("Please log in to bookmark movies.");
      return;
    }
    try {
      await apiBookmarkTitle(tconst);
      alert("Movie bookmarked!");
    } catch (err) {
      console.error(err);
      alert("Failed to bookmark movie.");
    }
  };

  const handleSaveRating = async () => {
    if (!user) {
      alert("Please log in to rate movies.");
      return;
    }
    if (!selectedMovie) {
      alert("Open Details first.");
      return;
    }

    const value = parseInt(rating, 10);
    if (isNaN(value) || value < 1 || value > 10) {
      alert("Please enter a rating between 1 and 10.");
      return;
    }

    try {
      await apiRateTitle(user.id, selectedMovie.tconst ?? selectedMovie.Tconst, value);
      alert("Rating saved!");
      setRating("");
    } catch (err) {
      console.error(err);
      alert("Failed to save rating.");
    }
  };

  return (
    <div className="page-container">
      <h1>Movies</h1>

      <input
        type="text"
        placeholder="Search movies..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        className="search-input"
      />

      {loading && <p>Loading…</p>}
      {error && <div className="error-banner">{error}</div>}

      {movies.length === 0 && !loading && !error && searchTerm.trim() !== "" && (
        <p>No movies found for this search.</p>
      )}

      {selectedMovie && (
        <div id="movie-details" className="mb-4">
          <h4 className="mb-2">Movie details</h4>

          <div className="movie-card">
            <div className="movie-left">
              <div className="movie-title">{selectedMovie.primaryTitle}</div>
              <div className="movie-meta">
                {(selectedMovie.tconst ?? selectedMovie.Tconst)} • Year:{" "}
                {selectedMovie.year ?? selectedMovie.startYear ?? "Unknown"}
              </div>
            </div>

            <div className="movie-actions">
              {user && (
                <button
                  className="btn btn-sm btn-outline-warning"
                  onClick={() =>
                    handleBookmark(selectedMovie.tconst ?? selectedMovie.Tconst)
                  }
                >
                  Bookmark
                </button>
              )}
            </div>
          </div>

          {user && (
            <div className="mt-3">
              <label className="me-2">Rate this movie (1–10):</label>
              <input
                type="number"
                min="1"
                max="10"
                value={rating}
                onChange={(e) => setRating(e.target.value)}
                style={{ width: "90px", marginRight: "10px" }}
              />
              <button className="btn btn-primary" onClick={handleSaveRating}>
                Save rating
              </button>
            </div>
          )}
        </div>
      )}
      
      <div className="movies-list">
        {movies.map((movie) => {
          const tconst = movie.tconst ?? movie.Tconst;
          const year = movie.year ?? movie.startYear ?? "Unknown";

          return (
            <div key={tconst} className="movie-card">
              <div className="movie-left">
                <div className="movie-title">{movie.primaryTitle}</div>
                <div className="movie-meta">
                  Year: {year}
                </div>
              </div>

              <div className="movie-actions">
                <button
                  className="btn btn-sm btn-outline-info"
                  onClick={() => handleDetails(tconst)}
                >
                  Details
                </button>

                {user && (
                  <button
                    className="btn btn-sm btn-outline-warning"
                    onClick={() => handleBookmark(tconst)}
                  >
                    Bookmark
                  </button>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

export default MoviesPage;
