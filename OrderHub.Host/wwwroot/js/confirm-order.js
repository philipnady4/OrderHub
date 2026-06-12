document.addEventListener('DOMContentLoaded', () => {
    const rows = document.querySelectorAll('#order-lines tbody tr[data-line-id]');
    const subtotalEl = document.getElementById('subtotal');

    function recalcRow(row) {
        const unitPrice = parseFloat(row.dataset.unitPrice);
        const qtyInput = row.querySelector('.qty-input');
        let qty = parseInt(qtyInput.value, 10);

        if (isNaN(qty) || qty < 0) {
            qty = 0;
            qtyInput.value = '0';
        }

        const lineTotal = unitPrice * qty;
        row.querySelector('.line-total span').textContent = lineTotal.toFixed(2);
        return lineTotal;
    }

    function recalcSubtotal() {
        let subtotal = 0;
        rows.forEach(row => { subtotal += recalcRow(row); });
        subtotalEl.textContent = subtotal.toFixed(2);
    }

    rows.forEach(row => {
        row.querySelector('.qty-input').addEventListener('input', recalcSubtotal);
    });
});