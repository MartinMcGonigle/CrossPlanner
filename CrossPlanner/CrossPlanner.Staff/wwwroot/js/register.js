document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('registerForm');

    const inputs = {
        affiliateName: document.getElementById('affiliate-name'),
        affiliateAddress: document.getElementById('affiliate-address'),
        affiliatePhoneNumber: document.getElementById('affiliate-phone-number'),
        affiliateEmail: document.getElementById('affiliate-email'),
        firstName: document.getElementById('first-name'),
        lastName: document.getElementById('last-name'),
        dateOfBirth: document.getElementById('date-of-birth'),
        email: document.getElementById('email'),
        password: document.getElementById('password'),
        confirmPassword: document.getElementById('confirm-password'),
    };

    const registerSubmit = document.getElementById('register-submit');

    const isValidLength = (input, minLength) => input.length >= minLength;
    const matchesPattern = (input, pattern) => pattern.test(input);
    const notFutureDate = (date) => new Date(date) <= new Date();
    const arePasswordsMatching = (password1, password2) => password1 === password2;

    const validatePassword = (password) => {
        const hasDigit = /\d/.test(password);
        const hasLowercase = /[a-z]/.test(password);
        const hasUpperCase = /[A-Z]/.test(password);
        const hasNonAlphanumeric = /[\W_]/.test(password);
        const isValidLength = password.length >= 7;

        return hasDigit && hasLowercase && hasUpperCase && hasNonAlphanumeric && isValidLength;
    };

    const validateForm = () => {
        const isFormValid = form.checkValidity();

        if (isFormValid) {
            const validations = [
                isValidLength(inputs.affiliateName.value, 1),
                isValidLength(inputs.affiliateAddress.value, 1),
                matchesPattern(inputs.affiliatePhoneNumber.value, /^\+?[0-9\s]{3,}$/),
                isValidLength(inputs.affiliateEmail.value, 1),
                isValidLength(inputs.firstName.value, 1),
                isValidLength(inputs.lastName.value, 1),
                notFutureDate(inputs.dateOfBirth.value),
                isValidLength(inputs.email.value, 1),
                validatePassword(inputs.password.value),
                arePasswordsMatching(inputs.password.value, inputs.confirmPassword.value),
            ];

            if (validations.every(Boolean)) {
                registerSubmit.removeAttribute('disabled');
            } else {
                registerSubmit.setAttribute('disabled', 'disabled');
            }
        } else {
            registerSubmit.setAttribute('disabled', 'disabled');
        }
    };

    Object.values(inputs).forEach(input => {
        input.addEventListener('input', validateForm);
    });
});