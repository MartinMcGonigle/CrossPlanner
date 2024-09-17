document.addEventListener('DOMContentLoaded', () => {
    tinymce.init({
        selector: '#message',
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

    const title = document.getElementById('title');
    const message = document.getElementById('message');
    const checkboxes = document.querySelectorAll('.user-checkbox');
    const notificationSubmit = document.getElementById('notification-submit');

    function validateForm() {
        const isTitleFilled = title.value.trim() !== "";
        const isMessageFilled = tinymce.get('message').getContent().trim() !== '';
        const isAnyChecked = Array.from(checkboxes).some(checkbox => checkbox.checked);

        if (isTitleFilled && isMessageFilled && isAnyChecked) {
            notificationSubmit.disabled = false;
        } else {
            notificationSubmit.disabled = true;
        }
    }

    title.addEventListener('input', validateForm);
    checkboxes.forEach(checkbox => {
        checkbox.addEventListener('change', validateForm)
    });

    validateForm();
});