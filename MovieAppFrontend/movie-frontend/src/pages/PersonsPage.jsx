// src/pages/PersonsPage.jsx
import React, { useEffect, useState } from "react";
import {
  apiSearchPersons,
  apiBookmarkPerson,
  apiRatePerson,
  apiPostSearchHistory,
} from "../api";
import { useAuth } from "../AuthContext";
import { getPersonImageUrlByNconst } from "../tmdb";

// Helpers to normalize different backend property names
function getNconstFromRow(p) {
  return p.nconst ?? p.Nconst ?? p.nConst ?? p.NCONST ?? "";
}

function getNameFromRow(p) {
  return (
    p.primaryName ??
    p.primary_name ??
    p.primary_Name ??
    p.name ??
    p.Name ??
    ""
  );
}

function getBirthYearFromRow(p) {
  return (
    p.birthYear ??
    p.birth_year ??
    p.birth_Year ??
    p.Birth_Year ??
    p.birth ??
    ""
  );
}

function getDeathYearFromRow(p) {
  return (
    p.deathYear ??
    p.death_year ??
    p.death_Year ??
    p.Death_Year ??
    p.death ??
    ""
  );
}

export default function PersonsPage() {
  const { user } = useAuth();

  const [search, setSearch] = useState("");
  const [persons, setPersons] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const [selectedNconst, setSelectedNconst] = useState("");
  const [rating, setRating] = useState("");

  // Store person image URLs keyed by nconst
  const [personImages, setPersonImages] = useState({}); // { [nconst]: url }

  const selectedPerson =
    persons.find((p) => getNconstFromRow(p) === selectedNconst) || null;

  const handleDetails = (nconst) => {
    if (!nconst) return;
    setSelectedNconst(nconst);
    setRating("");

    setTimeout(() => {
      document.getElementById("person-details")?.scrollIntoView({
        behavior: "smooth",
        block: "start",
      });
    }, 50);
  };
  
  useEffect(() => {
    if (search.trim().length < 2) {
      setPersons([]);
      setPersonImages({});
      setError("");
      setSelectedNconst("");
      return;
    }

    let cancelled = false;

    async function load() {
      setLoading(true);
      setError("");

      try {
        const result = await apiSearchPersons(search.trim(), 1, 20);

        if (!cancelled) {
          setPersons(result || []);

          // save search history (ignore errors)
          if (user) {
            apiPostSearchHistory(user.id, search.trim(), "person").catch(
              () => {}
            );
          }
        }
      } catch (err) {
        console.error("Failed to fetch persons:", err);
        if (!cancelled) {
          setError("Failed to fetch persons");
          setPersons([]);
          setPersonImages({});
          setSelectedNconst("");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    load();
    return () => {
      cancelled = true;
    };
  }, [search, user]);

  // after persons list loads, fetch images from TMDB
  useEffect(() => {
    if (!persons || persons.length === 0) {
      setPersonImages({});
      return;
    }

    let cancelled = false;

    async function loadImages() {
      const map = {};

      await Promise.all(
        persons.map(async (p) => {
          const nconst = getNconstFromRow(p);
          if (!nconst) return;

          try {
            const url = await getPersonImageUrlByNconst(nconst);
            if (url) map[nconst] = url;
          } catch (e) {
            console.log("TMDB image failed for", nconst, e?.message || e);
          }
        })
      );

      if (!cancelled) setPersonImages(map);
    }

    loadImages();
    return () => {
      cancelled = true;
    };
  }, [persons]);

  const handleBookmark = async (nconst) => {
    if (!user) {
      alert("Please login to bookmark.");
      return;
    }

    try {
      await apiBookmarkPerson(nconst);
      alert("Person bookmarked!");
    } catch (err) {
      console.error("Bookmark person failed", err);
      alert("Failed to bookmark person.");
    }
  };

  const handleSaveRating = async () => {
    if (!user) {
      alert("Please login to rate.");
      return;
    }
    if (!selectedNconst) {
      alert("Open Details first.");
      return;
    }
    if (!rating) {
      alert("Enter a rating between 1 and 10.");
      return;
    }

    try {
      await apiRatePerson(user.id, selectedNconst, rating);
      alert("Rating saved!");
      setRating("");
    } catch (err) {
      console.error("Rate person failed", err);
      alert("Failed to save rating.");
    }
  };

  return (
    <div className="container mt-4 text-light">
      <h2>Persons</h2>

      <input
        className="form-control mb-3"
        placeholder="Type at least 2 letters to search persons…"
        value={search}
        onChange={(e) => setSearch(e.target.value)}
      />

      {loading && <div>Loading…</div>}
      {error && <div className="alert alert-danger">{error}</div>}

      {/* ✅ DETAILS AREA (shows ABOVE list, so it won't go below screen) */}
      {selectedPerson && (
        <div id="person-details" className="mb-4">
          <h4 className="mb-2">Person details</h4>

          <div className="person-card">
            <div className="person-left">
              {personImages[getNconstFromRow(selectedPerson)] ? (
                <img
                  src={personImages[getNconstFromRow(selectedPerson)]}
                  alt={getNameFromRow(selectedPerson)}
                  className="person-thumb"
                  style={{ width: "70px", height: "70px" }}
                />
              ) : (
                <div
                  className="person-thumb person-thumb--empty"
                  style={{ width: "70px", height: "70px" }}
                >
                  No image
                </div>
              )}

              <div className="person-info">
                <div className="person-name" style={{ fontSize: "1.1rem" }}>
                  {getNameFromRow(selectedPerson)}
                </div>

                <div className="person-meta">
                  {getNconstFromRow(selectedPerson)}
                </div>

                <div className="person-meta">
                  {getBirthYearFromRow(selectedPerson) || "?"} –{" "}
                  {getDeathYearFromRow(selectedPerson) || "?"}
                </div>
              </div>
            </div>
          </div>

          {user && (
            <div className="mt-3">
              <label className="me-2">Rate this person (1–10):</label>
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

      {/* ✅ Movie-like list/cards */}
      <div className="persons-list">
        {persons.length === 0 && !loading && !error && (
          <div className="text-center" style={{ opacity: 0.8 }}>
            No persons found for this query.
          </div>
        )}

        {persons.map((p) => {
          const nconst = getNconstFromRow(p);
          const name = getNameFromRow(p);
          const birth = getBirthYearFromRow(p);
          const death = getDeathYearFromRow(p);
          const img = personImages[nconst];

          return (
            <div className="person-card" key={nconst || name}>
              <div className="person-left">
                {img ? (
                  <img className="person-thumb" src={img} alt={name} />
                ) : (
                  <div className="person-thumb person-thumb--empty">
                    No image
                  </div>
                )}

                <div className="person-info">
                  <div className="person-name">{name}</div>
                  <div className="person-meta">
                    {nconst} • {birth || "?"} – {death || "?"}
                  </div>
                </div>
              </div>

              <div className="person-actions">
                <button
                  className="btn btn-sm btn-outline-info"
                  onClick={() => handleDetails(nconst)}
                >
                  Details
                </button>

                {user && (
                  <button
                    className="btn btn-sm btn-outline-warning"
                    onClick={() => handleBookmark(nconst)}
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
