$(document).ready(function () {

    const username = "itzvikash"; // Replace with your Geonames username
    const stateGeonameId = 1269750; // ID for India

    // Get previously selected state & city
    const selectedState = $("#State").attr("data-selected");
    const selectedCity = $("#City").attr("data-selected");

    // Fetch States
    fetch(`http://api.geonames.org/childrenJSON?geonameId=${stateGeonameId}&username=${username}`)
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
            fetch(`http://api.geonames.org/childrenJSON?geonameId=${selectedStateGeonameId}&username=${username}`)
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

    document.getElementById('ShopImage').addEventListener('change', function (event) {
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

    $("#shopDetails").validate({
        rules: {
            ShopName: {
                required: false,
                lettersOnly: true, // Custom method for letters only
                maxlength: 20
            },
            StartTime: {
                required: true,
            },
            ClosingTime: {
                required: true,
            },
            State: {
                required: true,
            },
            City: {
                required: true,
            },
            Address: {
                required: true,
            }
        },
        messages: {
            ShopName: {
                required: "Shop name is required",
                maxlength: "Max length is 20"
            },
            StartTime: {
                required: "Start Time is required",
            },
            ClosingTime: {
                required: "Closing time is required",
            },
            State: {
                required: "Select a state",
            },
            City: {
                required: "select a city",
            },
            Address: {
                required: "enter your full adress",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            const btnRegister = $("#btnShopDetails");
            const btnLoader = $("#btnLoader");
            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");
            setTimeout(function () {

                // AJAX submission
                $.ajax({
                    url: '/Auth/ShopDetails',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            if (result.path != null) {
                                window.location.href = '/' + result.path;
                            }
                            else {
                                window.location.href = '/Auth/AdminDoc';
                            }
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
