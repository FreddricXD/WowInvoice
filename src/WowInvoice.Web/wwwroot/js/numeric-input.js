(function () {
    function sanitizeInt(value) {
        return value.replace(/\D/g, '');
    }

    function sanitizeDecimal(value) {
        let cleaned = value.replace(/[^\d.]/g, '');
        const dotIndex = cleaned.indexOf('.');
        if (dotIndex !== -1) {
            const whole = cleaned.slice(0, dotIndex);
            const fraction = cleaned.slice(dotIndex + 1).replace(/\./g, '').slice(0, 2);
            cleaned = whole + '.' + fraction;
        }
        return cleaned;
    }

    function sanitizePhone(value) {
        return value.replace(/[^\d+\-() ]/g, '');
    }

    function applySanitizer(el) {
        if (el.classList.contains('input-int')) {
            el.value = sanitizeInt(el.value);
        } else if (el.classList.contains('input-decimal')) {
            el.value = sanitizeDecimal(el.value);
        } else if (el.classList.contains('input-phone')) {
            el.value = sanitizePhone(el.value);
        }
    }

    function isNumericTarget(el) {
        return el instanceof HTMLInputElement && (
            el.classList.contains('input-int') ||
            el.classList.contains('input-decimal') ||
            el.classList.contains('input-phone'));
    }

    document.addEventListener('keydown', (e) => {
        const el = e.target;
        if (!(el instanceof HTMLInputElement) || el.readOnly || el.disabled) return;

        if (['Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(e.key)) {
            return;
        }

        if (e.ctrlKey || e.metaKey) return;

        if (el.classList.contains('input-int')) {
            if (!/^\d$/.test(e.key)) e.preventDefault();
            return;
        }

        if (el.classList.contains('input-decimal')) {
            if (/^\d$/.test(e.key)) return;
            if (e.key === '.' && !el.value.includes('.')) return;
            e.preventDefault();
            return;
        }

        if (el.classList.contains('input-phone')) {
            if (/^[\d+\-() ]$/.test(e.key)) return;
            e.preventDefault();
        }
    });

    document.addEventListener('input', (e) => {
        const el = e.target;
        if (!isNumericTarget(el)) return;
        applySanitizer(el);
    });

    document.addEventListener('paste', (e) => {
        const el = e.target;
        if (!isNumericTarget(el)) return;

        e.preventDefault();
        const paste = (e.clipboardData || window.clipboardData).getData('text') || '';
        let cleaned = paste;

        if (el.classList.contains('input-int')) cleaned = sanitizeInt(paste);
        else if (el.classList.contains('input-decimal')) cleaned = sanitizeDecimal(paste);
        else if (el.classList.contains('input-phone')) cleaned = sanitizePhone(paste);

        const start = el.selectionStart ?? el.value.length;
        const end = el.selectionEnd ?? el.value.length;
        el.value = el.value.slice(0, start) + cleaned + el.value.slice(end);
        el.dispatchEvent(new Event('input', { bubbles: true }));
    });

    document.querySelectorAll('.input-int, .input-decimal, .input-phone').forEach(applySanitizer);
})();
