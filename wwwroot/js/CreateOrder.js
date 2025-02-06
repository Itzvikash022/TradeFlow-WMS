document.addEventListener('DOMContentLoaded', function () {
    let selectedProducts = [];

    document.querySelectorAll('.plus-icon').forEach(icon => {
        icon.addEventListener('click', function () {
            let row = this.closest('tr');
            let productName = row.querySelector('.productimgname a:nth-child(2)').textContent;
            let qtyInput = row.querySelector('.small-input');
            let qty = parseInt(qtyInput.value, 10);
            let price = parseFloat(row.cells[3].textContent.replace(/[^0-9.]/g, ''));

            if (!qty || qty <= 0) {
                alert('Please enter a valid quantity');
                return;
            }

            let totalPrice = (price * qty).toFixed(2);
            let productId = row.cells[1].textContent; // Assuming the product ID is in the second column

            let product = { productId, productName, qty, totalPrice };
            selectedProducts.push(product);
            updateSelectedProducts();
        });
    });

    function updateSelectedProducts() {
        let selectedTable = document.getElementById('selected-products');
        selectedTable.innerHTML = "";
        let totalQty = 0;
        let totalBill = 0;

        selectedProducts.forEach((product, index) => {
            totalQty += product.qty;
            totalBill += parseFloat(product.totalPrice);

            let newRow = document.createElement('tr');
            newRow.innerHTML = `
              <td>${product.productName}</td>
              <td>${product.qty}</td>
              <td>$${product.totalPrice}</td>
              <td><button class="btn btn-sm btn-danger remove-btn" data-index="${index}">❌</button></td>
            `;
            selectedTable.appendChild(newRow);
        });

        document.getElementById('total-products').textContent = selectedProducts.length;
        document.getElementById('total-qty').textContent = totalQty;
        document.getElementById('total-bill').textContent = totalBill.toFixed(2);

        document.querySelectorAll('.remove-btn').forEach(btn => {
            btn.addEventListener('click', function () {
                let index = this.getAttribute('data-index');
                selectedProducts.splice(index, 1);
                updateSelectedProducts();
            });
        });

        document.getElementById('order-data').value = JSON.stringify(selectedProducts);
    }

    document.getElementById('order-form').addEventListener('submit', function (event) {
        if (selectedProducts.length === 0) {
            alert('No products selected!');
            event.preventDefault();
        }
    });
});