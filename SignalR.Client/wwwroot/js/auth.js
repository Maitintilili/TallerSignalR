"use strict";

// Manejo de la sesión en el navegador (el token JWT se guarda en localStorage)
const Auth = {
    tokenKey: "chat_token",
    userKey: "chat_user",
    expiresKey: "chat_expires",

    save(response) {
        localStorage.setItem(this.tokenKey, response.token);
        localStorage.setItem(this.userKey, response.username);
        localStorage.setItem(this.expiresKey, response.expiresAt);
    },

    get token() {
        return localStorage.getItem(this.tokenKey);
    },

    get user() {
        return localStorage.getItem(this.userKey);
    },

    isLoggedIn() {
        const expires = localStorage.getItem(this.expiresKey);
        return !!this.token && !!expires && new Date(expires) > new Date();
    },

    clear() {
        localStorage.removeItem(this.tokenKey);
        localStorage.removeItem(this.userKey);
        localStorage.removeItem(this.expiresKey);
    },

    logout() {
        this.clear();
        location.href = "/Login";
    },

    // POST a /api/auth/{path} del Hub (login o register)
    async post(path, body) {
        let response;
        try {
            response = await fetch(`${APP_CONFIG.hubUrl}/api/auth/${path}`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body)
            });
        } catch {
            throw new Error("No se pudo conectar con el servidor. ¿Está corriendo el Hub?");
        }

        let data = null;
        try { data = await response.json(); } catch { /* sin cuerpo */ }

        if (!response.ok) {
            throw new Error(data?.error ?? "Ocurrió un error. Intenta de nuevo.");
        }
        return data;
    }
};
