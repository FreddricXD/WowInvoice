(function () {
    const themeKey = 'wowinvoice-theme';
    const html = document.documentElement;
    const saved = localStorage.getItem(themeKey);
    if (saved) {
        html.setAttribute('data-theme', saved);
    }

    document.getElementById('theme-toggle')?.addEventListener('click', () => {
        const current = html.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
        html.setAttribute('data-theme', current);
        localStorage.setItem(themeKey, current);
    });

    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebar-overlay');
    const toggle = document.getElementById('sidebar-toggle');

    toggle?.addEventListener('click', () => {
        sidebar?.classList.toggle('open');
        overlay?.classList.toggle('show');
    });

    overlay?.addEventListener('click', () => {
        sidebar?.classList.remove('open');
        overlay?.classList.remove('show');
    });
})();
