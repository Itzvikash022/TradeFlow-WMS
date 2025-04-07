$(document).ready(function () {


    $("#googleDetailsForm").validate({
        rules: {
            Username: {
                required: true,
            },
            Email: {
                required: true,
            }
        },
        messages: {
            Username: {
                required: "Please enter your Username."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnSave");
            const btnLoader = $("#btnLoader");
            // AJAX submission
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                $.ajax({
                    url: '/Auth/GoogleDetails',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            window.location.href='/auth/moredetails'
                        }
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        alert('An error occurred while login');
                    }
                });
            }, 2000);
        }
    });

});
