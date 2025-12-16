import React, { useEffect, useMemo, useState } from "react";
import { useSearchParams, useNavigate } from "react-router-dom";
import { apiSearchTitles, apiSearchPersons } from "../api";

export default function SearchPage() {
  const [params] = useSearchParams();
  const navigate = useNavigate();

  const q = (params.get("q") || "").trim();
  const type = (params.get("type") || "all").toLowerCase(); // all | movies | persons

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [movies, setMovies] = useState([]);
  const [persons, setPersons] = useState([]);

  const showMovies = type === "all" || type === "movies";
  const showPersons = type === "all" || type === "persons";

  const title = useMemo(() => {
    if (!q) return "Search";
    const label =
      type === "movies" ? "Movies" : type === "persons" ? "Persons" : "All";
    return `Search: "${q}" (${label})`;
  }, [q, type]);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setError("");
      setMovies([]);
      setPersons([]);

      if (!q || q.length < 2) return;

      try {
        setLoading(true);

        const tasks = [];

        if (showMovies) {
          tasks.push(
            apiSearchTitles(q, 1, 10).then((res) => {
              if (!cancelled) setMovies(res || []);
            })
          );
        }

        if (showPersons) {
          tasks.push(
            apiSearchPersons(q, 1, 10).then((res) => {
              if (!cancelled) setPersons(res || []);
            })
          );
        }

        await Promise.all(tasks);
      } catch (e) {
        console.error(e);
        if (!cancelled) setError("Search failed. Please try again.");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [q, showMovies, showPersons]);

  const goToMovies = () => navigate("/movies");
  const goToPersons = () => navigate("/persons");

  return (
    <div className="page-container">
      <h2>{title}</h2>

      {!q && (
        <div style={{ opacity: 0.8, marginTop: 8 }}>
          Type something in the search bar above.
        </div>
      )}

      {q && q.length < 2 && (
        <div style={{ opacity: 0.8, marginTop: 8 }}>
          Please type at least 2 characters.
        </div>
      )}

      {loading && <p>Loading…</p>}
      {error && <div className="error-banner">{error}</div>}

      {/* Movies */}
      {showMovies && q.length >= 2 && (
        <div style={{ marginTop: 18 }}>
          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <h3 style={{ margin: 0 }}>Movies</h3>
            <button className="btn btn-outline" onClick={goToMovies}>
              Open full Movies page
            </button>
          </div>

          <div style={{ marginTop: 10 }}>
            {movies.length === 0 && !loading ? (
              <p style={{ opacity: 0.8 }}>No movies found.</p>
            ) : (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Tconst</th>
                    <th>Title</th>
                    <th>Year</th>
                  </tr>
                </thead>
                <tbody>
                  {movies.map((m) => (
                    <tr key={m.tconst ?? m.Tconst}>
                      <td>{m.tconst ?? m.Tconst}</td>
                      <td>{m.primaryTitle ?? m.title ?? ""}</td>
                      <td>{m.startYear ?? m.year ?? ""}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}

      {/* Persons */}
      {showPersons && q.length >= 2 && (
        <div style={{ marginTop: 22 }}>
          <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
            <h3 style={{ margin: 0 }}>Persons</h3>
            <button className="btn btn-outline" onClick={goToPersons}>
              Open full Persons page
            </button>
          </div>

          <div style={{ marginTop: 10 }}>
            {persons.length === 0 && !loading ? (
              <p style={{ opacity: 0.8 }}>No persons found.</p>
            ) : (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Nconst</th>
                    <th>Name</th>
                    <th>Birth</th>
                    <th>Death</th>
                  </tr>
                </thead>
                <tbody>
                  {persons.map((p) => (
                    <tr key={p.nconst ?? p.Nconst}>
                      <td>{p.nconst ?? p.Nconst}</td>
                      <td>{p.primaryName ?? p.name ?? ""}</td>
                      <td>{p.birthYear ?? p.birth_year ?? ""}</td>
                      <td>{p.deathYear ?? p.death_year ?? ""}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
