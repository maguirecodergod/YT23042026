/**
 * HTT Theme Engine
 * 
 * Runtime theme management. The initial theme is resolved synchronously
 * by an inline <script> in index.html (before any CSS loads) so there
 * is never a flash of wrong colours.
 * 
 * This module handles:
 *   - Listening for OS-level prefers-color-scheme changes
 *   - Applying theme changes triggered by the user at runtime
 *   - Keeping color-scheme, data-theme, data-bs-theme in sync
 */
window.httTheme = {

    /**
     * Subscribe to OS dark/light mode changes.
     * Calls back into .NET when the user toggles system appearance.
     */
    initThemeListener: function (dotNetHelper) {
        var mq = matchMedia('(prefers-color-scheme: dark)');
        mq.addEventListener('change', function (e) {
            dotNetHelper.invokeMethodAsync('OnSystemThemeChanged', e.matches);
        });
    },

    /** Returns true if the OS is currently in dark mode. */
    getSystemPreference: function () {
        return matchMedia('(prefers-color-scheme: dark)').matches;
    },

    /**
     * Apply a theme to the document.
     * @param {'dark'|'light'|'system'} theme
     * 
     * 'system' is resolved to concrete 'dark'|'light' based on OS pref
     * so CSS attribute selectors always have a concrete value.
     */
    applyTheme: function (theme) {
        var resolved = theme;
        if (theme === 'system') {
            resolved = matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
        }

        var d = document.documentElement;
        d.setAttribute('data-theme', resolved);
        d.setAttribute('data-bs-theme', resolved);
        d.style.colorScheme = resolved;
    },

    /**
     * Remove the loading guard that suppresses CSS transitions.
     * Called once after Blazor's first render so subsequent theme
     * switches animate smoothly.
     */
    enableTransitions: function () {
        document.documentElement.removeAttribute('data-htt-loading');
    }
};
