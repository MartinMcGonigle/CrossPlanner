document.addEventListener('DOMContentLoaded', () => {
    const linkedToGymSearch = document.getElementById('linkedToGymSearch');
    const emailConfirmedSearch = document.getElementById('emailConfirmedSearch');
    const activeMembershipSearch = document.getElementById('activeMembershipSearch');
    const roleSearch = document.getElementById('roleSearch');
    const q = document.getElementById('q');
    const clearFilters = document.getElementById('clear-filters');
    const filterForm = document.getElementById('filterForm');

    const filterChange = () => {
        const linkedToGymSearchValue = linkedToGymSearch.value;

        if (linkedToGymSearchValue == '0' || linkedToGymSearchValue == '2') {
            emailConfirmedSearch.value = '0';
            activeMembershipSearch.value = '0';
            roleSearch.value = '0';
        }

        if (linkedToGymSearch.value != '0' ||
            emailConfirmedSearch.value != '0' ||
            activeMembershipSearch.value != '0' ||
            roleSearch.value != '0' ||
            q.value != '') {
            clearFilters.classList.remove('collapse');
        } else {
            clearFilters.classList.add('collapse');
        }
    };

    const clearAllFilters = () => {
        linkedToGymSearch.value = '0';
        emailConfirmedSearch.value = '0';
        activeMembershipSearch.value = '0';
        roleSearch.value = '0';
        q.value = '';
        filterForm.submit();
    };

    linkedToGymSearch.addEventListener('change', filterChange);
    emailConfirmedSearch.addEventListener('change', filterChange);
    activeMembershipSearch.addEventListener('change', filterChange);
    roleSearch.addEventListener('change', filterChange);
    q.addEventListener('input', filterChange);
    clearFilters.addEventListener('click', clearAllFilters);

    $('[data-bs-toggle="tooltip"]').tooltip();
    filterChange();
});