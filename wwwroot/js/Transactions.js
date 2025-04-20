$(document).ready(function () {
    console.log("Transaction.js loaded");

    let razorpayTimeout = null; // store the timeout reference

    $("#TransactionForm").validate({
        rules: {
            ReferenceNumber: { required: true },
            Remarks: { required: true }
        },
        messages: {
            ReferenceNumber: { required: "Reference Number is required" },
            Remarks: { required: "Add a remark" }
        },

        submitHandler: function (form, event) {
            event.preventDefault();

            const btn = $("#btnCheckout");
            const btnLoader = $("#btnLoader");
            btn.prop("disabled", true);
            btnLoader.removeClass("d-none");

            let orderId = $("#OrderId").val();
            let amount = $("#Amount").val();
            let buyerName = $("#BuyerName").val();
            let sellerName = $("#SellerName").val();
            let paymentType = $("#TransactionType").val();

            if (paymentType == "Online") {

                // First Step: Call backend to create Razorpay Order
                $.ajax({
                    url: '/Payment/InitiatePayment',
                    type: 'POST',
                    contentType: "application/json",
                    data: JSON.stringify({
                        OrderId: orderId,
                        Amount: amount
                    }),
                    success: function (res) {
                        console.log(res);
                        if (res.success) {
                            var options = {
                                "key": "rzp_test_J0O7pTXoCjjDRN", // Razorpay KeyId
                                "amount": res.amount, // Amount in paisa
                                "currency": "INR",
                                "name": sellerName,
                                "description": "Order Payment",
                                "order_id": res.orderId,
                                "handler": function (response) {
                                    clearTimeout(razorpayTimeout); // Clear timeout
                                    SaveTransaction(orderId, response.razorpay_payment_id, response.razorpay_order_id, response.razorpay_signature);
                                },
                                "prefill": {
                                    "name": buyerName,
                                },
                                "theme": {
                                    "color": "#3399cc"
                                },
                                "modal": {
                                    ondismiss: function () {
                                        console.log("Razorpay payment popup closed by user.");
                                        clearTimeout(razorpayTimeout); // Clear timeout
                                        resetButton(); // Reset button if user closes/cancels
                                    }
                                }
                            };

                            var rzp = new Razorpay(options);

                            // Start the timeout (3 mins = 180000 ms)
                            razorpayTimeout = setTimeout(function () {
                                console.log("Payment timeout after 3 minutes.");
                                resetButton(); // Auto-reset button
                                rzp.close(); // Close Razorpay window if still open
                                showToast("Payment session expired. Please try again.", "error");
                            }, 180000); // 3 minutes

                            rzp.open();
                        } else {
                            showToast(res.message, "error");
                            resetButton();
                        }
                    },
                    error: function (xhr) {
                        console.error(xhr.responseText);
                        showToast("Error initiating payment: " + xhr.responseText, "error");
                        resetButton();
                    }
                });
            }
            else {
                    let ReferenceNo = $("#ReferenceNo").val()
                    SaveTransaction(orderId, ReferenceNo, null, null);
            }
        }
    });

    function SaveTransaction(orderId, razorpayPaymentId, razorpayOrderId, razorpaySignature) {
        let TransactionData = {
            OrderId: orderId,
            ReferenceNo: razorpayPaymentId,
            TransactionType: $("#TransactionType").val(),
            Amount: $("#Amount").val(),
            Remarks: $("#Remarks").val(),
            SellerName: $("#SellerName").val(),
            BuyerName: $("#BuyerName").val(),
            RazorpayOrderId: razorpayOrderId,
            RazorpaySignature: razorpaySignature
        };

        $.ajax({
            url: '/Orders/AddTransaction',
            type: 'POST',
            contentType: "application/json",
            data: JSON.stringify(TransactionData),
            success: function (result) {
                if (result.success) {
                    window.location.href = "/Orders";
                } else {
                    showToast(result.message, "error");
                }
            },
            complete: function () {
                resetButton();
            },
            error: function (xhr) {
                console.error(xhr.responseText);
                showToast("Error saving transaction: " + xhr.responseText, "error");
            }
        });
    }

    function resetButton() {
        $("#btnCheckout").prop("disabled", false);
        $("#btnLoader").addClass("d-none");
    }

});
