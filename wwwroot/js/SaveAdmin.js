$(document).ready(function () {
    console.log(typeof $.fn.validate); // Should print "function"

    document.getElementById('ProfileImage').addEventListener('change', function (event) {
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

    const phoneInputField = document.querySelector("#PhoneNumber");
    phoneInput = window.intlTelInput(phoneInputField, {
        initialCountry: "IN", // Set initial country (auto or a specific code like "us")
        geoIpLookup: function (callback) {
            fetch('https://ipapi.co/json', { mode: 'no-cors' })
                .then((response) => response.json())
                .then((data) => callback(data.country_code))
                .catch(() => callback("us"));
        },
        utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.8/js/utils.js"
    });


    $("#SaveAdmin").validate({
        rules: {
            FirstName: {
                required: true,
                lettersOnly: true, // Custom method for letters only
                maxlength: 20
            },
            LastName: {
                lettersOnly: true // Custom method for letters only
            },
            Username: {
                required: true
            },
            Email: {
                required: true,
                email: true
            },
            PhoneNumber: {
                required: true,
                digits: true,
                minlength: 6,
                maxlength: 15
            },
            PasswordHash: {
                minlength: 6,
                required: function () {
                    return $("#UserId").val() === ""; // Require password only if AdminId is empty (new admin)
                }
            }
        },
        messages: {
            FirstName: {
                required: "Please enter your first name."
            },
            Email: {
                required: "Please enter your email.",
                email: "Please enter a valid email address."
            },
            PhoneNumber: {
                required: "Please enter your phone number.",
                digits: "Please enter only numbers.",
                minlength: "Phone number must be at least 6 digits.",
                maxlength: "Phone number cannot exceed 15 digits."
            },
            PasswordHash: {
                minlength: "Password must have atleast 6 digits",
                required: "Password is required"
            },
            Username: {
                required: "Please enter admin Username"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            const formattedPhoneNumber = phoneInput.getNumber(intlTelInputUtils.numberFormat.E164);
            formData.set("PhoneNumber", formattedPhoneNumber);
            const btnRegister = $("#btnSubmit");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                // AJAX submission
                $.ajax({
                    url: '/Admins/SaveAdmin',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            window.location.href = '/Admins';
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
            }, 2000);
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
