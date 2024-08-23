document.addEventListener('DOMContentLoaded', () => {
    $('form[id^="cancelClassForm-"]').on('submit', function (e) {
        e.preventDefault();

        const form = $(this);
        const scheduledClassId = form.find('input[name="ScheduledClassId"]').val();
        const cancellationReason = form.find('textarea[name="CancellationReason"]').val();
        const modalId = form.closest('.modal').attr('id');
        $('#' + modalId).modal('hide');

        $.ajax({
            /*url: '@Url.Action("CancelClass", "ScheduledClass")',*/
            url: 'ScheduledClass/CancelClass',
            type: 'PUT',
            data: {
                scheduledClassId: scheduledClassId,
                cancellationReason: cancellationReason
            },
            success: function (data) {
                $('#feedbackModalTitle').text('Cancel Class Status');
                $('#feedbackModalBody p').text(data.message);
                $('#feedbackModal').modal('show');

                $('#feedbackModal').on('hidden.bs.modal', function () {
                    if (data.success) {
                        window.location.reload();
                    }
                });
            },
            error: function (xhr, status, error) {
                $('#feedbackModalTitle').text('Error');
                $('#feedbackModalBody p').text('Failed to cancel the class. Please try again.');
                $('#feedbackModal').modal('show');

                $('#feedbackModal').on('hidden.bs.modal', function () {
                    window.location.reload();
                });
            }
        });
    });

    $('[id^="cancelModal-"]').on('show.bs.modal', function (e) {
        const modal = $(this);
        modal.find('textarea').val(''); // Clear the textarea
    });

    document.querySelectorAll('.toggle-scheduled-class').forEach(button => {
        button.addEventListener('click', function () {
            const scheduledClassId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/ScheduledClass/ToggleScheduledClass?scheduledClassId=${scheduledClassId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Scheduled Class Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to toggle scheduled class. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });
});
