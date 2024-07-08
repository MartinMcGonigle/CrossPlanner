document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('account');
    const emailInput = document.getElementById('email-input');
    const forgotSubmit = document.getElementById('forgot-submit');

    function forgotSubmitState() {
        const isValid = form.checkValidity();
        const isEmailNotEmpty = emailInput.value !== "";


        if (isValid && isEmailNotEmpty) {
            forgotSubmit.removeAttribute('disabled');
        }
        else {
            forgotSubmit.setAttribute('disabled', 'disabled');
        }
    };

    emailInput.addEventListener('input', forgotSubmitState);
    forgotSubmitState();
});