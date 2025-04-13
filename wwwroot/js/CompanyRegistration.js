

$(document).ready(function () {


    //For Company registration
    document.getElementById('LogoFile').addEventListener('change', function (event) {
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

    //Password hide unhide
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

    // Grab current controller name from URL
    const segments = window.location.pathname.split('/').filter(Boolean);
    const controller = segments[0];  // 'Shops', 'Auth', 'Companies', etc.

    const selectedState = $("#State").attr("data-selected");
    const selectedCity = $("#City").attr("data-selected");

    // Fetch States
    fetch(`/${controller}/states`)
        .then(response => response.json())
        .then(data => {
            const states = data.geonames;
            let stateOptions = `<option value="">Select State</option>`;
            states.forEach(state => {
                stateOptions += `<option value="${state.name}" data-id="${state.geonameId}" 
                ${state.name === selectedState ? "selected" : ""}>${state.name}</option>`;
            });
            $("#State").html(stateOptions);

            // Trigger city fetch if state is preselected
            if (selectedState) {
                $("#State").trigger("change");
            }
        })
        .catch(error => console.error("Error fetching states:", error));

    // Fetch Cities when a state is selected
    $("#State").on("change", function () {
        const selectedStateGeonameId = $(this).find("option:selected").data("id");

        if (selectedStateGeonameId) {
            fetch(`/${controller}/cities/${selectedStateGeonameId}`)
                .then(response => response.json())
                .then(data => {
                    const cities = data.geonames;
                    let cityOptions = `<option value="">Select City</option>`;
                    cities.forEach(city => {
                        cityOptions += `<option value="${city.name}" data-id="${city.geonameId}" 
                        ${city.name === selectedCity ? "selected" : ""}>${city.name}</option>`;
                    });
                    $("#City").html(cityOptions);
                })
                .catch(error => console.error("Error fetching cities:", error));
        } else {
            $("#City").html(`<option value="">Select City</option>`); // Reset city dropdown
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
            const btnRegister = $("#btnCompanyRegistration");
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
                            if (result.path != null) {
                                window.location.href = '/' + result.path;
                            }
                            else {
                                window.location.href = '/Auth/CompanyLogin';
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
                        showToast("Unknown error occurred", "error")
                    }
                });
        }
    });

});

