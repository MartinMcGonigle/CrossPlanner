document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('membershipPlan');

    const inputs = {
        title: document.getElementById('title'),
        price: document.getElementById('price'),
        description: document.getElementById('description'),
        membershipType: document.getElementById('membershipType'),
        numberOfClasses: document.getElementById('numberOfClasses'),
        numberOfMonths: document.getElementById('numberOfMonths'),
    };

    const divs = {
        numberOfClassesDiv: document.getElementById('numberOfClassesDiv'),
        numberOfMonthsDiv: document.getElementById('numberOfMonthsDiv'),
    };

    const membershipPlanSubmit = document.getElementById('membership-plan-submit');

    const isValidLength = (input, minLength) => input.length >= minLength;

    const validateForm = () => {
        const isFormValid = form.checkValidity();

        if (isFormValid) {
            const validations = [
                isValidLength(inputs.title.value, 1),
                isValidLength(inputs.price.value, 1),
                isValidLength(inputs.description.value, 1),
                isValidLength(inputs.membershipType.value, 1),
            ];

            if (validations.every(Boolean)) {

                if (validateMembershipType()) {
                    membershipPlanSubmit.removeAttribute('disabled');
                }
                else {
                    membershipPlanSubmit.setAttribute('disabled', 'disabled');
                }
            } else {
                membershipPlanSubmit.setAttribute('disabled', 'disabled');
            }
        } else {
            membershipPlanSubmit.setAttribute('disabled', 'disabled');
        }
    };

    function validateMembershipType() {
        const membershipTypeValue = inputs.membershipType.value;

        if (membershipTypeValue == 1) {
            return isValidLength(inputs.numberOfClasses.value, 1);
        } else if (membershipTypeValue == 2) {
            return isValidLength(inputs.numberOfClasses.value, 1) && isValidLength(inputs.numberOfMonths.value, 1);
        }

        return true;
    }

    const membershipTypeChange = () => {
        const membershipTypeValue = inputs.membershipType.value;

        if (membershipTypeValue == 1) {
            divs.numberOfClassesDiv.classList.remove('collapse');
            divs.numberOfMonthsDiv.classList.add('collapse');

            $("#NumberOfClasses").prop('required', true);
            $("#NumberOfMonths").prop('required', false);

            inputs.numberOfMonths.value = '';
        } else if (membershipTypeValue == 2) {
            divs.numberOfClassesDiv.classList.remove('collapse');
            divs.numberOfMonthsDiv.classList.remove('collapse');

            $("#NumberOfClasses").prop('required', true);
            $("#NumberOfMonths").prop('required', true);
        } else {
            divs.numberOfClassesDiv.classList.add('collapse');
            divs.numberOfMonthsDiv.classList.add('collapse');

            $("#NumberOfClasses").prop('required', false);
            $("#NumberOfMonths").prop('required', false);

            inputs.numberOfClasses.value = '';
            inputs.numberOfMonths.value = '';
        }
    };

    inputs.membershipType.addEventListener('change', membershipTypeChange);
    membershipTypeChange();

    Object.values(inputs).forEach(input => {
        input.addEventListener('input', validateForm);
    });

    validateForm();
});