import React, { useState } from "react";
import {
  BrowserRouter as Router,
  Routes,
  Route,
  Navigate,
  NavLink,
  useNavigate,
} from "react-router-dom";

import { AuthProvider, useAuth } from "./AuthContext";
import HomePage from "./pages/HomePage";
import MoviesPage from "./pages/MoviesPage";
import PersonsPage from "./pages/PersonsPage";
import BookmarksPage from "./pages/BookmarksPage";
import HistoryPage from "./pages/HistoryPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import RatingsPage from "./pages/RatingsPage";
import SearchPage from "./pages/SearchPage";

import Footer from "./pages/FooterPage";
import "./App.css";

/* Navbar */
function Navbar() {
  const { user, logout } = useAuth();
  const isAuthenticated = !!user;

  const navigate = useNavigate();
  const [q, setQ] = useState("");
  const [type, setType] = useState("all"); // all | movies | persons | tv

  const onSubmitSearch = (e) => {
    e.preventDefault();
    const query = q.trim();
    if (!query) return;

    navigate(
      `/search?q=${encodeURIComponent(query)}&type=${encodeURIComponent(type)}`
    );
  };

  const onLogout = () => logout();

  const getInitials = (name = "") => {
    if (!name) return "👤";
    const parts = name.trim().split(" ");
    if (parts.length === 1) return parts[0][0].toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  };

  return (
    <header className="app-header">
      <nav className="navbar glass-nav">
        <div className="navbar-top">
          <div className="navbar-left">
            <NavLink to="/" className="logo-wrapper">
              <span className="logo-text">
                Movie<span>Portfolio</span>
              </span>
            </NavLink>

            <div className="nav-links">
              <NavLink
                to="/movies"
                className={({ isActive }) =>
                  `nav-link ${isActive ? "nav-link-active" : ""}`
                }
              >
                Movies
              </NavLink>

              <NavLink
                to="/persons"
                className={({ isActive }) =>
                  `nav-link ${isActive ? "nav-link-active" : ""}`
                }
              >
                Persons
              </NavLink>
              {isAuthenticated && (
                <>
                  <NavLink
                    to="/bookmarks"
                    className={({ isActive }) =>
                      `nav-link ${isActive ? "nav-link-active" : ""}`
                    }
                  >
                    Bookmarks
                  </NavLink>

                  <NavLink
                    to="/ratings"
                    className={({ isActive }) =>
                      `nav-link ${isActive ? "nav-link-active" : ""}`
                    }
                  >
                    Ratings
                  </NavLink>

                  <NavLink
                    to="/history"
                    className={({ isActive }) =>
                      `nav-link ${isActive ? "nav-link-active" : ""}`
                    }
                  >
                    History
                  </NavLink>
                </>
              )}
            </div>
          </div>

          <div className="navbar-right">
            {isAuthenticated ? (
              <div className="user-area">
                <div className="user-chip">
                  <div className="avatar-circle">
                    {getInitials(user?.username)}
                  </div>

                  <div className="user-meta">
                    <span className="user-greeting">Welcome,</span>
                    <span className="user-name">{user?.username}</span>
                  </div>
                </div>

                <button onClick={onLogout} className="btn btn-outline">
                  Logout
                </button>
              </div>
            ) : (
              <div className="auth-buttons">
                <NavLink to="/login" className="btn btn-outline">
                  Login
                </NavLink>
                <NavLink to="/register" className="btn btn-primary">
                  Get Started
                </NavLink>
              </div>
            )}
          </div>
        </div>

        {/*Global Search */}
        <div className="navbar-bottom">
          <form className="nav-search" onSubmit={onSubmitSearch}>
            <select
              className="nav-search-select"
              value={type}
              onChange={(e) => setType(e.target.value)}
              aria-label="Search type"
            >
              <option value="all">All</option>
              <option value="movies">Movies</option>
              <option value="persons">Persons</option>
            </select>

            <input
              className="nav-search-input"
              placeholder="Search movies, persons"
              value={q}
              onChange={(e) => setQ(e.target.value)}
            />

            <button className="nav-search-btn" type="submit">
              Search
            </button>
          </form>
        </div>
      </nav>
    </header>
  );
}

/* Route Guards*/
function ProtectedRoute({ children }) {
  const { user } = useAuth();
  return user ? children : <Navigate to="/login" replace />;
}

function PublicOnlyRoute({ children }) {
  const { user } = useAuth();
  return user ? <Navigate to="/" replace /> : children;
}

/*App*/
export default function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="app-root gradient-bg">
          <Navbar />

          <main className="main-content">
            <div className="main-shell">
              <Routes>
                {/*  PUBLIC ROUTES */}
                <Route path="/" element={<HomePage />} />
                <Route path="/movies" element={<MoviesPage />} />
                <Route path="/persons" element={<PersonsPage />} />
                <Route path="/search" element={<SearchPage />} />

                {/* PROTECTED ROUTES */}
                <Route
                  path="/bookmarks"
                  element={
                    <ProtectedRoute>
                      <BookmarksPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/ratings"
                  element={
                    <ProtectedRoute>
                      <RatingsPage />
                    </ProtectedRoute>
                  }
                />

                <Route
                  path="/history"
                  element={
                    <ProtectedRoute>
                      <HistoryPage />
                    </ProtectedRoute>
                  }
                />

                {/*  AUTH ROUTES */}
                <Route
                  path="/login"
                  element={
                    <PublicOnlyRoute>
                      <LoginPage />
                    </PublicOnlyRoute>
                  }
                />

                <Route
                  path="/register"
                  element={
                    <PublicOnlyRoute>
                      <RegisterPage />
                    </PublicOnlyRoute>
                  }
                />

                <Route path="*" element={<Navigate to="/" replace />} />
              </Routes>
            </div>
          </main>

          <Footer />
        </div>
      </Router>
    </AuthProvider>
  );
}
