// Forsa site scripts

document.addEventListener('DOMContentLoaded', function () {
    // Conversation page: start at the newest message.
    var chat = document.getElementById('chat-window');
    if (chat) {
        chat.scrollTop = chat.scrollHeight;
    }
});
