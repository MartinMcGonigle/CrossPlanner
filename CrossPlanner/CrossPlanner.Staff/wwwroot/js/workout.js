document.addEventListener('DOMContentLoaded', () => {
    tinymce.init({
        selector: '#description',
        plugins: [
            'advlist', 'autolink',
            'lists', 'link', 'image', 'charmap', 'preview', 'anchor', 'searchreplace', 'visualblocks',
            'fullscreen', 'insertdatetime', 'media', 'table', 'help', 'wordcount'
        ],
        toolbar: 'undo redo | a11ycheck casechange blocks | bold italic backcolor | alignleft aligncenter alignright alignjustify |' +
            'bullist numlist checklist outdent indent | removeformat | code table help',
        setup: function (editor) {
            editor.on('input', function () {
                validateForm();
            });
        }
    });

    const classType = document.getElementById('class-type-id');
    const workoutDate = document.getElementById('workout-date');
    const submitButton = document.getElementById('workout-submit');

    function validateForm() {
        const isDescriptionFilled = tinymce.get('description').getContent().trim() !== '';
        const isClassTypeSelected = classType.value !== '';
        const isWorkoutDateFilled = workoutDate.value !== '';

        submitButton.disabled = !(isDescriptionFilled && isClassTypeSelected && isWorkoutDateFilled);
    }

    classType.addEventListener('change', validateForm);
    workoutDate.addEventListener('input', validateForm);

    validateForm();
});
