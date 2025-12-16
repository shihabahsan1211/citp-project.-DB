import React from "react";

function HomePage() {
  return (
    <div className="home-wrapper">
      {/* Dark overlay to improve text readability over the background */}
      <div className="home-overlay" />

      <main className="home-content">
        {/* Left section: hero content */}
        <section className="home-hero">
          <h1 className="home-title">Welcome to MoviePortfolio</h1>

          <p className="home-tagline">
            Search IMDb titles and people, bookmark favourites, and track your
            activity in one place.
          </p>

          <div className="home-stats" role="list">
            <div className="home-stat" role="listitem">
              <span className="home-stat-number">Search</span>
              <span className="home-stat-label">
                IMDb titles &amp; persons
              </span>
            </div>

            <div className="home-stat" role="listitem">
              <span className="home-stat-number">Bookmark</span>
              <span className="home-stat-label">Keep a watchlist</span>
            </div>

            <div className="home-stat" role="listitem">
              <span className="home-stat-number">Rate</span>
              <span className="home-stat-label">
                Remember your favourites
              </span>
            </div>
          </div>
        </section>

      {/* right side: poster grid */}
        <section className="home-posters">
          <div className="poster-card poster-1" />
          <div className="poster-card poster-2" />
          <div className="poster-card poster-3" />
          <div className="poster-card poster-4" />
          <div className="poster-card poster-5" />
          <div className="poster-card poster-6" />
        </section>
      </main>
    </div>
  );
}

export default HomePage;
