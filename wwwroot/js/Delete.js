let adminIdToDelete = null;  // Variable to store the ID of the admin to delete

// When delete button is clicked, show the modal
$(".delete-btn").click(function () {
    adminIdToDelete = $(this).data("id");  // Get the admin ID
    $("#adminIdToDelete").val(adminIdToDelete);  // Set the ID in the hidden input

    // Show the modal
    $("#deleteModal").fadeIn();
});

function initializeDeleteButtons(deleteApiUrl) {
    deleteUrl = deleteApiUrl;

    // Handle deletion when the "Yes" button is clicked
    $("#confirmDeleteBtn").click(function () {
        if (adminIdToDelete !== null) {
            // Perform the delete operation using AJAX
            $.ajax({
                url: deleteUrl,
                type: 'POST',
                data: { id: adminIdToDelete },
                success: function (response) {
                    if (response.success) {
                        Swal.fire("Deleted!", response.message, "success");
                        $("#deleteModal").fadeOut();  // Hide the modal after deletion
                        adminIdToDelete = null;  // Reset admin ID
                        //window.location.reload(); // Reload the page after successful update

                    } else {
                        Swal.fire("Error!", response.message, "error");
                    }
                },
                error: function () {
                    Swal.fire("Error!", "Something went wrong while deleting.", "error");
                    $("#deleteModal").fadeOut();
                    adminIdToDelete = null;  // Reset admin ID
                }
            });
        }
    });
}

// Hide the modal when the "No" button is clicked
$("#cancelDeleteBtn").click(function () {
    $("#deleteModal").fadeOut();  // Hide the modal
    adminIdToDelete = null;  // Reset admin ID
});
