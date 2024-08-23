document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('scheduledClass');

    const inputs = {
        classTypeId: document.getElementById('class-type-id'),
        instructorId: document.getElementById('instructor-id'),
        startDateTime: document.getElementById('start-date-time'),
        endDateTime: document.getElementById('end-date-time'),
        capacity: document.getElementById('capacity')
    };

    const scheduledClassSubmit = document.getElementById('scheduled-class-submit');

    const validateCapacity = () => {
        if (inputs.capacity.value) {
            return inputs.capacity.value > 0;
        }

        return true;
    }

    const validateDateTimes = () => {
        const startDateTime = new Date(inputs.startDateTime.value);
        const endDateTime = new Date(inputs.endDateTime.value);
        const dateTimeNow = new Date();

        if (startDateTime < dateTimeNow || endDateTime < dateTimeNow) {
            return false;
        }

        return endDateTime > startDateTime;
    }

    const isValidLength = (input, minLength) => input.length >= minLength;



    const validateForm = () => {
        const isFormValid = form.checkValidity();

        if (isFormValid) {
            const validations = [
                isValidLength(inputs.classTypeId.value, 1),
                isValidLength(inputs.instructorId.value, 1),
                validateDateTimes(),
                validateCapacity()
            ];

            if (validations.every(Boolean)) {
                scheduledClassSubmit.removeAttribute('disabled');
            }
            else {
                scheduledClassSubmit.setAttribute('disabled', 'disabled');
            }
        }
        else {
            scheduledClassSubmit.setAttribute('disabled', 'disabled');
        }
    };

    Object.values(inputs).forEach(input => {
        input.addEventListener('input', validateForm);
    });

    validateForm();
});