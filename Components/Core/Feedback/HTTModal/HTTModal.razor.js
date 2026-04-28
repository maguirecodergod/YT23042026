export function onModalOpen(modalId) {
    document.body.style.overflow = 'hidden';
    document.body.style.paddingRight = getScrollbarWidth() + 'px';
    
    // Simple focus trap initialization could go here
    // For now, let's just focus the modal container or first input
    setTimeout(() => {
        const modal = document.getElementById(modalId);
        if (modal) {
            const firstInput = modal.querySelector('input, button, select, textarea, [tabindex]:not([tabindex="-1"])');
            if (firstInput) {
                firstInput.focus();
            }
        }
    }, 100);
}

export function onModalClose(modalId) {
    const openModals = document.querySelectorAll('.htt-modal-wrapper.is-visible');
    if (openModals.length <= 1) {
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
    }
}

function getScrollbarWidth() {
    return window.innerWidth - document.documentElement.clientWidth;
}
