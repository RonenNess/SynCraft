// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ===== Timeline horizontal scroll logic =====
// Compares the needed track width (timelineDays * pxPerDay) against
// the actual available width inside the container. If it doesn't fit,
// enable overflow-x:auto and set min-width on the tracks so the
// content is scrollable. Otherwise, hide overflow so no scrollbar appears.
(function () {
    var PX_PER_DAY = 12;
    var MIN_TRACK_WIDTH = 400;
    var LABEL_WIDTH = 160; // .timeline-label fixed width

    function updateTimelines() {
        document.querySelectorAll('.timeline-container[data-timeline-days]').forEach(function (container) {
            var days = parseInt(container.getAttribute('data-timeline-days')) || 0;
            var neededTrack = Math.max(MIN_TRACK_WIDTH, days * PX_PER_DAY);
            var neededTotal = neededTrack + LABEL_WIDTH;
            var availableWidth = container.clientWidth;

            var tracks = container.querySelectorAll('.timeline-track');

            if (neededTotal > availableWidth) {
                // Content is wider than the container — enable scrolling
                container.style.overflowX = 'auto';
                tracks.forEach(function (t) { t.style.minWidth = neededTrack + 'px'; });
            } else {
                // Content fits — no scrollbar
                container.style.overflowX = 'hidden';
                tracks.forEach(function (t) { t.style.minWidth = ''; });
            }
        });
    }

    // Run on load and on resize
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', updateTimelines);
    } else {
        updateTimelines();
    }
    window.addEventListener('resize', updateTimelines);
})();

// ===== Dark / Light mode toggle =====
(function () {
    function getTheme() {
        return localStorage.getItem('syncraft-theme') || 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        localStorage.setItem('syncraft-theme', theme);
        var icon = document.getElementById('themeIcon');
        if (icon) icon.textContent = theme === 'dark' ? '☀️' : '🌙';
    }

    function init() {
        applyTheme(getTheme());
        var btn = document.getElementById('themeToggleBtn');
        if (btn) {
            btn.addEventListener('click', function () {
                applyTheme(getTheme() === 'dark' ? 'light' : 'dark');
            });
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
