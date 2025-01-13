$(document).ready(function () {

    //// JavaScript to preview image
    //document.getElementById('ProfileImage').addEventListener('change', function (e) {
    //    var file = e.target.files[0];
    //    if (file) {
    //        var reader = new FileReader();
    //        reader.onload = function (event) {
    //            var imagePreview = document.getElementById('imagePreview');
    //            imagePreview.src = event.target.result;
    //            imagePreview.style.display = 'block';
    //        };
    //        reader.readAsDataURL(file);
    //    }
    //});

    //const phoneInputField = document.querySelector("#PhoneNumber");
    //phoneInput = window.intlTelInput(phoneInputField, {
    //    initialCountry: "IN", // Set initial country (auto or a specific code like "us")
    //    geoIpLookup: function (callback) {
    //        fetch('https://ipapi.co/json', { mode: 'no-cors' })
    //            .then((response) => response.json())
    //            .then((data) => callback(data.country_code))
    //            .catch(() => callback("us"));
    //    },
    //    utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.8/js/utils.js"
    //});

    //$('#roleEmployee').on('change', function () {
    //    $('#AdminRef').removeClass('visually-hidden');
    //    $('#AdminReference').prop('disabled', false); // Enable the AdminRef input
    //});

    //$('#roleAdmin').on('change', function () {
    //    $('#AdminRef').addClass('visually-hidden');
    //    $('#AdminReference').prop('disabled', true); // Enable the AdminRef input
    //});

    // Event listener for the AdminReference input field
    //$('#AdminReference').on('input', function () {
    //    var inputValue = $(this).val().toLowerCase();

    //    // If the input value is not empty, filter the adminUsernames
    //    if (inputValue.length > 0) {
    //        var matchingUsernames = adminUsernames.filter(function (username) {
    //            return username.toLowerCase().includes(inputValue);
    //        });

    //        $('#displayAdmin').empty();

    //        // If there are matching usernames, display them
    //        if (matchingUsernames.length > 0) {
    //            matchingUsernames.forEach(function (username) {
    //                var suggestionDiv = $('<div>')
    //                    .addClass('suggestion-item')
    //                    .text(username)
    //                    .css({ padding: '8px', cursor: 'pointer' });

    //                $('#displayAdmin').append(suggestionDiv);
    //            });

    //            $('#displayAdmin').show();
    //        } else {
    //            $('#displayAdmin').empty().hide();
    //        }
    //    } else {
    //        $('#displayAdmin').empty().hide();
    //    }
    //});

    // Event listener to handle clicking on a suggestion
    //$('#displayAdmin').on('click', '.suggestion-item', function () {
    //    var selectedUsername = $(this).text();
    //    $('#AdminReference').val(selectedUsername);
    //    $('#displayAdmin').empty().hide();
    //});

    //// Hide suggestions if the user clicks anywhere outside the input or suggestion container
    //$(document).on('click', function (e) {
    //    if (!$(e.target).closest('#AdminReference').length && !$(e.target).closest('#displayAdmin').length) {
    //        $('#displayAdmin').empty().hide();
    //    }
    //});

    // Event listener to handle clicking on a suggestion
    //$('#displayAdmin').on('click', '.suggestion-item', function () {
    //    var selectedUsername = $(this).text();

    //    // Set the value of the AdminReference input to the selected username
    //    $('#AdminReference').val(selectedUsername);
    //    $('#displayAdmin').empty().hide();
    //});

    //// Hide the suggestions if the user clicks anywhere outside the input or suggestion container
    //$(document).on('click', function (e) {
    //    if (!$(e.target).closest('#AdminReference').length && !$(e.target).closest('#displayAdmin').length) {
    //        $('#displayAdmin').empty().hide();
    //    }
    //});


    $("#SignUpForm").validate({
        rules: {
            Username: {
                required: true,
                minlength: 3,
                maxlength: 20
            },
            //FirstName: {
            //    required: false,
            //    lettersOnly: true, // Custom method for letters only
            //    maxlength: 20
            //},
            //LastName: {
            //    lettersOnly: true // Custom method for letters only
            //},
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
            //PhoneNumber: {
            //    required: true,
            //    digits: true,
            //    minlength: 6,
            //    maxlength: 15
            //},
            //Role: {
            //    required: true
            //},
            //DateOfBirth: {
            //    required: true,
            //    dateBeforeToday: true
            //},
            //Designation: {
            //    required: true
            //},
            //AdminRef: {
            //    required: function () {
            //        return $("#Role").val() === "Employee";
            //    }
            //}
        },
        messages: {
            Username: {
                required: "Please enter a username.",
                minlength: "Username must be at least 3 characters.",
                maxlength: "Username cannot exceed 15 characters."
            },
            //FirstName: {
            //    required: "Please enter your first name."
            //},
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
            //PhoneNumber: {
            //    required: "Please enter your phone number.",
            //    digits: "Please enter only numbers.",
            //    minlength: "Phone number must be at least 6 digits.",
            //    maxlength: "Phone number cannot exceed 15 digits."
            //},
            //Role: {
            //    required: "Please select a role."
            //},
            //DateOfBirth: {
            //    required: "Please enter your date of birth.",
            //},
            //Designation: {
            //    required: "Please enter your designation."
            //},
            //AdminRef: {
            //    required: "Admin Reference is required for Employee role."
            //}
        },
        
        submitHandler: function (form, event) {
            event.preventDefault()
            // Collect form data
            //const formData = {
            //    Username: $('#Username').val(),
            //    FirstName: $('#FirstName').val(),
            //    LastName: $('#LastName').val(),
            //    Email: $('#Email').val(),
            //    PasswordHash: $('#PasswordHash').val(),
            //    PhoneNumber: $('#PhoneNumber').val(),
            //    Role: $('#Role').val(),
            //    DateOfBirth: $('#DateOfBirth').val(),
            //    Designation: $('#Designation').val(),
            //    AdminRef: $('#AdminRef').val()
            //};


            const formData = new FormData(form);


             


            // AJAX submission
            $.ajax({
                url: '/Home/Register',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Home/OtpCheck';
                    }
                },
                error: function () {
                    alert('An error occurred while registering the user.');
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
