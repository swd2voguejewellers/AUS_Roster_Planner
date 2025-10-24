$(document).ready(function () {
    // ==========================
    // GLOBALS
    // ==========================
    const $table = $('#rosterTable');
    const $thead = $table.find('thead tr');
    const $tbody = $table.find('tbody');
    const $summaryBody = $('#summaryTable tbody');

    const COLORS = {
        Manager: '#007bff',
        Permanent: '#198754',
        Casual: '#ffc107'
    };

    let staffList = [];
    let rosterDefaults = {};

    // ==========================
    // INITIAL LOAD
    // ==========================
    $.get('/api/staff', function (staffs) {
        staffList = staffs.sort((a, b) => {
            if (a.isManager && !b.isManager) return -1;
            if (!a.isManager && b.isManager) return 1;
            if (a.isPermanent && !b.isPermanent) return -1;
            if (!a.isPermanent && b.isPermanent) return 1;
            return 0;
        });

        buildHeader();
        loadDefaultRoster();
    });

    // ==========================
    // BUILD HEADER
    // ==========================
    function buildHeader() {
        staffList.forEach(s => {
            const role = s.isManager ? 'Manager' : (s.isPermanent ? 'Permanent' : 'Casual');
            const color = COLORS[role];
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
    }

    // ==========================
    // LOAD DEFAULT ROSTER
    // ==========================
    function loadDefaultRoster() {
        $.get('/api/roster/suggestion', function (res) {
            const { days, permanentLeave } = res;
            rosterDefaults = {};

            days.forEach(d => {
                const [from, to] = d.timeRange.split('-');
                rosterDefaults[d.day] = { from, to, hours: d.hours, needCasuals: d.needCasuals };
            });

            $tbody.find('tr').each(function () {
                const day = $(this).find('td:first').clone().children().remove().end().text().trim();

                staffList.forEach(s => {
                    const role = s.isManager ? 'Manager' : (s.isPermanent ? 'Permanent' : 'Casual');
                    const color = COLORS[role];
                    const leaveDays = permanentLeave[s.firstName] || permanentLeave[role] || [];
                    const isLeave = leaveDays.includes(day);

                    const fromTime = rosterDefaults[day]?.from || '';
                    const toTime = rosterDefaults[day]?.to || '';
                    const totalHours = rosterDefaults[day]?.hours || 0;
                    const disableCasual = role === 'Casual' && totalHours !== 12;
                    const disabledAttr = disableCasual ? 'disabled' : '';
                    const fromVal = disableCasual ? '' : fromTime;
                    const toVal = disableCasual ? '' : toTime;

                    const $cell = $('<td>').addClass('p-2 align-middle').css('background-color', isLeave ? '#f8d7da' : color + '22');

                    if (isLeave) {
                        $cell.html('<span class="badge bg-danger w-100">Leave</span>');
                    } else {
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
                    $(this).append($cell);
                });
            });

            $thead.find('th:first-child, #rosterTable tbody td:first-child').css({
                backgroundColor: '#6c757d',
                color: 'white',
                fontWeight: 'bold',
                textAlign: 'center'
            });

            updateSummary();
        });
    }

    // ==========================
    // SAVE ROSTER
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
                rosterData.push({ day, staffIndex: i, from, to, isLeave });
            });
        });

        $.ajax({
            url: '/api/roster/save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(rosterData),
            success: () => alert('Roster saved successfully!'),
            error: xhr => alert('Failed to save roster: ' + xhr.responseText)
        });
    });

    // ==========================
    // TOGGLE LEAVE
    // ==========================
    $(document).on('dblclick', '#rosterTable td:not(:first-child)', function () {
        const $badge = $(this).find('.badge.bg-danger');
        const $row = $(this).closest('tr');
        const day = $row.find('td:first').clone().children().remove().end().text().trim();
        const colIndex = $(this).index() - 1;
        const headerCell = $('#rosterTable thead th').eq(colIndex + 1);
        const headerText = headerCell.text().trim();
        const role = headerText.includes('Manager')
            ? 'Manager'
            : (headerText.includes('Permanent') ? 'Permanent' : 'Casual');

        if ($badge.length) {
            const fromTime = rosterDefaults[day]?.from || '';
            const toTime = rosterDefaults[day]?.to || '';
            const totalHours = rosterDefaults[day]?.hours || 0;
            const disableCasual = role === 'Casual' && totalHours !== 12;
            const disabledAttr = disableCasual ? 'disabled' : '';
            const fromVal = disableCasual ? '' : fromTime;
            const toVal = disableCasual ? '' : toTime;

            $badge.parent().empty().append(`
                <div class="input-group input-group-sm shadow-sm"
                     style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                    <input type="time" class="form-control text-center from-time"
                           value="${fromVal}" style="border: none; min-width: 95px;" ${disabledAttr}>
                    <span class="input-group-text bg-light border-start border-end">to</span>
                    <input type="time" class="form-control text-center to-time"
                           value="${toVal}" style="border: none; min-width: 95px;" ${disabledAttr}>
                </div>
            `);

            $(this).css('background-color', COLORS[role] + '22');
        } else {
            $(this).empty().append('<span class="badge bg-danger w-100">Leave</span>');
            $(this).css('background-color', '#f8d7da');
        }

        setTimeout(updateSummary, 150);
    });

    // ==========================
    // UPDATE SUMMARY
    // ==========================
    $(document).on('change', '.from-time, .to-time', updateSummary);

    function updateSummary() {
        const summary = {};

        $('#rosterTable thead th:gt(0)').each(function (i) {
            const parts = $(this).text().trim().split(/(?=[A-Z])/);
            const name = parts[0].trim();
            const role = parts[1]?.trim() || '';
            summary[i] = { name, role, totalHours: 0, leaveDays: 0 };
        });

        $('#rosterTable tbody tr').each(function () {
            $(this).find('td:gt(0)').each(function (i) {
                const s = summary[i];
                if (!s) return;
                const isLeave = $(this).find('.badge.bg-danger').length > 0;

                if (isLeave) {
                    s.leaveDays++;
                } else {
                    const from = $(this).find('.from-time').val();
                    const to = $(this).find('.to-time').val();
                    if (from && to) {
                        const [fh, fm] = from.split(':').map(Number);
                        const [th, tm] = to.split(':').map(Number);
                        let hours = (th + tm / 60) - (fh + fm / 60);
                        if (hours < 0) hours += 24;
                        s.totalHours += hours;
                    }
                }
            });
        });

        $summaryBody.empty();
        Object.values(summary).forEach(s => {
            const ot = s.totalHours > 40 ? (s.totalHours - 40).toFixed(1) : 0;
            $summaryBody.append(`
                <tr>
                    <td>${s.name}</td>
                    <td>${s.role}</td>
                    <td>${s.totalHours.toFixed(1)}</td>
                    <td class="${ot > 0 ? 'text-danger fw-bold' : ''}">${ot}</td>
                    <td>${s.leaveDays}</td>
                </tr>
            `);
        });
    }
});
