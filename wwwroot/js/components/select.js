window.httSelect = {
    checkPlacement: function (container, dropdown) {
        if (!container || !dropdown) return;
        
        const containerRect = container.getBoundingClientRect();
        const dropdownHeight = dropdown.offsetHeight;
        const viewportHeight = window.innerHeight;
        
        // Default to down, but if not enough space and more space above, flip it
        const spaceBelow = viewportHeight - containerRect.bottom;
        const spaceAbove = containerRect.top;
        
        if (spaceBelow < dropdownHeight && spaceAbove > spaceBelow) {
            dropdown.style.top = 'auto';
            dropdown.style.bottom = 'calc(100% + 4px)';
            dropdown.classList.add('is-flipped');
        } else {
            dropdown.style.top = 'calc(100% + 4px)';
            dropdown.style.bottom = 'auto';
            dropdown.classList.remove('is-flipped');
        }
    },
    scrollToSelected: function (dropdown) {
        if (!dropdown) return;
        const selected = dropdown.querySelector('.htt-select__option.is-selected');
        if (selected) {
            selected.scrollIntoView({ block: 'nearest', inline: 'nearest' });
        }
    }
};
