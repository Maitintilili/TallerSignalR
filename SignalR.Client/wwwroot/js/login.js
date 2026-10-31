"use strict";

if (Auth.isLoggedIn()) {
    location.replace("/");
}

const loginForm = document.getElementById("loginForm");
const alertBox = document.getElementById("alertBox");
const submitBtn = document.getElementById("submitBtn");

function showError(message) {
    alertBox.textContent = message;
    alertBox.classList.remove("d-none");
}

loginForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    alertBox.classList.add("d-none");

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;

    if (!username || !password) {
        showError("Completa el usuario y la contraseña.");
        return;
    }

    submitBtn.disabled = true;
    try {
        const response = await Auth.post("login", { username, password });
        Auth.save(response);
        location.href = "/";
    } catch (error) {
        showError(error.message);
        submitBtn.disabled = false;
    }
});
