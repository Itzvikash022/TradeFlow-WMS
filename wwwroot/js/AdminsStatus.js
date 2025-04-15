$(document).ready(function () {
    const userId = document.getElementById("UserId").value;

    // For Reject button
    $("#btnReject").on("click", function () {
        const remark = $("#remarkInput").val().trim(); // Get remark input value
        if (!remark) {
            showToast("Please provide a remark before approving.", "warning");
            return;
        }

        const data = {
            UserId: userId,
            Status: false,
            Remark: remark
        };
        const btnRegister = $("#btnReject");
        const btnLoader = $("#btnLoader");
        btnRegister.prop("disabled", true);
        btnLoader.removeClass("d-none");
        $.ajax({
            url: '/Admins/UpdateStatus',  // Ensure the URL is correct
            type: 'POST',
            data: data,
            success: function (result) {
                if (result.success) {
                    showToast(result.message, "success"); // Notify the user
                    setTimeout(function () {
                        location.reload(); // Reload the page after 2 seconds
                    }, 1000);// Reload the page after successful update
                }
                else {
                    showToast(result.message,"error"); // Notify the user
                }
            },
            complete: function () {
                // Re-enable button and hide loader
                btnRegister.prop("disabled", false);
                btnLoader.addClass("d-none");
            },
            error: function () {
                showToast('An error occurred while updating the status.');
            }
        });
    });

    // For Accept button
    $("#btnAccept").on("click", function () {
        const remark = $("#remarkInput").val().trim(); // Get remark input value
        if (!remark) {
            showToast("Please provide a remark before approving.","warning");
            return;
        }

        const data = {
            UserId: userId,
            Status: true,
            Remark: remark
        };
        const btnRegister = $("#btnAccept");
        const btnLoader = $("#btnLoader");
        btnRegister.prop("disabled", true);
        btnLoader.removeClass("d-none");

        $.ajax({
            url: '/Admins/UpdateStatus',  // Ensure the URL is correct
            type: 'POST',
            data: data,
            success: function (result) {
                if (result.success) {
                    showToast(result.message, "success"); // Notify the user
                    setTimeout(function () {
                        location.reload(); // Reload the page after 2 seconds
                    }, 1000); // Reload the page after successful update
                }
                else {
                    showToast(result.message,"error"); // Notify the user
                }
            },
            complete: function () {
                // Re-enable button and hide loader
                btnRegister.prop("disabled", false);
                btnLoader.addClass("d-none");
            },
            error: function () {
                showToast('An error occurred while updating the status.',"error");
            }
        });
    });
});