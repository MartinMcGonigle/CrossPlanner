document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('editForm');

    const inputs = {
        firstName: document.getElementById('first-name'),
        lastName: document.getElementById('last-name'),
        dateOfBirth: document.getElementById('date-of-birth'),
        email: document.getElementById('email')
    };

    const editSubmit = document.getElementById('edit-submit');

    const isValidLength = (input, minLength) => input.length >= minLength;
    const notFutureDate = (date) => new Date(date) <= new Date();

    const validateForm = () => {
        const isFormValid = form.checkValidity();

        if (isFormValid) {
            const validations = [
                isValidLength(inputs.firstName.value, 1),
                isValidLength(inputs.lastName.value, 1),
                notFutureDate(inputs.dateOfBirth.value),
                isValidLength(inputs.email.value, 1)
            ];

            if (validations.every(Boolean)) {
                editSubmit.removeAttribute('disabled');
            } else {
                editSubmit.setAttribute('disabled', 'disabled');
            }
        } else {
            editSubmit.setAttribute('disabled', 'disabled');
        }
    };

    Object.values(inputs).forEach(input => {
        input.addEventListener('input', validateForm);
    });

    validateForm();
});