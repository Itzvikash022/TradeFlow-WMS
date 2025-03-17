let userIdToDelete = null;  // Variable to store the ID of the admin to delete

// When delete button is clicked, show the modal
$(".delete-btn").click(function () {
    userIdToDelete = $(this).data("id");  // Get the admin ID
    $("#userIdToDelete").val(userIdToDelete);  // Set the ID in the hidden input

    // Show the modal
    $("#deleteModal").fadeIn();
});

function initializeDeleteButtons(deleteApiUrl) {
    deleteUrl = deleteApiUrl;

    // Handle deletion when the "Yes" button is clicked
    $("#confirmDeleteBtn").click(function () {
        if (userIdToDelete !== null) {
            // Perform the delete operation using AJAX
            $.ajax({
                url: deleteUrl,
                type: 'POST',
                data: { id: userIdToDelete },
                success: function (response) {
                    if (response.success) {
                        //Swal.fire("Deleted!", response.message, "success");
                        Swal.fire({
                            title: "Deleted!",
                            text: response.message,
                            icon: "success",
                            confirmButtonText: "OK"
                        }).then(() => {
                            location.reload(); // Reload the page after clicking OK
                        });
                        $("#deleteModal").fadeOut();  // Hide the modal after deletion
                        userIdToDelete = null;  // Reset admin ID

                        //window.location.reload(); // Reload the page after successful update

                    } else {
                        Swal.fire("Error!", response.message, "error");
                    }
                },
                error: function () {
                    Swal.fire("Error!", "Something went wrong while deleting.", "error");
                    $("#deleteModal").fadeOut();
                    userIdToDelete = null;  // Reset admin ID
                }
            });
        }
    });
}

// Hide the modal when the "No" button is clicked
$("#cancelDeleteBtn").click(function () {
    $("#deleteModal").fadeOut();  // Hide the modal
    userIdToDelete = null;  // Reset admin ID
});
