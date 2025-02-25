$(document).ready(function () {

    console.log("test-hehe");
    $("#TransactionForm").validate({
        rules: {
            ReferenceNumber: {
                required: true
            },
            Remarks: {
                required: true
            }
        },
        messages: {
            ReferenceNumber: {
                required: "Reference Number is required"
            },
            Remarks: {
                required: "Add a remark"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()

            const btn = $("#btnCheckout");
            const btnLoader = $("#btnLoader");
            btn.prop("disabled", true);
            btnLoader.removeClass("d-none");
            console.log("test-hehe2");

            let TransactionData = {
                ReferenceNo: $("#ReferenceNo").val(),
                Remarks: $("#Remarks").val(),
                TransactionType: $("#TransactionType").val(),
                Amount: $("#Amount").val(),
                OrderId: $("#OrderId").val(),
                BuyerName: $("#BuyerName").val(),
                SellerName: $("#SellerName").val()
            }
                // AJAX submission
                $.ajax({
                    url: '/Orders/AddTransaction',
                    type: 'POST',
                    contentType: "application/json",
                    data: JSON.stringify(TransactionData),
                    success: function (result) {
                        alert(result.message);
                        window.location.href = "/Orders";
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btn.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function (xhr) {
                        console.error(xhr.responseText); // Logs the error in the browser console
                        alert("Error: " + xhr.responseText); // Shows the actual error message from the server
                    }

                });
        }
    });

    // Custom method for letters only
    $.validator.addMethod("lettersOnly", function (value, element) {
        return this.optional(element) || /^[a-zA-Z]+$/.test(value);
    }, "Please enter only letters.");

    // Custom validation method for file extension
    $.validator.addMethod("extension", function (value, element, param) {
        return this.optional(element) || param.split("|").some(ext => value.endsWith(`.${ext}`));
    }, "Please upload a valid image file (jpg, jpeg, png).");

    // Add custom validation method for DateOfBirth
    $.validator.addMethod("dateBeforeToday", function (value, element) {
        // Get today's date
        var today = new Date();

        // Convert the input value into a date object (assuming it's in 'YYYY-MM-DD' format)
        var selectedDate = new Date(value);

        // Compare if the selected date is earlier than today
        return this.optional(element) || selectedDate < today;
    }, "Date of birth must be before today's date.");


});
