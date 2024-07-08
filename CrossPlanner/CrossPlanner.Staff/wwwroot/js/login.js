document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('account');
    const emailInput = document.getElementById('email-input');
    const passwordInput = document.getElementById('password-input');
    const loginSubmit = document.getElementById('login-submit');

    function loginBtnState() {
        const isValid = form.checkValidity();
        const isEmailNotEmpty = emailInput.value !== "";
        const isPasswordNotEmpty = passwordInput.value !== "";

        if (isValid && isEmailNotEmpty && isPasswordNotEmpty) {
            loginSubmit.removeAttribute('disabled');
        } else {
            loginSubmit.setAttribute('disabled', 'disabled');
        }
    }

    emailInput.addEventListener('input', loginBtnState);
    passwordInput.addEventListener('input', loginBtnState);

    loginBtnState();
});