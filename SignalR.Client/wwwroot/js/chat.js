"use strict";

// Sin sesión válida no se puede entrar al chat
if (!Auth.isLoggedIn()) {
    location.replace("/Login");
} else {
    initChat();
}

function initChat() {
    const me = Auth.user;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${APP_CONFIG.hubUrl}/chatHub`, {
            accessTokenFactory: () => Auth.token
        })
        .withAutomaticReconnect()
        .build();

    const chatForm = document.getElementById("chatForm");
    const messageInput = document.getElementById("messageInput");
    const sendButton = document.getElementById("sendButton");
    const statusBadge = document.getElementById("statusBadge");
    const messagesList = document.getElementById("messagesList");
    const onlineList = document.getElementById("onlineList");
    const errorBox = document.getElementById("errorBox");

    function setStatus(text, kind) {
        statusBadge.textContent = text;
        statusBadge.className = `badge text-bg-${kind}`;
    }

    function setEnabled(enabled) {
        messageInput.disabled = !enabled;
        sendButton.disabled = !enabled;
        if (enabled) messageInput.focus();
    }

    function showError(text) {
        errorBox.textContent = text;
        errorBox.classList.remove("d-none");
        setTimeout(() => errorBox.classList.add("d-none"), 4000);
    }

    // Mis mensajes -> derecha | mensajes de otros -> izquierda
    function addMessage(message) {
        const mine = message.user.toLowerCase() === me.toLowerCase();

        const row = document.createElement("div");
        row.className = `msg-row ${mine ? "mine" : "theirs"}`;

        const bubble = document.createElement("div");
        bubble.className = "msg-bubble";

        if (!mine) {
            const author = document.createElement("div");
            author.className = "msg-author";
            author.textContent = message.user;
            bubble.appendChild(author);
        }

        const text = document.createElement("div");
        text.className = "msg-text";
        text.textContent = message.message;   // textContent evita inyección de HTML
        bubble.appendChild(text);

        const time = document.createElement("div");
        time.className = "msg-time";
        time.textContent = new Date(message.sentAt)
            .toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" });
        bubble.appendChild(time);

        row.appendChild(bubble);
        messagesList.appendChild(row);
    }

    function scrollToBottom() {
        messagesList.scrollTop = messagesList.scrollHeight;
    }

    // Historial guardado en la tabla Mensajes
    connection.on("LoadHistory", (messages) => {
        messagesList.innerHTML = "";
        messages.forEach(addMessage);
        scrollToBottom();
    });

    connection.on("ReceiveMessage", (message) => {
        addMessage(message);
        scrollToBottom();
    });

    connection.on("OnlineUsers", (users) => {
        onlineList.textContent = users.length ? users.join(", ") : "—";
    });

    connection.onreconnecting(() => {
        setStatus("Reconectando...", "warning");
        setEnabled(false);
    });

    connection.onreconnected(() => {
        setStatus("Conectado", "success");
        setEnabled(true);
    });

    connection.onclose(() => {
        setStatus("Desconectado", "danger");
        setEnabled(false);
    });

    async function startConnection() {
        try {
            await connection.start();
            setStatus("Conectado", "success");
            setEnabled(true);
        } catch (error) {
            console.error(error);
            if (error.statusCode === 401) {
                Auth.logout();          // token inválido o vencido
                return;
            }
            setStatus("Error de conexión", "danger");
        }
    }

    // Enter o clic en "Enviar"
    chatForm.addEventListener("submit", async (event) => {
        event.preventDefault();
        const message = messageInput.value.trim();
        if (!message) return;

        if (connection.state !== signalR.HubConnectionState.Connected) {
            showError("No existe una conexión activa.");
            return;
        }

        try {
            await connection.invoke("SendMessage", message);
            messageInput.value = "";
            messageInput.focus();
        } catch (error) {
            console.error("No se pudo enviar el mensaje:", error);
            showError("No se pudo enviar el mensaje.");
        }
    });

    startConnection();
}
