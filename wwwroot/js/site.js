// Immediately‑invoked function to scope our variables
(function () {
    const toggleBtn = document.getElementById('themeToggle');
    const icon = document.getElementById('themeToggleIcon');
    if (!toggleBtn || !icon) return;

    // Apply theme on load
    const isDark = localStorage.getItem('darkMode') === 'true';
    document.body.classList.toggle('dark-mode', isDark);
    icon.classList.toggle('bi-sun-fill', isDark);
    icon.classList.toggle('bi-moon-stars', !isDark);

    // Wire up the click
    toggleBtn.addEventListener('click', () => {
        const nowDark = !document.body.classList.contains('dark-mode');
        document.body.classList.toggle('dark-mode', nowDark);
        localStorage.setItem('darkMode', nowDark);
        icon.classList.toggle('bi-sun-fill', nowDark);
        icon.classList.toggle('bi-moon-stars', !nowDark);
        // Initialize all Bootstrap tooltips on the page
        (function () {
            const tooltipTriggerList = Array.from(
                document.querySelectorAll('[data-bs-toggle="tooltip"]')
            );
            tooltipTriggerList.forEach(el => new bootstrap.Tooltip(el));
        })();

    });
})();
