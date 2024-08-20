document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.toggle-class-type').forEach(button => {
        button.addEventListener('click', function () {
            const classTypeId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/ClassType/ToggleClassType?classTypeId=${classTypeId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Class Type Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to toggle class type. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                });
        });
    });

    document.querySelectorAll('.delete-class-type').forEach(button => {
        button.addEventListener('click', function () {
            const classTypeId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/ClassType/DeleteClassType?classTypeId=${classTypeId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Class Type Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to delete class type. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                });
        });
    });
});