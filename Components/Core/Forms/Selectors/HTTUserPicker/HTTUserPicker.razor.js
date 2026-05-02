// Quản lý đóng dropdown khi click ra ngoài
export function initializeClickOutside(containerElement, dotNetHelper) {
    const handleClickOutside = (event) => {
        if (containerElement && !containerElement.contains(event.target)) {
            dotNetHelper.invokeMethodAsync('CloseDropdown');
        }
    };

    document.addEventListener('mousedown', handleClickOutside);

    return {
        dispose: () => {
            document.removeEventListener('mousedown', handleClickOutside);
        }
    };
}

// Quản lý phân trang khi cuộn danh sách (Infinite Scroll)
export function initializeScrollPaging(listElement, dotNetHelper) {
    if (!listElement) return;

    const options = {
        root: listElement,
        rootMargin: '20px',
        threshold: 0.1
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                console.log("[HTTUserPicker] Sentinel visible, triggering LoadMoreItems...");
                dotNetHelper.invokeMethodAsync('LoadMoreItems');
            }
        });
    }, options);

    // Dùng MutationObserver để theo dõi khi nào sentinel xuất hiện trong DOM
    const mutationObserver = new MutationObserver(() => {
        const sentinel = listElement.querySelector('.htt-user-picker__sentinel');
        if (sentinel) {
            observer.disconnect(); 
            observer.observe(sentinel);
        }
    });

    mutationObserver.observe(listElement, { 
        childList: true, 
        subtree: true 
    });

    // Thử tìm ngay lập tức nếu đã có sẵn
    const sentinel = listElement.querySelector('.htt-user-picker__sentinel');
    if (sentinel) {
        observer.observe(sentinel);
    }

    return {
        dispose: () => {
            observer.disconnect();
            mutationObserver.disconnect();
        }
    };
}
