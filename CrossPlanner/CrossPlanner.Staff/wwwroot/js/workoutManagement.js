document.addEventListener('DOMContentLoaded', () => {

    document.querySelectorAll('.toggle-workout').forEach(button => {
        button.addEventListener('click', function () {
            const workoutId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/Workout/ToggleWorkout?workoutId=${workoutId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Workout Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to toggle workout. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });

    document.querySelectorAll('.delete-workout').forEach(button => {
        button.addEventListener('click', function () {
            const workoutId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/Workout/DeleteWorkout?workoutId=${workoutId}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Workout Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to delete workout. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });
});