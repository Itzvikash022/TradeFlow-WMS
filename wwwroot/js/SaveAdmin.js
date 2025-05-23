﻿$(document).ready(function () {
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

        // Function to get URL query parameters
        function getQueryParam(param) {
            let urlParams = new URLSearchParams(window.location.search);
            return urlParams.get(param);
        }

        // Get the 'from' URL parameter or default to "employee"
        let formSource = getQueryParam("from") || "employee"; // Default to "employee"

        // If the form is being accessed from "admin" or it's an Admin form, set the role to 2 (Admin)
        if (formSource === "admin") {
            $("#RoleId").val(2); // Set the role to Admin (Admin role = 2)
            $("#designationDropdown").find("select").prop("disabled", true); // Disable the designation dropdown
            $("#designationDropdown").hide();
            $("#massEmployee").hide();
            $("#UserHeadingEmployee").hide();
            $("#CancelEmployee").hide();
        } else {
            // If it's Employee, show the designation dropdown and enable it
            $("#designationDropdown").find("select").prop("disabled", false);
            $("#designationDropdown").show();   
            $("#massEmployee").show();   
            $("#UserHeadingAdmin").hide();   
            $("#CancelAdmin").hide();   

            // Pre-select designation if RoleId is already set (in case of edit)
            let currentRoleId = $("#RoleId").val();
            if (formSource !== "admin" && currentRoleId) {
                $("#designation").val(currentRoleId);
            }

        }

        // Handle designation change (only for Employee role)
        $("#designation").change(function () {
            let selectedDesignation = $(this).val();
            if (selectedDesignation) {
                $("#RoleId").val(selectedDesignation); // Set role to the selected designation ID (e.g., 3 for Manager)
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

                // AJAX submission
                $.ajax({
                    url: '/Admins/SaveAdmin',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        if (result.success) {
                            window.location.href = '/' + result.role;
                        } else {
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
