$(document).ready(function () {
    document.getElementById('ProductImage').addEventListener('change', function (event) {
        const file = event.target.files[0];
        const previewImage = document.getElementById('imagePreview');
        const previewText = document.getElementById('imagePreviewText');

        if (file) {
            const reader = new FileReader();

            // Display image once it's loaded
            reader.onload = function (e) {
                previewImage.src = e.target.result;
                previewImage.style.display = 'block';
                previewText.style.display = 'none';
            };

            // Read the selected file
            reader.readAsDataURL(file);
        } else {
            // Reset preview if no file is selected
            previewImage.src = '#';
            previewImage.style.display = 'none';
            previewText.style.display = 'block';
        }
    });

    $("#SaveShopProduct").validate({
        rules: {
            ProductName: {
                required: true,
            },
            Category: {
                required: true,
            },
            PricePerUnit: {
                required: true,
            },
            CompanyName: {
                required: true,
            },
            Manufacturer: {
                required: true,
            },
            ProductQty: {
                required: true,
                min: 1,
            }
        },
        messages: {
            ProductName: {
                required: "Product Name is required",
            },
            Category: {
                required: "Category is required",
            },
            PricePerUnit: {
                required: "Price Per Unit is required",
            },
            CompanyName: {
                required: "Company Name is required",
            },
            Manufacturer: {
                required: "Manufacturer Name is required",
            },
            ProductQty: {
                required: "Product Quantity is required",
                min: "Product Quantity must be greater than 0",
            }
        },
        submitHandler: function (form, event) {

            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnSubmit");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                // AJAX submission
                $.ajax({
                    url: '/Product/SaveShopProduct',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {

                        if (result.success) {

                            window.location.href = '/Products';
                        }
                        else {
                            showToast(result.message, "error")
                        }
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("Unknown error occurred", "error")
                    }
                });
            });
        } 
    })

    // Custom method for letters only
    $.validator.addMethod("lettersOnly", function (value, element) {
        return this.optional(element) || /^[a-zA-Z]+$/.test(value);
    }, "Please enter only letters.");

    // Custom validation method for file extension
    $.validator.addMethod("extension", function (value, element, param) {
        return this.optional(element) || param.split("|").some(ext => value.endsWith(`.${ext}`));
    }, "Please upload a valid image file (jpg, jpeg, png).");

});
