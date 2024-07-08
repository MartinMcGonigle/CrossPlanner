document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById('account');
    const passwordInput = document.getElementById('password-input');
    const confirmPasswordInput = document.getElementById('confirm-password-input');
    const changeSubmit = document.getElementById('change-submit');

    const validatePassword = (password) => {
        const hasDigit = /\d/.test(password);
        const hasLowercase = /[a-z]/.test(password);
        const hasUpperCase = /[A-Z]/.test(password);
        const hasNonAlphanumeric = /[\W_]/.test(password);
        const isValidLength = password.length >= 7;

        return hasDigit && hasLowercase && hasUpperCase && hasNonAlphanumeric && isValidLength;
    };

    const changeSubmitBtnState = () => {
        const isFormValid = form.checkValidity();
        const isPasswordNotEmpty = passwordInput.value !== "";
        const isConfirmPasswordNotEmpty = confirmPasswordInput.value !== "";
        const arePasswordsMatching = passwordInput.value === confirmPasswordInput.value;

        if (isFormValid && isPasswordNotEmpty && isConfirmPasswordNotEmpty && arePasswordsMatching && validatePassword(passwordInput.value)) {
            changeSubmit.removeAttribute("disabled");
        } else {
            changeSubmit.setAttribute("disabled", "disabled");
        }
    };

    passwordInput.addEventListener('input', changeSubmitBtnState);
    confirmPasswordInput.addEventListener('input', changeSubmitBtnState);

    changeSubmitBtnState();
});
