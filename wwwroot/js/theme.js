window.httTheme = {
    initThemeListener: function (dotNetHelper) {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
        
        const listener = (e) => {
            dotNetHelper.invokeMethodAsync('OnSystemThemeChanged', e.matches);
        };
        
        mediaQuery.addEventListener('change', listener);
    },
    getSystemPreference: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    },
    applyTheme: function (theme) {
        if (theme === 'system') {
            document.documentElement.removeAttribute('data-theme');
            document.documentElement.removeAttribute('data-bs-theme');
        } else {
            document.documentElement.setAttribute('data-theme', theme);
            document.documentElement.setAttribute('data-bs-theme', theme);
        }
    }
};
