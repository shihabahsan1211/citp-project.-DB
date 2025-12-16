// src/pages/RatingsPage.jsx
import React, { useEffect, useState } from "react";
import { useAuth } from "../AuthContext";
import {
  apiGetTitleRatings,
  apiGetPersonRatings,
  apiGetTitle,
  apiGetPerson,
} from "../api";

export default function RatingsPage() {
  const { user } = useAuth();

  const [titleRatings, setTitleRatings] = useState([]);
  const [personRatings, setPersonRatings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Helper to normalize rating objects from backend (snake_case, PascalCase, etc.)
  const normalizeTitleRating = (r) => ({
    tconst: r.tconst ?? r.Tconst ?? r.titleId ?? r.TitleId ?? "",
    rating: r.rating ?? r.Rating ?? r.value ?? r.Value ?? null,
  });

  const normalizePersonRating = (r) => ({
    nconst: r.nconst ?? r.Nconst ?? r.personId ?? r.PersonId ?? "",
    rating: r.rating ?? r.Rating ?? r.value ?? r.Value ?? null,
  });

  useEffect(() => {
    if (!user?.id) return;

    let cancelled = false;

    async function load() {
      setLoading(true);
      setError("");

      try {
        // get rating rows
        const [rawTitles, rawPersons] = await Promise.all([
          apiGetTitleRatings(user.id),
          apiGetPersonRatings(user.id),
        ]);

        const tList = (rawTitles || []).map(normalizeTitleRating).filter(x => x.tconst);
        const pList = (rawPersons || []).map(normalizePersonRating).filter(x => x.nconst);

        //fetch names for nicer UI (optional but recommended)
        const [titleDetails, personDetails] = await Promise.all([
          Promise.all(
            tList.map(async (x) => {
              try {
                const t = await apiGetTitle(x.tconst);
                return {
                  ...x,
                  title: t?.primaryTitle ?? t?.title ?? x.tconst,
                  year: t?.startYear ?? t?.year ?? "",
                };
              } catch {
                return { ...x, title: x.tconst, year: "" };
              }
            })
          ),
          Promise.all(
            pList.map(async (x) => {
              try {
                const p = await apiGetPerson(x.nconst);
                return {
                  ...x,
                  name: p?.primaryName ?? p?.name ?? x.nconst,
                };
              } catch {
                return { ...x, name: x.nconst };
              }
            })
          ),
        ]);

        if (!cancelled) {
          setTitleRatings(titleDetails);
          setPersonRatings(personDetails);
        }
      } catch (e) {
        console.error(e);
        if (!cancelled) setError(e?.message || "Failed to load ratings");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [user?.id]);

  if (!user) {
    return (
      <div className="container mt-4 text-light">
        <h2>Ratings</h2>
        <div>Please login to see your ratings.</div>
      </div>
    );
  }

  return (
    <div className="container mt-4 text-light">
      <h2>Ratings</h2>

      {loading && <div>Loading…</div>}
      {error && <div className="alert alert-danger">{error}</div>}

      <h4 className="mt-3">Movie ratings</h4>
      <table className="table table-dark table-striped align-middle">
        <thead>
          <tr>
            <th>Tconst</th>
            <th>Title</th>
            <th>Year</th>
            <th>Rating</th>
          </tr>
        </thead>
        <tbody>
          {titleRatings.length === 0 && !loading && (
            <tr>
              <td colSpan="4" className="text-center">
                No movie ratings yet.
              </td>
            </tr>
          )}

          {titleRatings.map((x) => (
            <tr key={x.tconst}>
              <td>{x.tconst}</td>
              <td>{x.title}</td>
              <td>{x.year}</td>
              <td>{x.rating}</td>
            </tr>
          ))}
        </tbody>
      </table>

      <h4 className="mt-4">Person ratings</h4>
      <table className="table table-dark table-striped align-middle">
        <thead>
          <tr>
            <th>Nconst</th>
            <th>Name</th>
            <th>Rating</th>
          </tr>
        </thead>
        <tbody>
          {personRatings.length === 0 && !loading && (
            <tr>
              <td colSpan="3" className="text-center">
                No person ratings yet.
              </td>
            </tr>
          )}

          {personRatings.map((x) => (
            <tr key={x.nconst}>
              <td>{x.nconst}</td>
              <td>{x.name}</td>
              <td>{x.rating}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
