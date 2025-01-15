$(document).ready(function () {
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
            url: '/Home/OtpCheck',
            type: 'POST',
            data: formData,
            success: function (result) {
                alert(result.message);
                if (result.success) {
                    window.location.href = '/Home/MoreDetails';
                }
            },
            error: function () {
                alert('An error occurred while registering the user.');
            }
        });
    })
});


