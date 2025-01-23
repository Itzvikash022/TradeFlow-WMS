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

    $("#SignUpForm").validate({
        rules: {
            Username: {
                required: true,
                minlength: 3,
                maxlength: 20
            },
            Email: {
                required: true,
                email: true
            },
            PasswordHash: {
                required: true,
                minlength: 8
            },
            ConfirmPassword: {
                required : true,
                minlength : 8,
                equalTo: "#PasswordHash"
            },
        },
        messages: {
            Username: {
                required: "Please enter a username.",
                minlength: "Username must be at least 3 characters.",
                maxlength: "Username cannot exceed 15 characters."
            },
            Email: {
                required: "Please enter your email.",
                email: "Please enter a valid email address."
            },
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required : "Please enter confirm password",
                minlength : "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            },
        },
        
        submitHandler: function (form, event) {
            event.preventDefault()
            const btnRegister = $("#btnRegister");
            const btnLoader = $("#btnLoader");

            // Disable button and show loader
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            const formData = new FormData(form);

            // AJAX submission
            $.ajax({
                url: '/Auth/Register',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Auth/OtpCheck';
                    }
                },
                complete: function () {
                    // Re-enable button and hide loader
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }
    });

    $("#OtpCheck").submit(function (event) {
        event.preventDefault();

        const formData = {
            Email: $('#Email').val(),
            Otp: $('#Otp').val()
        };

        const btnVerify = $("#btnVerify");
        const btnLoader = $("#btnLoader");

        // Disable button and show loader immediately
        btnVerify.prop("disabled", true);
        btnLoader.removeClass("d-none");

        // Add a 5-second delay before making the AJAX request
        setTimeout(function () {
            // AJAX submission
            $.ajax({
                url: '/Auth/OtpCheck',
                type: 'POST',
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Auth/MoreDetails';
                    }
                },
                complete: function () {
                    // Re-enable button and hide loader
                    btnVerify.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }, 2000);
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

    const timerElement = $("#timer");
    const resendSection = $("#resend-section");
    const resendButton = $("#resend-btn");
    const duration = 60;
    let timeRemaining = duration;

    // Update the timer every second
    const interval = setInterval(() => {
        if (timeRemaining <= 0) {
            clearInterval(interval);
            timerElement.text("You can resend the OTP.");
            resendSection.show(); // Show the resend button when timer expires
        } else {
            timeRemaining--;
            const minutes = Math.floor(timeRemaining / 60).toString().padStart(2, "0");
            const seconds = (timeRemaining % 60).toString().padStart(2, "0");
            timerElement.text(`Time remaining: ${minutes}:${seconds}`);
        }
    }, 1000);

    // Resend OTP functionality
    resendButton.click(function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Auth/ResendOTP", // Adjust the route to your server's endpoint
            method: "GET",
            success: function (data) {
                if (data.success) {
                    alert("OTP has been resent to your email.");
                    timeRemaining = duration; // Reset the timer
                    resendSection.hide(); // Hide the resend button
                    timerElement.text("Time remaining: 01:00"); // Reset the timer text
                    setInterval(interval); // Restart the interval
                } else {
                    alert("Failed to resend OTP. Please try again.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error resending OTP:", error);
                alert("An error occurred. Please try again.");
            }
        });
    });
});
