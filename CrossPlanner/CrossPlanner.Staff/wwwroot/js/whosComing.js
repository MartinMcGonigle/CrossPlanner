document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.mark-absent').forEach(button => {
        button.addEventListener('click', function () {
            const scheduledClassReservationId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/ScheduledClass/MarkAbsent?scheduledClassReservationId=${scheduledClassReservationId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Class Reservation Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to mark class reservation as absent. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })


        });
    });

    document.querySelectorAll('.mark-present').forEach(button => {
        button.addEventListener('click', function () {
            const scheduledClassReservationId = this.dataset.id;
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

            fetch(`/ScheduledClass/MarkPresent?scheduledClassReservationId=${scheduledClassReservationId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'X-CSRF-TOKEN': token
                }
            })

                .then(response => response.json())

                .then(data => {
                    $('#feedbackModalTitle').text('Class Reservation Status');
                    $('#feedbackModalBody p').text(data.message);
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })

                .catch(error => {
                    $('#feedbackModalTitle').text('Error');
                    $('#feedbackModalBody p').text('Failed to mark class reservation as present. Please try again.');
                    $('#feedbackModal').modal('show');

                    $('#feedbackModal').on('hidden.bs.modal', function () {
                        window.location.reload();
                    });
                })
        });
    });


    document.getElementById('add-attendee').addEventListener('click', function (event) {
        event.preventDefault();
        const scheduledClassId = this.dataset.id;
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        fetch(`/ScheduledClass/GetUnreservedMembers?scheduledClassId=${scheduledClassId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'X-CSRF-TOKEN': token
            }
        })

            .then(response => response.json())

            .then(data => {
                if (data.length === 0) {
                    $('#feedbackModalTitle').text('Add Attendee');
                    $('#feedbackModalBody p').text('There are no unreserved members available at this time. Please try again later or contact support for further assistance.');
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
                            $('#userSelectionModal').modal('hide');
                            fetch(`/ScheduledClass/ReserveScheduledClass?scheduledClassId=${user.scheduledClassId}&membershipId=${user.membershipId}`, {
                                method: 'POST',
                                headers: {
                                    'Content-Type': 'application/json',
                                    'X-CSRF-TOKEN': token
                                },

                            })

                                .then(response => response.json())
                                .then(result => {
                                    $('#feedbackModalTitle').text('Add Attendee');
                                    $('#feedbackModalBody p').text(result.message);
                                    $('#feedbackModal').modal('show');

                                    $('#feedbackModal').on('hidden.bs.modal', function () {
                                        window.location.reload();
                                    });
                                })
                                .catch(error => {
                                    $('#feedbackModalTitle').text('Error');
                                    $('#feedbackModalBody p').text('Failed to add reservation. Please try again.');
                                    $('#feedbackModal').modal('show');

                                    $('#feedbackModal').on('hidden.bs.modal', function () {
                                        window.location.reload();
                                    });
                                });
                        };
                        actionCell.appendChild(addButton);
                    });

                    initializeUserDetailsTableSearch();
                    $('#userSelectionModal').modal('show');
                }
            })

            .catch(error => {
                $('#feedbackModalTitle').text('Error');
                $('#feedbackModalBody p').text('Failed to retrieve unreserved members. Please try again.');
                $('#feedbackModal').modal('show');
            });
    });

    function initializeUserDetailsTableSearch() {
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