import React, { useEffect, useState } from "react";
import {
  apiGetBookmarkedTitles,
  apiGetBookmarkedPersons,
  apiGetTitle,
  apiGetPerson,
} from "../api";
import { useAuth } from "../AuthContext";

export default function BookmarksPage() {
  const { user } = useAuth();
  const [titles, setTitles] = useState([]);
  const [persons, setPersons] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!user?.id) return;

    let cancelled = false;

    async function load() {
      try {
        setLoading(true);
        setError("");

        // get raw bookmark ids
        const [titleBookmarks, personBookmarks] = await Promise.all([
          apiGetBookmarkedTitles(),
          apiGetBookmarkedPersons(),
        ]);

        // Normalize IDs (handle casing differences)
        const tconsts = (titleBookmarks || [])
          .map((x) => x.tconst ?? x.Tconst)
          .filter(Boolean);

        const nconsts = (personBookmarks || [])
          .map((x) => x.nconst ?? x.Nconst)
          .filter(Boolean);
        const [titleDetails, personDetails] = await Promise.all([
          Promise.all(tconsts.map((id) => apiGetTitle(id).catch(() => null))),
          Promise.all(nconsts.map((id) => apiGetPerson(id).catch(() => null))),
        ]);

        if (cancelled) return;

        setTitles(titleDetails.filter(Boolean));
        setPersons(personDetails.filter(Boolean));
      } catch (e) {
        console.error("Failed to load bookmarks:", e);
        if (!cancelled) setError("Failed to load bookmarks");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [user]);

  if (!user) {
    return (
      <div className="page-container">
        <h2>Bookmarks</h2>
        <p>Please login to see your bookmarks.</p>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h2>Bookmarks</h2>

      {loading && <p>Loading…</p>}
      {error && <div className="error-banner">{error}</div>}

      <h3 style={{ marginTop: 20 }}>Titles</h3>
      <table className="data-table">
        <thead>
          <tr>
            <th>Tconst</th>
            <th>Title</th>
            <th>Year</th>
          </tr>
        </thead>
        <tbody>
          {titles.length === 0 ? (
            <tr>
              <td colSpan="3">No bookmarked titles yet.</td>
            </tr>
          ) : (
            titles.map((t) => (
              <tr key={t.tconst}>
                <td>{t.tconst}</td>
                <td>{t.primaryTitle ?? t.title}</td>
                <td>{t.startYear ?? t.year ?? ""}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>

      <h3 style={{ marginTop: 28 }}>Persons</h3>
      <table className="data-table">
        <thead>
          <tr>
            <th>Nconst</th>
            <th>Name</th>
          </tr>
        </thead>
        <tbody>
          {persons.length === 0 ? (
            <tr>
              <td colSpan="2">No bookmarked persons yet.</td>
            </tr>
          ) : (
            persons.map((p) => (
              <tr key={p.nconst}>
                <td>{p.nconst}</td>
                <td>{p.primaryName ?? p.name}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
