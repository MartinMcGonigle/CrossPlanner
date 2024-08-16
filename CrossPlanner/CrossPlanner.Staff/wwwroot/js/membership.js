document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.cancel-membership').forEach(button => {
        button.addEventListener('click', function () {
            const membershipId = this.dataset.membershipid;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/Membership/CancelMembership?membershipId=${membershipId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Membership Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to cancel membership. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });
});