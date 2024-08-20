document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('classType');
    const titleInput = document.getElementById('title');
    const classTypeSubmit = document.getElementById('class-type-submit');

    const isValidLength = (input, minLength) => input.length >= minLength;

    const validateForm = () => {
        const isFormValid = form.checkValidity();

        if (isFormValid) {
            const isTitleValid = isValidLength(titleInput.value, 3);

            if (isTitleValid) {
                classTypeSubmit.removeAttribute('disabled');
            } else {
                classTypeSubmit.setAttribute('disabled', 'disabled');
            }
        } else {
            classTypeSubmit.setAttribute('disabled', 'disabled');
        }
    };

    titleInput.addEventListener('input', validateForm);
    validateForm();
});