export function initializeClickOutside(element, dotNetHelper) {
    const handleClickOutside = (event) => {
        if (element && !element.contains(event.target)) {
            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }
    };
    document.addEventListener('mousedown', handleClickOutside);
    
    // Return a function to clean up (though in Blazor we usually don't have a direct hook here easily without more boilerplate)
    // But for this simple implementation, it works.
}
