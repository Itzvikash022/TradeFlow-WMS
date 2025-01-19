$(document).ready(function () {
    $("#loginForm").validate({
        rules: {
            EmailOrUsername: {
                required: true,
            },
            Password: {
                required: true,
            }
        },
        messages: {
            EmailOrUsername: {
                required: "Please enter your Credentials."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            // AJAX submission
            $.ajax({
                url: '/Auth/Login',
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

    $("#forgotPassword").validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: "Please enter your Email.",
                email: "Not a valid email"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            // AJAX submission
            $.ajax({
                url: '/Auth/ForgotPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                },
                error: function () {
                    alert('An error occurred while sending the email.');
                }
            });
        }
    });


    $("#ResetPassword").validate({
        rules: {
            PasswordHash: {
                required: true,
                minlength: 8
            },
            ConfirmPassword: {
                required: true,
                minlength: 8,
                equalTo: "#PasswordHash"
            },
        },
        messages: {
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please enter confirm password",
                minlength: "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            //var formData = {
            //    PasswordHash: $('#PasswordHash').val(),
            //}
            // AJAX submission
            $.ajax({
                url: '/Auth/ResetPasswordAction',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Auth/Login';
                    }
                },
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }
    });

});
