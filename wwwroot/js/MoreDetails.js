$(document).ready(function () {

    //For More details
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


    $("#moreDetails").validate({
        rules: {
            FirstName: {
                required: false,
                lettersOnly: true, // Custom method for letters only
                maxlength: 20
            },
            LastName: {
                lettersOnly: true // Custom method for letters only
            },
            Email: {
                required: true,
                email: true
            },
            PhoneNumber: {
                required: true,
                minlength: 6,
                maxlength: 15
            },
            DateOfBirth: {
                required: true,
                dateBeforeToday: true
            },
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
                minlength: "Phone number must be at least 6 digits.",
                maxlength: "Phone number cannot exceed 15 digits."
            },
            DateOfBirth: {
                required: "Please enter your date of birth.",
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            const formattedPhoneNumber = phoneInput.getNumber(intlTelInputUtils.numberFormat.E164);
            formData.set("PhoneNumber", formattedPhoneNumber); 
            const btnRegister = $("#btnMoreDetails");
            const btnCancel = $("#btnCancel");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnCancel.prop("hidden", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                // AJAX submission
                $.ajax({
                    url: '/Auth/MoreDetails',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            if (result.path != null) {
                                window.location.href = '/' + result.path;
                            }
                            else {
                                window.location.href = '/Auth/ShopDetails';
                            }
                        }
                        else {
                            showToast(result.message, "error")
                        }
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnCancel.prop("hidden", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast("Unknown error occurred", "error")
                    }
                });
            }, 2000);
        }
    });


    $("#companyRegistration").validate({
        rules: {
            CompanyName: {
                required: true,
                maxlength: 30
            },
            Email: {
                required: true,
                email: true
            },
            City: {
                required: true,
            },
            State: {
                required: true,
            },
            PhoneNumber: {
                required: true,
                minlength: 6,
                maxlength: 15
            },
            PasswordHash: {
                required: true,
            },
            Pincode: {
                required: true,
            },
            Address: {
                required: true,
            },
        },
        messages: {
            CompanyName: {
                required: "Company Name is Required",
                maxlength: "Name must not be greater than 30 letters"
            },
            Email: {
                required: "Email is Required",
                email: "Not a valid email"
            },
            State: {
                required: "Select a state",
            },
            City: {
                required: "Select a city",
            },
            PhoneNumber: {
                required: "Phone Number is required",
                minlength: "Must be greater than 6 digit",
                maxlength: "Must be smaller than 15 digit"
            },
            PasswordHash: {
                required: "Password is Required",
            },
            Pincode: {
                required: "Email is Required",
            },
            Address: {
                required: "Address is Required",
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            const formattedPhoneNumber = phoneInput.getNumber(intlTelInputUtils.numberFormat.E164);
            formData.set("PhoneNumber", formattedPhoneNumber);
            const btnRegister = $("#btnMoreDetails");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
                // AJAX submission
                $.ajax({
                    url: '/Auth/CompanyRegistration',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            window.location.href = '/';
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
                        showToast("Unknown error occurred", "success")
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
