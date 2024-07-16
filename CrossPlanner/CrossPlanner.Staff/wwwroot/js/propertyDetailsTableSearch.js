document.addEventListener('DOMContentLoaded', () => {
    const searchInput = document.getElementById('property-details-table-search');
    const tableRows = document.querySelectorAll('#property-details-table tr');
    const noResultsDiv = document.getElementById('no-results');

    const performSearch = () => {
        const searchValue = searchInput.value.toLowerCase().trim();
        let visibleRows = 0;

        tableRows.forEach(row => {
            const text = row.textContent.toLowerCase();
            const isVisible = searchValue.split(/\s+/).every(part => text.includes(part));
            row.style.display = isVisible ? '' : 'none';
            if (isVisible) visibleRows++;
        });

        noResultsDiv.style.display = visibleRows > 0 ? 'none' : 'block';
    };

    searchInput.addEventListener('input', performSearch);
    performSearch();
});