// src/pages/HistoryPage.jsx
import React, { useEffect, useState } from "react";
import { useAuth } from "../AuthContext";
import {
  apiGetSearchHistory,
  apiDeleteSearchHistoryItem,
  apiClearSearchHistory,
} from "../api";

export default function HistoryPage() {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!user) {
      setLoading(false);
      return;
    }

    (async () => {
      try {
        const data = await apiGetSearchHistory(user.id);
        setItems(data);
      } catch (err) {
        console.error("Failed to load history:", err);
        setError(err.message ?? "Failed to load history");
      } finally {
        setLoading(false);
      }
    })();
  }, [user]);

  const deleteOne = async (id) => {
    if (!user) return;
    if (!window.confirm("Delete this search from history?")) return;

    setBusy(true);
    try {
      await apiDeleteSearchHistoryItem(user.id, id);
      setItems((prev) => prev.filter((x) => x.id !== id));
    } catch (err) {
      console.error("Failed to delete history item:", err);
      alert(err.message ?? "Failed to delete");
    } finally {
      setBusy(false);
    }
  };

  const clearAll = async () => {
    if (!user) return;
    if (!window.confirm("Clear ALL search history?")) return;

    setBusy(true);
    try {
      await apiClearSearchHistory(user.id);
      setItems([]);
    } catch (err) {
      console.error("Failed to clear history:", err);
      alert(err.message ?? "Failed to clear history");
    } finally {
      setBusy(false);
    }
  };

  if (!user) {
    return (
      <div className="container mt-4 text-light">
        <h1>Search history</h1>
        <p>Please log in to see your search history.</p>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container mt-4 text-light">
        <h1>Search history</h1>
        <p>Loading…</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mt-4 text-light">
        <h1>Search history</h1>
        <p>Failed to load history: {error}</p>
      </div>
    );
  }

  return (
    <div className="container mt-4 text-light">
      <div className="d-flex align-items-center justify-content-between">
        <h1 className="mb-0">Search history</h1>

        <button
          className="btn btn-danger"
          onClick={clearAll}
          disabled={busy || items.length === 0}
        >
          Clear all
        </button>
      </div>

      {(!items || items.length === 0) ? (
        <p className="mt-3">No search history yet.</p>
      ) : (
        <table className="table table-dark table-striped mt-3">
          <thead>
            <tr>
              <th>When</th>
              <th>Query</th>
              <th style={{ width: 120 }}>Action</th>
            </tr>
          </thead>
          <tbody>
            {items.map((h) => (
              <tr key={h.id}>
                <td>{new Date(h.searchedOn).toLocaleString()}</td>
                <td>{h.queryText}</td>
                <td>
                  <button
                    className="btn btn-sm btn-outline-light"
                    onClick={() => deleteOne(h.id)}
                    disabled={busy}
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
