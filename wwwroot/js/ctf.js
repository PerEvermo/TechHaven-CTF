// TechHaven Electronics — internal debug utilities
// v2.3.1-dev
// Dev note: session cache written to localStorage key "th_debug_token" on page load

window.getCookie = function (name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
    return null;
};

window.setCookie = function (name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
};

window.setLocalStorage = function (key, value) {
    localStorage.setItem(key, value);
};

window.printDebugBanner = function (encoded) {
    console.log(
        "%c[TechHaven Debug] %c" + encoded,
        "color:#2ecc71;font-weight:bold;font-family:monospace;",
        "color:#f39c12;font-family:monospace;"
    );
    console.log(
        "%cHint: this looks encoded. Try atob(\"" + encoded + "\") in the console.",
        "color:#888;font-style:italic;"
    );
};
