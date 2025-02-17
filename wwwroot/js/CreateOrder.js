document.addEventListener("DOMContentLoaded", function () {
    let selectedProducts = [];
    let cachedProducts = []; // Store API response globally




    $(document).ready(function () {
        let selectedProducts = [];

        // Event Delegation for dynamically added .plus-icon elements
        $(document).on("click", ".plus-icon", function () {
            let row = $(this).closest("tr");
            let productName = row.find(".productimgname a").text().trim();
            let qtyInput = row.find(".small-input");
            let qty = parseInt(qtyInput.val(), 10);
            let availableQty = parseInt(row.find("td:nth-child(5)").text().trim(), 10); // Available Stock
            let price = parseFloat(row.find("td:nth-child(4)").text().trim()); // Price
            let productId = row.data("product-id"); // Product ID from data attribute
            let companyId = row.data("company-id");
            let sellerShopId = row.data("sellershop-id");

            console.log("Product ID:", productId);
            console.log("Company ID:", companyId);
            console.log("Seller Shop ID:", sellerShopId); // Debugging statement

            if (!qty || qty <= 0) {
                alert("Please enter a valid quantity.");
                return;
            }

            let existingProduct = selectedProducts.find(p => p.productId === productId);
            let totalSelectedQty = existingProduct ? existingProduct.qty + qty : qty;

            if (totalSelectedQty > availableQty) {
                alert("Cannot add more than available stock!");
                return;
            }

            if (existingProduct) {
                existingProduct.qty += qty;
                existingProduct.totalPrice = (existingProduct.qty * price).toFixed(2);
            } else {
                let product = { productId, productName, qty, pricePerUnit: price, totalPrice: (qty * price).toFixed(2), companyId, sellerShopId };
                selectedProducts.push(product);
            }

            updateSelectedProducts();
        });

        // Function to update the selected products list in the table
        function updateSelectedProducts() {
            let selectedTable = $("#selected-products");
            selectedTable.empty();
            let totalQty = 0;
            let totalBill = 0;

            selectedProducts.forEach((product, index) => {
                totalQty += product.qty;
                totalBill += parseFloat(product.totalPrice);

                selectedTable.append(`
                <tr>
                    <td>${product.productName}</td>
                    <td>${product.qty}</td>
                    <td>Rs.${product.totalPrice}</td>
                    <td style="display: none;">${product.companyId}</td>
                    <td style="display: none;">${product.sellerId}</td>
                    <td><button class="btn btn-sm btn-danger remove-btn" data-index="${index}">❌</button></td>
                </tr>
            `);
            });

            $("#total-products").text(selectedProducts.length);
            $("#total-qty").text(totalQty);
            $("#total-bill").text(totalBill.toFixed(2));

            $("#order-data").val(JSON.stringify(selectedProducts));
        }

        // Remove product from selection
        $(document).on("click", ".remove-btn", function () {
            let index = $(this).data("index");
            selectedProducts.splice(index, 1);
            updateSelectedProducts();
        });

        // Submit Order using AJAX
            $("#placeOrderBtn").on("click", function () {
                if (selectedProducts.length === 0) {
                    alert("No products selected!");
                    return;
                }

                let shopId = $("#shopId").val();
                let customerId = $("#customerId").val();
                let companyId = selectedProducts.length > 0 ? selectedProducts[0].companyId : null; // Get from first product
                let sellerShopId = selectedProducts.length > 0 ? selectedProducts[0].sellerShopId : null; // Get from first product

                // Ensure all products have the same companyId
                let allSameCompany = selectedProducts.every(p => p.companyId === companyId);
                let allSameShop = selectedProducts.every(p => p.sellerShopId === sellerShopId);

                if(!allSameShop) {
                    alert("All selected products must be from the same Shop.");
                    return;
                }

                if (!sellerShopId) {
                    if(!allSameCompany) {
                        alert("All selected products must be from the same company.");
                        return;
                    }
                }
                let SelectedshopId = $("#shopDropdownSell").val();
                let type = $("#orderTypeSelect").val();
                console.log("Type : " + type);
                console.log("SelecttedShop : " + SelectedshopId);

                if (type == "shopToShopSell") {
                    shopId = SelectedshopId;
                }

                console.log("Shop ID : " + shopId);

            

                let orderData = {
                    companyId: companyId || 0,
                    shopId: shopId || 0,
                    sellerShopId: sellerShopId || 0,
                    totalQty: selectedProducts.reduce((sum, p) => sum + p.qty, 0),
                    totalAmount: selectedProducts.reduce((sum, p) => sum + parseFloat(p.totalPrice), 0),
                    customerId: customerId || 0,
                    products: selectedProducts,
                };

                console.log(orderData);

                // Send AJAX request to create order
                $.ajax({
                    url: "/Orders/CreateOrder",
                    type: "POST",
                    contentType: "application/json",
                    data: JSON.stringify(orderData),
                    success: function (response) {
                        alert("Order placed successfully!");
                        selectedProducts = [];
                        updateSelectedProducts();
                    },
                    error: function (xhr) {
                        alert("Failed to place order. Please try again.");
                    }
                });
            });


        // Handle order type selection
        $(".order-section").hide();
        $("#orderTypeSelect").change(function () {
            let selectedType = $(this).val();
            $(".order-section").hide();
            $("#" + selectedType).show();

            // Reset the selected products list
            selectedProducts = [];
            updateSelectedProducts();
        }).trigger("change");
    });





































    // AJAX Call for Filtering Products
    $("#btnSubmitC2S").on("click", function () {
        $("#btnLoader").removeClass("d-none"); // Show loader

        let productName = $("#productName").val().trim();
        let category = $("#categoryDropdown").val();
        let company = $("#companyDropdown").val();

        $.ajax({
            url: "/Orders/GetFilteredProducts",
            type: "GET",
            data: { productName, category, company },
            success: function (response) {
                console.log("API Response:", response);
                cachedProducts = response; // Store in global variable
                displayProducts(response);
            },
            error: function () {
                alert("Error fetching products.");
            },
            complete: function () {
                $("#btnLoader").addClass("d-none"); // Hide loader
            }
        });
    });


    //Shop to customer
    $("#btnSubmitS2C").on("click", function () {
        let customerName = $("#customerName").val().trim();
        let customerEmail = $("#customerEmail").val().trim();
        let customerPhone = $("#customerPhone").val().trim();

        if (!customerName || !customerEmail || !customerPhone) {
            alert("Please enter all details before proceeding!");
            return;
        }

        $("#btnLoader").removeClass("d-none"); // Show loader

        $.ajax({
            url: "/Orders/GetProductsS2C",
            type: "POST",
            data: { customerName, customerEmail, customerPhone },
            success: function (response) {
                console.log("API Response:", response);
                alert(response.msg);
                cachedProducts = response.result; // Store in global variable
                displayProducts(response.result);
                $("#customerId").val(response.customerId);
                alert(response.customerId);
            },
            error: function () {
                alert("Error fetching products.");
            },
            complete: function () {
                $("#btnLoader").addClass("d-none"); // Hide loader
            }
        });
    });

    // AJAX Call for Filtering Products
    $("#btnSubmitS2SBuying").on("click", function () {
        $("#btnLoader").removeClass("d-none"); // Show loader

        let productName = $("#productNameShop").val().trim();
        let category = $("#ctDropdown").val();
        let shop = $("#shopDropdown").val();

        $.ajax({
            url: "/Orders/GetFilteredProductsShop",
            type: "GET",
            data: { productName, category, shop },
            success: function (response) {
                console.log("API Response:", response);
                cachedProducts = response; // Store in global variable
                displayProducts(response);
            },
            error: function () {
                alert("Error fetching products.");
            },
            complete: function () {
                $("#btnLoader").addClass("d-none"); // Hide loader
            }
        });
    });

    // AJAX Call for Filtering Products (Shop to Shop Selling)
    $("#shopDropdownSell").on("change", function () {
        let shopId = $("#shopDropdownSell").val();
        //console.log(shopId);
        if (!shopId) return;

        //selectedProducts = [];
        //updateSelectedProducts();

        // Show loader (if applicable)
        $("#btnLoader").removeClass("d-none");

        // Fetch Owner Name & Address
        $.ajax({
            url: "/Orders/GetShopKeepersDetails",
            type: "GET",
            data: { selectedShopId: shopId },
            success: function (data) {
                $("#ownerName").val(data.ownerName);
                $("#shopAddress").val(data.shopAddress);
            },
            error: function () {
                alert("Error fetching shop details.");
            }
        });

        // Fetch Products Available in My Shop
        $.ajax({
            url: "/Orders/GetMyShopProducts",
            type: "GET",
            success: function (response) {
                console.log("API Response:", response);
                cachedProducts = response; // Store in global variable
                displayProducts(response);
            },
            error: function () {
                alert("Error fetching products.");
            },
            complete: function () {
                $("#btnLoader").addClass("d-none"); // Hide loader
            }
        });
    });




    // Function to display products from stored data
    function displayProducts(products) {
        let tableBody = $("#productTable tbody");
        tableBody.empty();

        if (products.text) {
            tableBody.append(`<tr><td colspan="5" class="text-center">${products.text}</td></tr>`);
        } else {
            products.result.forEach(product => {
                tableBody.append(`
                    <tr data-sellershop-id="${product.sellerShopId}" data-product-id="${product.productId}" data-company-id="${product.companyId}" >
                    <td class="productimgname">
                                            <a href="javascript:void(0);" class="product-img">
                                                <img src="${product.productImagePath}" alt="Img 404" />
                                            </a>
                                            <a>${product.productName}</a>
                                        </td>
                        <td>${product.companyName}</td>
                        <td>${product.category}</td>
                        <td class="price-column">${product.pricePerUnit}</td>
                        <td class="price-column">${product.productQty}</td>
                        <td class="product-qty">
                            <div class="qty-container">
                                <input type="number" class="form-control small-input" />
                                <img src="/assets/img/icons/plus-circle.svg" alt="img" class="plus-icon" />
                            </div>
                        </td>
                    </tr>
                `);
            });
        }
    }
});
