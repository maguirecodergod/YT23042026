/**
 * HTTFileUploader.js
 * JS Interop module for HTTFileUploader Blazor component.
 */

window.HTTFileUploader = (() => {
    const _instances = new Map();

    return {
        /**
         * Initialize interop on the drop zone element.
         * @param {HTMLElement} dropZone  - The dropzone container element
         * @param {string} inputId        - The unique ID of the hidden <InputFile>
         * @param {Object} dotNetRef      - DotNetObjectReference
         */
        init(dropZone, inputId, dotNetRef) {
            if (!dropZone || _instances.has(dropZone)) return;

            // --- Global Prevention ---
            // Stop browser from opening files dropped anywhere on the page
            if (!_instances.size) {
                window.addEventListener('dragover', e => e.preventDefault(), false);
                window.addEventListener('drop', e => e.preventDefault(), false);
            }

            // --- Clipboard Paste ---
            const pasteHandler = async (e) => {
                // Only process paste if the dropzone is visible and focused or being interacted with
                // For enterprise UX, we check if the active element is within our component
                // or if the mouse is currently over our dropzone.
                const isOver = dropZone.classList.contains('htt-uploader__dropzone--drag-over') || 
                               dropZone.matches(':hover');
                const isFocusWithin = dropZone.contains(document.activeElement);

                if (!isOver && !isFocusWithin) return;

                const items = e.clipboardData?.items;
                if (!items) return;

                let hasFile = false;
                for (const item of items) {
                    if (item.kind !== 'file') continue;
                    const file = item.getAsFile();
                    if (!file) continue;

                    hasFile = true;
                    // We don't preventDefault here if it's not a file to allow normal text paste in other inputs
                    e.preventDefault(); 
                    
                    await dotNetRef.invokeMethodAsync(
                        'OnClipboardPaste',
                        file.name || `pasted-file-${Date.now()}`,
                        file.size,
                        file.type || 'application/octet-stream'
                    );
                }
            };

            // --- Drag & Drop ---
            const preventDefault = (e) => {
                e.preventDefault();
                e.stopPropagation();
            };

            const dropHandler = async (e) => {
                preventDefault(e);
                
                const files = e.dataTransfer.files;
                if (files && files.length > 0) {
                    const input = document.getElementById(inputId);
                    if (input) {
                        // DataTransfer object is the modern way to set files programmatically
                        const dataTransfer = new DataTransfer();
                        for (let i = 0; i < files.length; i++) {
                            dataTransfer.items.add(files[i]);
                        }
                        input.files = dataTransfer.files;
                        input.dispatchEvent(new Event('change', { bubbles: true }));
                    }
                }
            };

            dropZone.addEventListener('dragenter', preventDefault);
            dropZone.addEventListener('dragover', preventDefault);
            dropZone.addEventListener('dragleave', preventDefault);
            dropZone.addEventListener('drop', dropHandler);
            
            // Listen on window to catch paste events from anywhere, 
            // but the handler filters by focus/hover.
            window.addEventListener('paste', pasteHandler);

            _instances.set(dropZone, { pasteHandler, dropHandler, preventDefault, dotNetRef });
        },

        triggerFilePicker(inputId) {
            const input = document.getElementById(inputId);
            if (input) {
                input.click();
            }
        },

        createPreviewUrl(inputId, fileName) {
            try {
                const input = document.getElementById(inputId);
                const files = input?.files;
                if (!files) return '';

                for (const file of files) {
                    if (file.name === fileName) {
                        return URL.createObjectURL(file);
                    }
                }
                return '';
            } catch {
                return '';
            }
        },

        destroy(dropZone) {
            const instance = _instances.get(dropZone);
            if (!instance) return;

            window.removeEventListener('paste', instance.pasteHandler);
            dropZone.removeEventListener('drop', instance.dropHandler);
            _instances.delete(dropZone);
        }
    };
})();
