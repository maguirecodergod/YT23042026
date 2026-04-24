window.httLayout = {
    initResizeListener: function (dotNetHelper) {
        window.addEventListener('resize', () => {
            dotNetHelper.invokeMethodAsync('OnWindowResize', window.innerWidth);
        });
    },
    getWindowWidth: function () {
        return window.innerWidth;
    }
};
