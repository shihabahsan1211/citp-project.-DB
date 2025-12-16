import React, { createContext, useContext, useEffect, useState } from "react";
import { apiLogin, apiRegister } from "./api";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    try {
      const stored = localStorage.getItem("movieUser");
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  });

  const [token, setToken] = useState(() => {
    return localStorage.getItem("movieToken") || "";
  });

  const [loading, setLoading] = useState(false);

  // persist user
  useEffect(() => {
    if (user) localStorage.setItem("movieUser", JSON.stringify(user));
    else localStorage.removeItem("movieUser");
  }, [user]);

  // persist token
  useEffect(() => {
    if (token) localStorage.setItem("movieToken", token);
    else localStorage.removeItem("movieToken");
  }, [token]);

  const login = async ({ username, password }) => {
    setLoading(true);
    try {
      const data = await apiLogin({ username, password });

     // Expected backend response:
// { token, user: { id, username, email } }
      const jwt = data?.token || "";
      const u = data?.user || null;

      if (!jwt) throw new Error("Token missing from server response");
      if (!u?.id) throw new Error("User id missing from server response");

      setToken(jwt);
      setUser({
        id: u.id,
        username: u.username ?? username,
        email: u.email ?? "",
      });

      return { ok: true };
    } catch (err) {
      console.error("Login failed:", err);
      setToken("");
      setUser(null);
      return { ok: false, error: err?.message || "Login failed" };
    } finally {
      setLoading(false);
    }
  };

  const register = async ({ username, email, password }) => {
    setLoading(true);
    try {
      await apiRegister({ username, email, password });
      return { ok: true };
    } catch (err) {
      console.error("Register failed:", err);
      return { ok: false, error: err?.message || "Register failed" };
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    setToken("");
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        loading,
        login,
        register,
        logout,
        isAuthenticated: !!token && !!user?.id,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
}
