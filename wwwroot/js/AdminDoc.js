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

            // AJAX submission
            $.ajax({
                url: '/Auth/AdminDoc',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Auth/Index';
                    }
                },
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }
    });
});
