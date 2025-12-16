// src/api.js
const API_BASE_URL = "https://localhost:7034";

// Helper: read token safely
function getToken() {
  try {
    return localStorage.getItem("movieToken") || "";
  } catch {
    return "";
  }
}

/**
 * Helper: fetch + JSON + error handling + auth header
 */
async function request(pathOrUrl, options = {}) {
  const url = pathOrUrl.startsWith("http")
    ? pathOrUrl
    : `${API_BASE_URL}${pathOrUrl.startsWith("/") ? "" : "/"}${pathOrUrl}`;

  const token = getToken();

  const headers = {
    ...(options.headers || {}),
    "Content-Type": "application/json",
  };

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(url, {
    ...options,
    headers,
  });

  if (response.status === 401) {
    try {
      localStorage.removeItem("movieToken");
      localStorage.removeItem("movieUser");
    } catch {}
  }

  if (!response.ok) {
    let message = `HTTP ${response.status}`;
    try {
      const errBody = await response.json();
      message = errBody?.message || errBody?.error || errBody?.title || message;
    } catch {}
    throw new Error(message);
  }

  if (response.status === 204) return null;

  const text = await response.text();
  if (!text) return null;

  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

function extractItems(data) {
  if (!data) return [];
  if (Array.isArray(data)) return data;

  if (Array.isArray(data.items)) return data.items;
  if (Array.isArray(data.results)) return data.results;
  if (Array.isArray(data.titles)) return data.titles;
  if (Array.isArray(data.persons)) return data.persons;
  if (Array.isArray(data.data)) return data.data;

  return [];
}

//AUTH

export async function apiRegister({ username, email, password }) {
  const data = await request("/api/users/register", {
    method: "POST",
    body: JSON.stringify({ username, email, password }),
  });

  const u = data?.user ?? data ?? {};
  return {
    id: u.id ?? u.userId ?? data?.id ?? data?.userId ?? null,
    username: u.username ?? username,
    email: u.email ?? email ?? "",
  };
}

export async function apiLogin({ username, password }) {
  const data = await request("/api/users/login", {
    method: "POST",
    body: JSON.stringify({ username, password }),
  });

  const token =
    data?.token || data?.jwt || data?.accessToken || data?.access_token || "";

  const user = data?.user ?? null;

  return { token, user };
}

export async function apiSearchTitles(searchText, page = 1, pageSize = 10) {
  const urlObj = new URL(`${API_BASE_URL}/api/titles`);
  if (searchText && searchText.trim() !== "") {
    urlObj.searchParams.set("q", searchText.trim());
  }
  urlObj.searchParams.set("page", String(page));
  urlObj.searchParams.set("pageSize", String(pageSize));

  const data = await request(urlObj.toString());
  return extractItems(data);
}

export async function apiGetTitle(tconst) {
  return request(`/api/titles/${encodeURIComponent(tconst)}`);
}

export async function apiSearchPersons(searchText, page = 1, pageSize = 10) {
  const urlObj = new URL(`${API_BASE_URL}/api/persons`);
  if (searchText && searchText.trim() !== "") {
    urlObj.searchParams.set("q", searchText.trim());
  }
  urlObj.searchParams.set("page", String(page));
  urlObj.searchParams.set("pageSize", String(pageSize));

  const data = await request(urlObj.toString());
  return extractItems(data);
}

export async function apiGetPerson(nconst) {
  return request(`/api/persons/${encodeURIComponent(nconst)}`);
}


export async function apiGetSearchHistory(userId) {
  const data = await request(
    `/api/users/${encodeURIComponent(userId)}/search-history`
  );
  return extractItems(data);
}

// Updated to match backend API contract (Swagger)

export async function apiPostSearchHistory(userId, term) {
  return request(`/api/users/${encodeURIComponent(userId)}/search-history`, {
    method: "POST",
    body: JSON.stringify({
      queryText: term,
      searchedOn: new Date().toISOString(),
    }),
  });
}

export async function apiDeleteSearchHistoryItem(userId, historyId) {
  return request(
    `/api/users/${encodeURIComponent(userId)}/search-history/${encodeURIComponent(
      historyId
    )}`,
    { method: "DELETE" }
  );
}

export async function apiClearSearchHistory(userId) {
  return request(`/api/users/${encodeURIComponent(userId)}/search-history`, {
    method: "DELETE",
  });
}


export async function apiBookmarkTitle(tconst) {
  return request(`/api/bookmarks/titles/${encodeURIComponent(tconst)}`, {
    method: "POST",
  });
}

export async function apiGetBookmarkedTitles() {
  const data = await request(`/api/bookmarks/titles`);
  return extractItems(data);
}

export async function apiBookmarkPerson(nconst) {
  return request(`/api/bookmarks/persons/${encodeURIComponent(nconst)}`, {
    method: "POST",
  });
}

export async function apiGetBookmarkedPersons() {
  const data = await request(`/api/bookmarks/persons`);
  return extractItems(data);
}

export async function apiRateTitle(userId, tconst, rating) {
  return request(`/api/users/${encodeURIComponent(userId)}/ratings/titles`, {
    method: "POST",
    body: JSON.stringify({ tconst, rating: Number(rating) }),
  });
}

export async function apiGetTitleRatings(userId) {
  const data = await request(
    `/api/users/${encodeURIComponent(userId)}/ratings/titles`
  );
  return extractItems(data);
}

export async function apiRatePerson(userId, nconst, rating) {
  return request(`/api/users/${encodeURIComponent(userId)}/ratings/persons`, {
    method: "POST",
    body: JSON.stringify({ nconst, rating: Number(rating) }),
  });
}

export async function apiGetPersonRatings(userId) {
  const data = await request(
    `/api/users/${encodeURIComponent(userId)}/ratings/persons`
  );
  return extractItems(data);
}
