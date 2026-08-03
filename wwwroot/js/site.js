/* ─── site.js — SpendingTracker Client Logic ─── */

document.addEventListener('DOMContentLoaded', function () {

    // ── Theme Toggle ─────────────────────────────────────────────────────────
    var themeToggleBtn = document.getElementById('themeToggle');
    var themeIcon = document.getElementById('themeIcon');

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);
        if (themeIcon) {
            themeIcon.className = theme === 'dark' ? 'bi bi-moon-fill' : 'bi bi-sun-fill';
        }
    }

    // Apply saved theme on load (already done inline, but sync the icon)
    var savedTheme = localStorage.getItem('theme') || 'light';
    applyTheme(savedTheme);

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', function () {
            var current = document.documentElement.getAttribute('data-theme') || 'light';
            applyTheme(current === 'dark' ? 'light' : 'dark');
        });
    }

    // ── Sidebar Toggle (desktop collapse) ──────────────────────────────────
    const sidebar = document.getElementById('sidebar');
    const mainWrapper = document.getElementById('mainWrapper');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const mobileSidebarToggle = document.getElementById('mobileSidebarToggle');

    // Restore collapse state
    const collapsed = localStorage.getItem('sidebarCollapsed') === 'true';
    if (collapsed && sidebar) sidebar.classList.add('collapsed');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });
    }

    // ── Sidebar Toggle (mobile) ─────────────────────────────────────────────
    if (mobileSidebarToggle && sidebar) {
        mobileSidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('mobile-open');
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', function (e) {
            if (window.innerWidth <= 991 &&
                sidebar.classList.contains('mobile-open') &&
                !sidebar.contains(e.target) &&
                e.target !== mobileSidebarToggle) {
                sidebar.classList.remove('mobile-open');
            }
        });
    }

    // ── Auto-dismiss toasts ──────────────────────────────────────────────────
    const toasts = document.querySelectorAll('.toast.show');
    toasts.forEach(function (toast) {
        setTimeout(function () {
            toast.classList.add('fade');
            setTimeout(() => toast.remove(), 400);
        }, 4000);
    });

    // ── Animate stat cards on load ───────────────────────────────────────────
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach(function (card, index) {
        card.style.opacity = '0';
        card.style.transform = 'translateY(16px)';
        setTimeout(function () {
            card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 80);
    });

    // ── Animate budget progress bars ─────────────────────────────────────────
    const progressBars = document.querySelectorAll('.budget-progress .progress-bar');
    progressBars.forEach(function (bar) {
        const targetWidth = bar.style.width;
        bar.style.width = '0%';
        setTimeout(function () {
            bar.style.width = targetWidth;
        }, 300);
    });

    // ── Confirm delete dialogs ────────────────────────────────────────────────
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            const msg = form.getAttribute('data-confirm') || 'Are you sure?';
            if (!confirm(msg)) e.preventDefault();
        });
    });

    // ── Color swatch picker ───────────────────────────────────────────────────
    const colorSwatches = document.querySelectorAll('.color-swatch');
    const colorInput = document.getElementById('colorInput');

    colorSwatches.forEach(function (swatch) {
        swatch.addEventListener('click', function () {
            colorSwatches.forEach(s => s.classList.remove('selected'));
            swatch.classList.add('selected');
            if (colorInput) colorInput.value = swatch.dataset.color;
        });

        // Mark currently selected
        if (colorInput && swatch.dataset.color === colorInput.value) {
            swatch.classList.add('selected');
        }
    });

    // ── Tooltip setup (Bootstrap) ─────────────────────────────────────────────
    const tooltipEls = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipEls.forEach(el => new bootstrap.Tooltip(el));

    // ── Chart.js global defaults ──────────────────────────────────────────────
    if (typeof Chart !== 'undefined') {
        Chart.defaults.font.family = 'Inter, -apple-system, sans-serif';
        Chart.defaults.color = '#a0aec0';
        Chart.defaults.plugins.legend.labels.boxWidth = 12;
        Chart.defaults.plugins.legend.labels.borderRadius = 4;
    }

});

// ── Helper: format currency (dynamic per user) ───────────────────────────────
function formatCurrency(value) {
    var sym = (typeof window.CURRENCY_SYMBOL !== 'undefined') ? window.CURRENCY_SYMBOL : '£';
    return sym + Number(value).toLocaleString('en-GB', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

