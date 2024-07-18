document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('createForm');

    const inputs = {
        firstName: document.getElementById('first-name'),
        lastName: document.getElementById('last-name'),
        dateOfBirth: document.getElementById('date-of-birth'),
        email: document.getElementById('email'),
        role: document.getElementById('role')
    };

    const createSubmit = document.getElementById('create-submit');

    const isValidLength = (input, minLength) => input.length >= minLength;
    const notFutureDate = (date) => new Date(date) <= new Date();

    const validateForm = () => {
        const isFormValid = form.checkValidity();

        if (isFormValid) {
            const validations = [
                isValidLength(inputs.firstName.value, 1),
                isValidLength(inputs.lastName.value, 1),
                notFutureDate(inputs.dateOfBirth.value),
                isValidLength(inputs.email.value, 1),
                isValidLength(inputs.role.value, 1),
            ];

            if (validations.every(Boolean)) {
                createSubmit.removeAttribute('disabled');
            } else {
                createSubmit.setAttribute('disabled', 'disabled');
            }
        } else {
            createSubmit.setAttribute('disabled', 'disabled')
        }
    };


    Object.values(inputs).forEach(input => {
        input.addEventListener('input', validateForm);
    });

    validateForm();
});