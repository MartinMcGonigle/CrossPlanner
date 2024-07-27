﻿document.addEventListener('DOMContentLoaded', function () {
    const stripe = Stripe('pk_test_51PfKoV2LZAPH4Vf6rbHMetDjkM8geRJgH5oZXbicyjaGRrfwwQsWLXpvIoFOAFe885WP0zrhaREx9GYSGwvoE8SH00MBRJqFGl');
    let elements = stripe.elements();

    const style = {
        base: {
            color: '#32325d',
            lineHeight: '18px',
            fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
            fontSmoothing: 'antialiased',
            fontSize: '16px',
            '::placeholder': {
                color: '#aab7c4'
            }
        },

        invalid: {
            color: '#FF0000',
            iconColor: '#FF0000'
        }
    };

    let card = elements.create('card', { style: style });
    card.mount('#card-element');

    card.addEventListener('change', function (event) {
        const displayError = document.getElementById('card-errors');

        if (event.error) {
            displayError.textContent = event.error.message;
        } else {
            displayError.textContent = '';
        }
    });

    const form = document.getElementById('payment-form');

    form.addEventListener('submit', function (event) {
        event.preventDefault();
        stripe.createToken(card).then(function (result) {
            if (result.error) {
                const errorElement = document.getElementById('card-errors');
                errorElement.textContent = result.error.message;
            } else {
                stripeTokenHandler(result.token);
            }
        });
    });

    function stripeTokenHandler(token) {
        const form = document.getElementById('payment-form');
        const hiddenInput = document.createElement('input');
        hiddenInput.setAttribute('type', 'hidden');
        hiddenInput.setAttribute('name', 'stripeToken');
        hiddenInput.setAttribute('value', token.id);
        form.appendChild(hiddenInput);
        form.submit();
    }
});