document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById("chatToggleBtn");
    const closeBtn = document.getElementById("chatCloseBtn");
    const chatWindow = document.getElementById("chatWindow");
    const chatBody = document.getElementById("chatBody");
    const chatInput = document.getElementById("chatInput");
    const sendBtn = document.getElementById("chatSendBtn");

    toggleBtn.addEventListener("click", () => chatWindow.classList.toggle("d-none"));
    closeBtn.addEventListener("click", () => chatWindow.classList.add("d-none"));

    function appendMessage(text, sender) {
        const div = document.createElement("div");
        div.className = "chat-msg " + sender;
        div.textContent = text;
        chatBody.appendChild(div);
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    async function sendMessage() {
        const message = chatInput.value.trim();
        if (!message) return;

        appendMessage(message, "user");
        chatInput.value = "";

        try {
            const response = await fetch("/api/ChatBot/ask", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: message })
            });
            const data = await response.json();
            appendMessage(data.reply, "bot");
        } catch (err) {
            appendMessage("Sorry, I couldn't process that right now. Please try again.", "bot");
        }
    }

    sendBtn.addEventListener("click", sendMessage);
    chatInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") sendMessage();
    });
});
