$(document).ready(function () {
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
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }
    });



    $("#OtpCheck").submit(function (event) {
        event.preventDefault()

        const formData = {
            Email: $('#Email').val(),
            Otp: $('#Otp').val()
        }

        console.log(formData.Email)
        console.log(formData.Otp)
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
            error: function () {
                alert('An error occurred while registering the user.');
            }
        });
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
