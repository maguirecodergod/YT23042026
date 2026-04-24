window.httTooltip = {
    calculatePosition: (wrapper, tooltip, preferred) => {
        const wRect = wrapper.getBoundingClientRect();
        const tRect = tooltip.getBoundingClientRect();

        let position = preferred;
        let top = 0;
        let left = 0;

        const margin = 8;

        const fitsTop = wRect.top >= tRect.height;
        const fitsBottom = window.innerHeight - wRect.bottom >= tRect.height;
        const fitsLeft = wRect.left >= tRect.width;
        const fitsRight = window.innerWidth - wRect.right >= tRect.width;

        if (preferred === "top" && !fitsTop) position = "bottom";
        if (preferred === "bottom" && !fitsBottom) position = "top";
        if (preferred === "left" && !fitsLeft) position = "right";
        if (preferred === "right" && !fitsRight) position = "left";

        switch (position) {
            case "top":
                top = wRect.top - tRect.height - margin;
                left = wRect.left + (wRect.width - tRect.width) / 2;
                break;
            case "bottom":
                top = wRect.bottom + margin;
                left = wRect.left + (wRect.width - tRect.width) / 2;
                break;
            case "left":
                top = wRect.top + (wRect.height - tRect.height) / 2;
                left = wRect.left - tRect.width - margin;
                break;
            case "right":
                top = wRect.top + (wRect.height - tRect.height) / 2;
                left = wRect.right + margin;
                break;
        }

        return {
            position,
            top,
            left
        };
    }
};