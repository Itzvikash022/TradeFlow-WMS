$(document).ready(function () {

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


    const username = "itzvikash"; // Replace with your Geonames username
    const stateGeonameId = 1269750; // ID for India

    // Fetch States
    fetch((`/${controller}/states`))
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            const states = data.geonames;
            let stateOptions = `<option value="">Select State</option>`;
            states.forEach(state => {
                // Store name as the value and geonameId in data-id
                stateOptions += `<option value="${state.name}" data-id="${state.geonameId}">${state.name}</option>`;
            });
            $("#State").html(stateOptions);
        })
        .catch(error => {
            console.error("Error fetching states:", error);
        });

    // Fetch Cities when a state is selected
    $("#State").on("change", function () {
        const selectedStateGeonameId = $(this).find("option:selected").data("id"); // Get geonameId from data-id

        if (selectedStateGeonameId) {
            fetch(`cities/${selectedStateGeonameId}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`HTTP error! status: ${response.status}`);
                    }
                    return response.json();
                })
                .then(data => {
                    const cities = data.geonames;
                    let cityOptions = `<option value="">Select City</option>`;
                    cities.forEach(city => {
                        // Store city name as the value and optionally include geonameId in data-id
                        cityOptions += `<option value="${city.name}" data-id="${city.geonameId}">${city.name}</option>`;
                    });
                    $("#City").html(cityOptions);
                })
                .catch(error => {
                    console.error("Error fetching cities:", error);
                });
        } else {
            $("#City").html(`<option value="">Select City</option>`); // Reset city dropdown
        }
    });

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

    $("#SaveCompany").validate({
        rules: {
            CompanyName: {
                required: true,
            },
            ContactPerson: {
                required: true,
            },
            PhoneNumber: {
                required: true,
            },
            Email: {
                email: true,
                required: true,
            },
            State: {
                required: true,
            },
            City: {
                required: true,
            },
            Pincode: {
                required: true,
            },
            Address: {
                required: true,
            }
        },
        messages: {
            CompanyName: {
                required: "Company name cannot be empty",
            },
            ContactPerson: {
                required: "Contact Person name cannot be empty",
            },
            PhoneNumber: {
                required: "Enter Phone Number",
            },
            Email: {
                email: "Not a valid email",
                required: "Email must be given",
            },
            State: {
                required: "Select a state",
            },
            City: {
                required: "Select a city",
            },
            Pincode: {
                required: "Enter pincode",
            },
            Address: {
                required: "Enter address",
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
                    url: '/Company/SaveCompany',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            window.location.href = '/Company';
                        }
                    },
                    complete: function () {
                        // Re-enable button and hide loader
                        btnRegister.prop("disabled", false);
                        btnLoader.addClass("d-none");
                    },
                    error: function () {
                        showToast('An error occurred while registering the company.');
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

});
