$(document).ready(function () {
    const adminId = document.getElementById("AdminId").value;

    // For Reject button
    $("#btnReject").on("click", function () {
        const data = {
            AdminId: adminId,
            Status: false
        };

        $.ajax({
            url: '/Admins/UpdateStatus',  // Ensure the URL is correct
            type: 'POST',
            data: data,
            success: function (result) {
                alert(result.message); // Notify the user
                if (result.success) {
                    window.location.reload(); // Reload the page after successful update
                }
            },
            error: function () {
                alert('An error occurred while updating the status.');
            }
        });
    });

    // For Accept button
    $("#btnAccept").on("click", function () {
        const data = {
            AdminId: adminId,
            Status: true
        };

        $.ajax({
            url: '/Admins/UpdateStatus',  // Ensure the URL is correct
            type: 'POST',
            data: data,
            success: function (result) {
                alert(result.message); // Notify the user
                if (result.success) {
                    window.location.reload(); // Reload the page after successful update
                }
            },
            error: function () {
                alert('An error occurred while updating the status.');
            }
        });
    });
});