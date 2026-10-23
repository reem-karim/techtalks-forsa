// Forsa site scripts

document.addEventListener('DOMContentLoaded', function () {
    // Conversation page: start at the newest message.
    var chat = document.getElementById('chat-window');
    if (chat) {
        chat.scrollTop = chat.scrollHeight;
    }
});

// Double-submit protection (A2, A3, A4, ...): a form can only be submitted once per page load.
// This is a convenience layer only; the server still enforces every rule (second tab, retries, scripts).
document.addEventListener('submit', function (e) {
    var form = e.target;
    if (!(form instanceof HTMLFormElement)) return;

    // Validation failed or a confirm() was cancelled: leave the form usable.
    if (e.defaultPrevented) return;

    if (form.dataset.submitting) {
        e.preventDefault();
        return;
    }
    form.dataset.submitting = '1';

    // Disable on the next tick: by then the browser has already collected the form data (incl. the clicked
    // button's name/value such as accept/decline). Disabling synchronously could drop that value.
    setTimeout(function () {
        form.querySelectorAll('button[type=submit], input[type=submit]').forEach(function (b) {
            b.disabled = true;
        });
    }, 0);
});

// Coming back with the browser's back button must not leave the form locked.
window.addEventListener('pageshow', function (e) {
    if (!e.persisted) return;
    document.querySelectorAll('form').forEach(function (form) {
        delete form.dataset.submitting;
        form.querySelectorAll('button[type=submit], input[type=submit]').forEach(function (b) {
            b.disabled = false;
        });
    });
});
