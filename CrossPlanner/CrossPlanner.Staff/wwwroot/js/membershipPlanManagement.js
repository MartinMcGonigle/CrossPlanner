document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.toggle-membership-plan').forEach(button => {
        button.addEventListener('click', function () {
            const membershipPlanId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/MembershipPlan/ToggleMembershipPlan?membershipPlanId=${membershipPlanId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Membership Plan Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch (error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to toggle membership plan. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });

    const statusSearch = document.getElementById('statusSearch');
    const typeSearch = document.getElementById('typeSearch');
    const q = document.getElementById('q');
    const filterForm = document.getElementById('filterForm');
    const clearFilters = document.getElementById('clear-filters');

    const filterChange = () => {
        if (statusSearch.value != '0' ||
            typeSearch.value != '0' ||
            q.value != '') {
            clearFilters.classList.remove('collapse');
        } else {
            clearFilters.classList.add('collapse');
        }
    };

    const clearAllFilters = () => {
        statusSearch.value = '0';
        typeSearch.value = '0';
        q.value = '';
        filterForm.submit();
    };

    statusSearch.addEventListener('change', filterChange);
    typeSearch.addEventListener('change', filterChange);
    q.addEventListener('input', filterChange);
    clearFilters.addEventListener('click', clearAllFilters);

    filterChange();
});