document.addEventListener('DOMContentLoaded', () => {

    document.querySelectorAll('.deactivate-account').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/User/DeactivateAccount?userId=${userId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })
                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Deactivation Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to deactivate the account. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });

    document.querySelectorAll('.resend-verification').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.dataset.id;
            const userRole = this.dataset.role;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/User/ResendVerification?userId=${userId}&role=${userRole}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })
                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Verification Email Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to resend verification email. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                });
        });
    });

    document.getElementById('add-existing-user').addEventListener('click', function (event) {
        event.preventDefault();

        fetch(`/User/GetDeactivatedAccounts`)
       
            .then(response => response.json())

                .then(data => {

                if (data.length === 0) {
                    $('#feedbackModalTitle').text('Add Existing User Status');
                    $('#feedbackModalBody p').text('There are no deactivated users available at this time. Please try again later or contact support for further assistance.');
                    $('#feedbackModal').modal('show');
                } else {

                    const userDetailsTable = document.getElementById('user-details-table');
                    userDetailsTable.innerHTML = '';

                    data.forEach(user => {
                        const row = userDetailsTable.insertRow();
                        row.insertCell(0).textContent = user.firstName + ' ' + user.lastName;
                        row.insertCell(1).textContent = user.email;
                        const actionCell = row.insertCell(2);
                        const addButton = document.createElement('button');
                        addButton.textContent = 'Add';
                        addButton.className = 'btn btn-sm btn-green';
                        addButton.onclick = function () {
                            window.location.href = `/User/ReviewUserDetails?userId=${user.id}`;
                        };
                        actionCell.appendChild(addButton);
                    });

                    initializeUserDetailsTableSearch();
                    $('#userSelectionModal').modal('show');
                }
            })

            .catch(error => {
                $('#feedbackModalTitle').text('Error');
                $('#feedbackModalBody p').text('Failed to retrieve deactivated accounts. Please try again.');
                $('#feedbackModal').modal('show');
            });
    });

    function initializeUserDetailsTableSearch () {
        const searchUserDetailsTable = document.getElementById('search-user-details-table');
        const userDetailsTableRows = document.querySelectorAll('#user-details-table tr');

        const performUserSearch = () => {
            const searchValue = searchUserDetailsTable.value.toLowerCase().trim();
            let visibleRows = 0;

            userDetailsTableRows.forEach(row => {
                const text = row.textContent.toLowerCase();
                const isVisible = searchValue.split(/\s+/).every(part => text.includes(part));
                row.style.display = isVisible ? '' : 'none';
                if (isVisible) visibleRows++;
            });

            document.getElementById('user-selection-modal-no-results').style.display = visibleRows > 0 ? 'none' : 'block';
        };

        searchUserDetailsTable.addEventListener('input', performUserSearch);
    }

    initializeUserDetailsTableSearch();
});