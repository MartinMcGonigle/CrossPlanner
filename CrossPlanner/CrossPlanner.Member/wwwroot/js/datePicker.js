document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('goToDate').addEventListener('click', function () {
        const selectedDateString = document.getElementById('datepicker').value;

        if (selectedDateString) {
            const selectedDate = new Date(selectedDateString);

            const formattedDate = formatDateToYMD(selectedDate);
            const currentPath = window.location.pathname.split('/');
            const currentController = currentPath[1];
            window.location.href = `/${currentController}?date=${formattedDate}`;
        } else {
            alert('Please select a date.');
        }
    });
});

function formatDateToYMD(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}