(function () {
    const form = document.getElementById('invoice-form');
    if (!form) return;

    const lineItemsContainer = document.getElementById('line-items');
    const addLineBtn = document.getElementById('add-line');
    let lineIndex = lineItemsContainer.querySelectorAll('.line-item').length;

    function formatCurrency(value) {
        return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value || 0);
    }

    function recalculate() {
        const quantities = [];
        const unitPrices = [];
        lineItemsContainer.querySelectorAll('.line-item').forEach(row => {
            const qty = parseFloat(row.querySelector('[name$=".Quantity"]')?.value) || 0;
            const price = parseFloat(row.querySelector('[name$=".UnitPrice"]')?.value) || 0;
            quantities.push(qty);
            unitPrices.push(price);
        });

        const taxRate = parseFloat(form.querySelector('[name="TaxRate"]')?.value) || 0;
        const discount = parseFloat(form.querySelector('[name="DiscountAmount"]')?.value) || 0;

        const params = new URLSearchParams();
        params.append('taxRate', taxRate);
        params.append('discountAmount', discount);
        quantities.forEach(q => params.append('quantities', q));
        unitPrices.forEach(p => params.append('unitPrices', p));

        fetch(`/Invoices/PreviewTotals?${params}`)
            .then(r => r.json())
            .then(data => {
                document.getElementById('preview-subtotal').textContent = formatCurrency(data.subtotal);
                document.getElementById('preview-tax').textContent = formatCurrency(data.taxAmount);
                document.getElementById('preview-discount').textContent = formatCurrency(discount);
                document.getElementById('preview-total').textContent = formatCurrency(data.grandTotal);
            })
            .catch(() => { /* silent */ });
    }

    function updateRemoveButtons() {
        const rows = lineItemsContainer.querySelectorAll('.line-item');
        rows.forEach(row => {
            const btn = row.querySelector('.remove-line');
            if (btn) btn.disabled = rows.length <= 1;
        });
    }

    function addLineItem() {
        const row = document.createElement('div');
        row.className = 'line-item row g-2 align-items-end mb-2';
        row.innerHTML = `
            <div class="col-md-5">
                <label class="form-label">Description</label>
                <input name="LineItems[${lineIndex}].Description" class="form-control" />
            </div>
            <div class="col-md-2">
                <label class="form-label">Qty</label>
                <input name="LineItems[${lineIndex}].Quantity" class="form-control calc-trigger" step="0.01" value="1" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Unit Price</label>
                <input name="LineItems[${lineIndex}].UnitPrice" class="form-control calc-trigger" step="0.01" value="0" />
            </div>
            <div class="col-md-2">
                <button type="button" class="btn btn-outline-danger w-100 remove-line">Remove</button>
            </div>`;
        lineItemsContainer.appendChild(row);
        lineIndex++;
        updateRemoveButtons();
        recalculate();
    }

    addLineBtn?.addEventListener('click', addLineItem);

    lineItemsContainer.addEventListener('click', e => {
        if (e.target.classList.contains('remove-line')) {
            e.target.closest('.line-item')?.remove();
            reindexLines();
            updateRemoveButtons();
            recalculate();
        }
    });

    form.addEventListener('input', e => {
        if (e.target.classList.contains('calc-trigger') ||
            e.target.name === 'TaxRate' ||
            e.target.name === 'DiscountAmount' ||
            e.target.name?.endsWith('.Quantity') ||
            e.target.name?.endsWith('.UnitPrice')) {
            recalculate();
        }
    });

    function reindexLines() {
        lineItemsContainer.querySelectorAll('.line-item').forEach((row, i) => {
            row.querySelectorAll('[name]').forEach(input => {
                input.name = input.name.replace(/LineItems\[\d+\]/, `LineItems[${i}]`);
            });
        });
        lineIndex = lineItemsContainer.querySelectorAll('.line-item').length;
    }

    recalculate();
})();
