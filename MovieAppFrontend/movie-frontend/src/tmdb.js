const TMDB_API_KEY = process.env.REACT_APP_TMDB_API_KEY;
const TMDB_IMG_BASE = "https://image.tmdb.org/t/p/w185";

export async function getPersonImageUrlByNconst(nconst) {
  if (!TMDB_API_KEY) {
    console.error("TMDB API key missing. Add REACT_APP_TMDB_API_KEY in .env");
    return null;
  }

  try {
    const url = `https://api.themoviedb.org/3/find/${encodeURIComponent(
      nconst
    )}?external_source=imdb_id&api_key=${TMDB_API_KEY}`;

    const res = await fetch(url);
    if (!res.ok) return null;

    const data = await res.json();
    const person = data?.person_results?.[0];

    if (!person?.profile_path) return null;

    return `${TMDB_IMG_BASE}${person.profile_path}`;
  } catch (e) {
    console.error("TMDB person fetch failed:", e);
    return null;
  }
}
