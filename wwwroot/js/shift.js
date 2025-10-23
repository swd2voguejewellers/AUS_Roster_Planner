$(document).ready(function () {
    const $table = $('#rosterTable');
    const $thead = $table.find('thead tr');
    const $tbody = $table.find('tbody');

    // Role colors
    const colors = {
        manager: '#007bff',   // Blue
        permanent: '#198754', // Green
        casual: '#ffc107'     // Yellow
    };

    // Load staff list
    $.get('/api/staff', function (staffList) {
        // Sort order: Manager → Permanent → Casual
        staffList.sort((a, b) => {
            if (a.isManager && !b.isManager) return -1;
            if (!a.isManager && b.isManager) return 1;
            if (a.isPermanent && !b.isPermanent) return -1;
            if (!a.isPermanent && b.isPermanent) return 1;
            return 0;
        });

        // === Build table header ===
        staffList.forEach(s => {
            let role = s.isManager ? 'Manager' : (s.isPermanent ? 'Permanent' : 'Casual');
            let color = s.isManager ? colors.manager : (s.isPermanent ? colors.permanent : colors.casual);

            $thead.append(
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

        // === Get default time template from backend ===
        $.get('/api/roster/suggestion', function (response) {
            const { days, permanentLeave } = response;

            // Map day defaults for quick access
            const defaults = {};

            days.forEach(d => {
                const [from, to] = d.timeRange.split('-');
                defaults[d.day] = { from, to, hours: d.hours, needCasuals: d.needCasuals };
            });

            // === Build table body ===
            $tbody.find('tr').each(function () {
                const day = $(this).find('td:first').clone().children().remove().end().text().trim();
                console.log(day)

                staffList.forEach(s => {
                    const role = s.isManager ? 'Manager'
                        : (s.isPermanent ? 'Permanent' : 'Casual');
                    const color = s.isManager ? colors.manager
                        : (s.isPermanent ? colors.permanent : colors.casual);

                    // check if this person is on leave for the day
                    const leaveDays = permanentLeave[s.firstName] || permanentLeave[role] || [];
                    const isLeave = leaveDays.includes(day);

                    const $cell = $('<td>').addClass('p-2 align-middle');

                    if (isLeave) {
                        $cell.html('<span class="badge bg-danger w-100">Leave</span>');
                    } else {
                        const fromTime = defaults[day]?.from || '';
                        const toTime = defaults[day]?.to || '';
                        const totalHours = defaults[day]?.hours || 0;

                        // Disable casuals if not a 12-hour day
                        const disableCasual = !s.isPermanent && totalHours !== 12;
                        const disabledAttr = disableCasual ? 'disabled' : '';

                        // If disabled, clear times
                        const fromVal = disableCasual ? '' : fromTime;
                        const toVal = disableCasual ? '' : toTime;

                        $cell.append(`
                            <div class="input-group input-group-sm shadow-sm"
                                 style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                                <input type="time" class="form-control text-center from-time"
                                       value="${fromVal}" style="border: none; min-width: 95px;" ${disabledAttr}>
                                <span class="input-group-text bg-light border-start border-end">to</span>
                                <input type="time" class="form-control text-center to-time"
                                       value="${toVal}" style="border: none; min-width: 95px;" ${disabledAttr}>
                            </div>
                        `);
                    }

                    $cell.css('background-color', isLeave ? '#f8d7da' : color + '22');
                    $(this).append($cell);
                });
            });

            // Style Day column
            $thead.find('th:first-child, #rosterTable tbody td:first-child').css({
                backgroundColor: '#6c757d',
                color: 'white',
                fontWeight: 'bold',
                textAlign: 'center'
            });
        });
    });

    // ==========================
    // Save roster button handler
    // ==========================
    $('#rosterForm').on('submit', function (e) {
        e.preventDefault();

        const rosterData = [];

        $('#rosterTable tbody tr').each(function () {
            const day = $(this).find('td:first').text().trim();
            $(this).find('td:gt(0)').each(function (i) {
                const from = $(this).find('.from-time').val();
                const to = $(this).find('.to-time').val();
                const isLeave = $(this).find('.badge.bg-danger').length > 0;

                rosterData.push({
                    day,
                    staffIndex: i,
                    from,
                    to,
                    isLeave
                });
            });
        });

        console.log('Saving roster:', rosterData);

        $.ajax({
            url: '/api/roster/save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(rosterData),
            success: function () {
                alert('Roster saved successfully!');
            },
            error: function (xhr) {
                alert('Failed to save roster: ' + xhr.responseText);
            }
        });
    });

    // ==========================
    // Toggle Leave by double click
    // ==========================
    $(document).on('dblclick', '#rosterTable td:not(:first-child)', function () {
        const $badge = $(this).find('.badge.bg-danger');
        if ($badge.length) {
            // Remove leave, restore time pickers
            $badge.parent().empty().append(`
                <div class="input-group input-group-sm shadow-sm"
                     style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                    <input type="time" class="form-control text-center from-time" style="border: none; min-width: 95px;">
                    <span class="input-group-text bg-light border-start border-end">to</span>
                    <input type="time" class="form-control text-center to-time" style="border: none; min-width: 95px;">
                </div>
            `);
            $(this).css('background-color', '#fff');
        } else {
            // Mark leave
            $(this).empty().append('<span class="badge bg-danger w-100">Leave</span>');
            $(this).css('background-color', '#f8d7da');
        }
    });
});
