export function initResizer(dotNetRef, sidebarElement, position) {
    let isResizing = false;

    const onMouseDown = (e) => {
        isResizing = true;
        document.body.classList.add('is-resizing');
        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
    };

    const onMouseMove = (e) => {
        if (!isResizing) return;

        let newSize = 0;
        const rect = sidebarElement.getBoundingClientRect();

        if (position === 'Left') {
            newSize = e.clientX - rect.left;
        } else if (position === 'Right') {
            newSize = rect.right - e.clientX;
        } else if (position === 'Top') {
            newSize = e.clientY - rect.top;
        } else if (position === 'Bottom') {
            newSize = rect.bottom - e.clientY;
        }

        if (newSize > 0) {
            dotNetRef.invokeMethodAsync('UpdateSize', newSize + 'px');
        }
    };

    const onMouseUp = () => {
        isResizing = false;
        document.body.classList.remove('is-resizing');
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
    };

    const onKeyDown = (e) => {
        if (e.key === 'Escape') {
            dotNetRef.invokeMethodAsync('CloseOverlay');
        }
    };

    const resizer = sidebarElement.querySelector('.htt-sidebar-resizer');
    if (resizer) {
        resizer.addEventListener('mousedown', onMouseDown);
    }

    document.addEventListener('keydown', onKeyDown);

    return {
        dispose: () => {
            if (resizer) resizer.removeEventListener('mousedown', onMouseDown);
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
            document.removeEventListener('keydown', onKeyDown);
        }
    };
}
