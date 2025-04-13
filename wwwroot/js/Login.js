$(document).ready(function () {

    document.querySelectorAll('.toggle-password').forEach(toggle => {
        toggle.addEventListener('click', () => {
            // Locate the password input within the same parent (.pass-group)
            const passwordInput = toggle.closest('.pass-group').querySelector('.pass-input');

            // Toggle input type between "password" and "text"
            if (passwordInput.type === "password") {
                passwordInput.type = "text";
                toggle.classList.remove("fa-eye-slash");
                toggle.classList.add("fa-eye");
            } else {
                passwordInput.type = "password";
                toggle.classList.remove("fa-eye");
                toggle.classList.add("fa-eye-slash");
            }
        });

    });
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
            const btnRegister = $("#btnLogin");
            const btnLoader = $("#btnLoader");
            // AJAX submission
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");

                $.ajax({
                    url: '/Auth/Login',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            if (result.res != "Dashboard") {
                                window.location.href = "/Auth/" + result.res;
                            }
                            else {
                                window.location.href = "/" + result.res;
                            }

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
                        showToast('Unknown error occurred during login', "error")

                    }
                });
        }
    });

    $("#companyLogin").validate({
        rules: {
            Email: {
                required: true,
            },
            PasswordHash: {
                required: true,
            }
        },
        messages: {
            Email: {
                required: "Please enter your Credentials."
            },
            PasswordHash: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnLogin");
            const btnLoader = $("#btnLoader");
            // AJAX submission
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");

                $.ajax({
                    url: '/Auth/CompanyLogin',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                                window.location.href = "/Dashboard" ;
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
            const btnRegister = $("#btnSubmit");
            const btnLoader = $("#btnLoader");

            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            // AJAX submission
                $.ajax({
                    url: '/Auth/ForgotPassword',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            showToast(result.message, "success")
                        }
                        else {
                            showToast(result.message, "warning")
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

            const btnRegister = $("#btnSubmit");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");

                // AJAX submission
                $.ajax({
                    url: '/Auth/ResetPasswordAction',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            window.location.href = '/Auth/Login';
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
