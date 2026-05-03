window.httTooltip = {
    update: (wrapper, floating, arrowEl, preferred, isDark) => {
        if (!wrapper || !floating) return;

        // Move to body to escape all parent clipping
        if (floating.parentElement !== document.body) {
            document.body.appendChild(floating);
        }

        // Apply theme using the value provided by C# (The Source of Truth)
        const applyTheme = () => {
            if (floating.classList.contains('htt-tooltip--plain')) {
                floating.style.backgroundColor = 'transparent';
                floating.style.border = 'none';
                floating.style.boxShadow = 'none';
                return;
            }

            if (isDark) {
                // Dark Theme -> Light Tooltip
                floating.style.setProperty('background-color', '#ffffff', 'important');
                floating.style.setProperty('color', '#161b22', 'important');
                floating.style.setProperty('border', '1px solid rgba(0,0,0,0.1)', 'important');
            } else {
                // Light Theme -> Dark Tooltip
                floating.style.setProperty('background-color', '#161b22', 'important');
                floating.style.setProperty('color', '#ffffff', 'important');
                floating.style.setProperty('border', '1px solid rgba(255,255,255,0.1)', 'important');
            }
        };

        applyTheme();
        setTimeout(applyTheme, 0);

        const wRect = wrapper.getBoundingClientRect();
        
        // Measure
        floating.style.display = 'block';
        floating.style.visibility = 'hidden';
        const width = floating.offsetWidth;
        const height = floating.offsetHeight;
        floating.style.display = '';
        floating.style.visibility = '';

        const margin = 8;
        const padding = 16;

        let top = 0;
        let left = 0;
        let finalPosition = preferred;

        if (preferred === 'top' && wRect.top < height + margin + padding) {
            finalPosition = 'bottom';
        } else if (preferred === 'bottom' && window.innerHeight - wRect.bottom < height + margin + padding) {
            finalPosition = 'top';
        }

        if (finalPosition === 'top' || finalPosition === 'bottom') {
            top = finalPosition === 'top' ? wRect.top - height - margin : wRect.bottom + margin;
            left = wRect.left + (wRect.width - width) / 2;
        } else {
            top = wRect.top + (wRect.height - height) / 2;
            left = finalPosition === 'left' ? wRect.left - width - margin : wRect.right + margin;
        }

        left = Math.max(padding, Math.min(left, window.innerWidth - width - padding));
        top = Math.max(padding, Math.min(top, window.innerHeight - height - padding));

        floating.style.left = `${Math.round(left)}px`;
        floating.style.top = `${Math.round(top)}px`;

        if (arrowEl) {
            const isVertical = finalPosition === 'top' || finalPosition === 'bottom';
            if (isVertical) {
                const arrowLeft = (wRect.left + wRect.width / 2) - left - 4;
                arrowEl.style.left = `${Math.max(8, Math.min(arrowLeft, width - 16))}px`;
                arrowEl.style.top = finalPosition === 'top' ? '' : '-4px';
                arrowEl.style.bottom = finalPosition === 'top' ? '-4px' : '';
            }
        }

        return finalPosition;
    },

    hide: (floating) => {
        if (floating && floating.parentElement === document.body) {
            document.body.removeChild(floating);
        }
    }
};
