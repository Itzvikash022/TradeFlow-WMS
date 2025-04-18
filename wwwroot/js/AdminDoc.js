$(document).ready(function () {
    $("#AdminDocForm").validate({
        rules: {
            IdentityDocType: {
                required: true,
            },
            IdentityDoc: {
                required: true,
            },
            IdentityDocNo: {
                required: true,
            },
            AddressProof: {
                required: true,
            },
        },
        messages: {
            IdentityDocType: {
                required: "Select Identitification Document Type",
            },
            IdentityDoc: {
                required: "Upload Identity Document",
            },
            IdentityDocNo: {
                required: "Provide the uploaded document's unique number",
            },
            AddressProof: {
                required: "Upload Addresss Proof",
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnDocumentUpload");
            const btnLoader = $("#btnLoader");

            for (let pair of formData.entries()) {
                console.log(`${pair[0]}:`, pair[1]);
            }

            // AJAX submission
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
                $.ajax({
                    url: '/Auth/AdminDoc',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            window.location.href = '/Auth/Login';
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
        }
    });
});
