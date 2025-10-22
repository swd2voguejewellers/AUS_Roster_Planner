$(document).ready(function () {
    $.get('api/staff', function (staffList) {
        // Sort order: Manager → Permanent → Casual
        staffList.sort((a, b) => {
            if (a.isManager && !b.isManager) return -1;
            if (!a.isManager && b.isManager) return 1;
            if (a.isPermanent && !b.isPermanent) return -1;
            if (!a.isPermanent && b.isPermanent) return 1;
            return 0;
        });

        // Role colors
        const colors = {
            manager: '#007bff',
            permanent: '#198754',
            casual: '#ffc107'
        };

        // === Build table header ===
        staffList.forEach(s => {
            let role = s.isManager ? 'Manager' : (s.isPermanent ? 'Permanent' : 'Casual');
            let color = s.isManager ? colors.manager : (s.isPermanent ? colors.permanent : colors.casual);

            $('#rosterTable thead tr').append(
                $('<th>')
                    .css({
                        backgroundColor: color,
                        color: 'white',
                        fontWeight: 'bold',
                        textAlign: 'center'
                    })
                    .html(`${s.firstName}<br><small>${role}</small>`)
            );
        });

        // === Build table body ===
        $('#rosterTable tbody tr').each(function () {
            staffList.forEach(s => {
                $(this).append(`
                    <td class="p-2 roster-cell" data-leave="false">
                        <div class="input-group input-group-sm shadow-sm work-hours"
                             style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                            <input type="time" class="form-control text-center fw-semibold" placeholder="From" style="border: none; min-width: 95px;">
                            <span class="input-group-text bg-light border-start border-end">to</span>
                            <input type="time" class="form-control text-center fw-semibold" placeholder="To" style="border: none; min-width: 95px;">
                        </div>
                    </td>
                `);
            });
        });

        // === Style Day header column ===
        $('#rosterTable thead th:first-child, #rosterTable tbody td:first-child').css({
            backgroundColor: '#6c757d',
            color: 'white',
            fontWeight: 'bold',
            textAlign: 'center'
        });

        // === Toggle leave state ===
        $(document).on('dblclick', '.roster-cell', function () {
            const isLeave = $(this).attr('data-leave') === 'true';
            if (isLeave) {
                // Restore input fields
                $(this)
                    .attr('data-leave', 'false')
                    .removeClass('leave-cell')
                    .html(`
                        <div class="input-group input-group-sm shadow-sm work-hours"
                             style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                            <input type="time" class="form-control text-center fw-semibold" placeholder="From" style="border: none; min-width: 95px;">
                            <span class="input-group-text bg-light border-start border-end">to</span>
                            <input type="time" class="form-control text-center fw-semibold" placeholder="To" style="border: none; min-width: 95px;">
                        </div>
                    `);
            } else {
                // Mark as Leave
                $(this)
                    .attr('data-leave', 'true')
                    .addClass('leave-cell')
                    .html(`<div class="text-danger fw-bold p-2">On Leave</div>`);
            }
        });
    });
});
