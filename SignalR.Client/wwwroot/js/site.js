"use strict";

// Barra superior: muestra el usuario y el botón de cerrar sesión
(function () {
    const box = document.getElementById("navUserBox");
    if (!box || !Auth.isLoggedIn()) return;

    document.getElementById("navUserName").textContent = Auth.user;
    box.classList.remove("d-none");
    document.getElementById("logoutBtn").addEventListener("click", () => Auth.logout());
})();
