"use strict";

if (Auth.isLoggedIn()) {
    location.replace("/");
}

const registerForm = document.getElementById("registerForm");
const alertBox = document.getElementById("alertBox");
const submitBtn = document.getElementById("submitBtn");

function showError(message) {
    alertBox.textContent = message;
    alertBox.classList.remove("d-none");
}

registerForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    alertBox.classList.add("d-none");

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;
    const confirm = document.getElementById("confirm").value;

    if (!/^[A-Za-z0-9_]{3,20}$/.test(username)) {
        showError("El usuario debe tener de 3 a 20 caracteres (letras, números o guion bajo).");
        return;
    }
    if (password.length < 6) {
        showError("La contraseña debe tener al menos 6 caracteres.");
        return;
    }
    if (password !== confirm) {
        showError("Las contraseñas no coinciden.");
        return;
    }

    submitBtn.disabled = true;
    try {
        // Crea la cuenta (tabla Usuarios) y entra directo al chat
        const response = await Auth.post("register", { username, password });
        Auth.save(response);
        location.href = "/";
    } catch (error) {
        showError(error.message);
        submitBtn.disabled = false;
    }
});
