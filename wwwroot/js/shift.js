$(document).ready(function () {
    // ==========================
    // WEEK SELECTOR
    // ==========================
    const $weekSelect = $('#weekSelect');
    initWeekSelector();

    function initWeekSelector() {
        const today = new Date();
        const currentSunday = new Date(today);
        currentSunday.setDate(today.getDate() - ((today.getDay() + 6) % 7)); // Monday-based

        $weekSelect.empty();

        // Generate 5 weeks: past 2, current, next 2
        for (let offset = -2; offset <= 2; offset++) {
            const sunday = new Date(currentSunday);
            sunday.setDate(sunday.getDate() + offset * 7);
            const saturday = new Date(sunday);
            saturday.setDate(sunday.getDate() + 6);

            const label = `${sunday.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })} - ${saturday.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })}`;
            const value = sunday.toISOString().split('T')[0];

            $weekSelect.append(`<option value="${value}">${label}</option>`);
        }

        // Default to current week (Sunday-based)
        $weekSelect.val(currentSunday.toISOString().split('T')[0]);
    }

    // ==========================
    // REBUILD TABLE BODY (Days)
    // ==========================
    function rebuildDaysForWeek(weekStartDate) {
        const start = new Date(weekStartDate);
        const $tbody = $('#rosterTable tbody');
        $tbody.empty(); // clear existing rows

        for (let i = 0; i < 7; i++) {
            const date = new Date(start);
            date.setDate(start.getDate() + i);

            const isWeekend = (date.getDay() === 0 || date.getDay() === 6); // Sunday or Saturday
            const dayName = date.toLocaleDateString('en-US', { weekday: 'long' });
            const shortDate = date.toLocaleDateString('en-US', { month: 'short', day: '2-digit' });

            const row = `
            <tr class="${isWeekend ? 'table-secondary' : ''}">
                <td class="fw-bold bg-light">
                    ${dayName}<br />
                    <small class="text-muted">${shortDate}</small>
                </td>
            </tr>
        `;
            $tbody.append(row);
        }
    }


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

        const defaultWeek = $weekSelect.val();
        rebuildDaysForWeek(defaultWeek);
        loadDefaultRoster(defaultWeek);
    });

    $weekSelect.on('change', function () {
        const selectedWeek = $(this).val();
        rebuildDaysForWeek(selectedWeek);  // update tbody days
        loadDefaultRoster(selectedWeek);   // reload roster data
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
    function loadDefaultRoster(selectedWeekStart = null) {
        const weekStart = selectedWeekStart || $('#weekSelect').val();
        const $tbody = $('#rosterTable tbody');
        const $thead = $('#rosterTable thead');

        $.get(`/api/roster/load?weekStart=${weekStart}`, function (res) {
            $tbody.find('tr').each(function () {
                $(this).find('td:gt(0)').remove(); // clear all except first column
            });

            if (res.type === 'saved') {
                // 🟢 Load saved roster
                const entriesByDay = {};

                // Group entries by day
                res.entries.forEach(e => {
                    if (!entriesByDay[e.day]) entriesByDay[e.day] = [];
                    entriesByDay[e.day].push(e);
                });

                $tbody.find('tr').each(function () {
                    const day = $(this).find('td:first').clone().children().remove().end().text().trim();
                    const entries = entriesByDay[day] || [];

                    staffList.forEach(s => {
                        const entry = entries.find(e => e.staffId == s.employeeID);
                        const role = s.isManager ? 'Manager' : (s.isPermanent ? 'Permanent' : 'Casual');
                        const color = COLORS[role];
                        const isLeave = entry?.isLeave || false;

                        const $cell = $('<td>').addClass('p-2 align-middle')
                            .css('background-color', isLeave ? '#f8d7da' : color + '22');

                        if (isLeave) {
                            $cell.html(`
                                        <input type="text" class="form-control text-center leave-text"
                                               value="${entry?.leaveType || 'RDO'}"
                                               style="background-color:#dc3545; color:white; border:none; font-weight:bold;">
                                    `);
                        } else {
                            $cell.append(`
                            <div class="input-group input-group-sm shadow-sm"
                                 style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                                <input type="time" class="form-control text-center from-time"
                                       value="${entry?.from || ''}" style="border: none; min-width: 95px;">
                                <span class="input-group-text bg-light border-start border-end">to</span>
                                <input type="time" class="form-control text-center to-time"
                                       value="${entry?.to || ''}" style="border: none; min-width: 95px;">
                            </div>
                        `);
                        }

                        $(this).append($cell);
                    });
                });
            } else {
                // 🟠 Fallback to suggested default
                console.log('Loaded suggested roster');
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
                        const leaveDays = permanentLeave[s.firstName] || [];
                        const isLeave = leaveDays.includes(day);

                        const fromTime = rosterDefaults[day]?.from || '';
                        const toTime = rosterDefaults[day]?.to || '';
                        const totalHours = rosterDefaults[day]?.hours || 0;
                        const disableCasual = role === 'Casual';
                        const fromVal = disableCasual ? '' : fromTime;
                        const toVal = disableCasual ? '' : toTime;

                        const $cell = $('<td>').addClass('p-2 align-middle')
                            .css('background-color', isLeave ? '#f8d7da' : color + '22');

                        if (isLeave) {
                            $cell.html(`
                                        <input type="text"
                                               class="form-control form-control-sm leave-text text-center"
                                               placeholder="Leave"
                                               value="RDO" style="background-color:#dc3545; color:white; border:none; font-weight:bold;">
                                    `);

                        } else {
                            $cell.append(`
                            <div class="input-group input-group-sm shadow-sm"
                                 style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                                <input type="time" class="form-control text-center from-time"
                                       value="${fromVal}" style="border: none; min-width: 95px;">
                                <span class="input-group-text bg-light border-start border-end">to</span>
                                <input type="time" class="form-control text-center to-time"
                                       value="${toVal}" style="border: none; min-width: 95px;">
                            </div>
                        `);
                        }
                        $(this).append($cell);
                    });
                });
            }

            // Common header styling
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

        const weekStart = $('#weekSelect').val(); // use selected week
        const createdBy = 'Manager'; // optional, set from your session or user context

        const entries = [];
        $('#rosterTable tbody tr').each(function () {
            const dayName = $(this).find('td:first').clone().children().remove().end().text().trim();

            $(this).find('td:gt(0)').each(function (i) {
                const from = $(this).find('.from-time').val();
                const to = $(this).find('.to-time').val();
                const leaveText = $(this).find('.leave-text').val()?.trim() || null;
                const isLeave = !!leaveText;


                const staff = staffList[i]; // assumes staffList is in the same order as table columns
                if (!staff) return;

                entries.push({
                    rosterId: 0,      // will be set on backend
                    staffId: staff.employeeID,
                    dayName,
                    fromTime: from || null,
                    toTime: to || null,
                    isLeave,
                    isDeleted: false,
                    leaveType: leaveText
                });
            });
        });

        const roster = {
            rosterId: 0,
            weekStart,
            createdBy,
            isDeleted: false,
            entries
        };

        $.ajax({
            url: '/api/roster/save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(roster),
            success: res => {
                alert("✅ " + res);
            },
            error: xhr => {
                alert("⚠️ Validation failed:\n" + xhr.responseText);
            }

        });
    });


    // ==========================
    // TOGGLE LEAVE
    // ==========================
    $(document).on('dblclick', '#rosterTable td:not(:first-child)', function () {
        const $cell = $(this);
        const $row = $cell.closest('tr');
        const colIndex = $cell.index() - 1;
        const staff = staffList[colIndex];
        const role = staff.isManager ? 'Manager' : (staff.isPermanent ? 'Permanent' : 'Casual');

        if (role === 'Casual') return; // optional: casuals cannot toggle leave

        const isLeaveCell = $cell.find('input.leave-text').length > 0 || $cell.css('background-color') === 'rgb(220, 53, 69)'; // red

        if (isLeaveCell) {
            // Toggle to normal from-to inputs
            const day = $row.find('td:first').clone().children().remove().end().text().trim();
            const fromTime = rosterDefaults[day]?.from || '';
            const toTime = rosterDefaults[day]?.to || '';

            $cell.html(`
            <div class="input-group input-group-sm shadow-sm"
                 style="border: 1px solid #ddd; border-radius: 8px; overflow: hidden;">
                <input type="time" class="form-control text-center from-time"
                       value="${fromTime}" style="border: none; min-width: 95px;">
                <span class="input-group-text bg-light border-start border-end">to</span>
                <input type="time" class="form-control text-center to-time"
                       value="${toTime}" style="border: none; min-width: 95px;">
            </div>
        `);

            // Set cell background according to role
            let bgColor = COLORS[role] + '22'; // slightly transparent
            $cell.css({ 'background-color': bgColor, color: 'black' });

        } else {
            // Toggle to editable leave input
            const currentVal = 'RDO';

            $cell.css({ 'background-color': '#f8d7da'});

            $cell.html(`
            <input type="text" class="form-control text-center leave-text"
                   value="${currentVal}"
                   style="background-color:#dc3545; color:white; border:none; font-weight:bold;">
        `);

            const $input = $cell.find('input.leave-text');
            $input.focus().select();
        }

        setTimeout(updateSummary, 150);
    });




    // ✅ Clear time inputs when pressing Delete / Backspace / Enter
    $(document).on('keyup', '.from-time, .to-time', function (e) {
        if (e.key === 'Delete') {
            $(this).val('').trigger('change');
        }
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
                const isLeave = $(this).find('.leave-text').length > 0;

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

    $(document).on('input', '.leave-text', function () {
        const hasText = $(this).val().trim().length > 0;
        const $cell = $(this).closest('td');

        if (hasText) {
            $cell.find('.from-time, .to-time').val('');
            $cell.find('.input-group').hide();
            $cell.css('background-color', '#f8d7da');
        } else {
            $cell.find('.input-group').show();
        }

        updateSummary();
    });

    $('#exportExcelLink').on('click', function (e) {
        e.preventDefault();

        // Example: get weekStart from a hidden field or JS variable
        // Replace with your actual logic to get the weekStart value
        var weekStart = $('#weekSelect').val() || new Date().toISOString().substring(0, 10);

        $.ajax({
            url: '/api/roster/excel?weekStart=' + encodeURIComponent(weekStart),
            type: 'POST',
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data, status, xhr) {
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }
                var blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = filename || "Roster.xlsx";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            },
            error: function (err) {
                if (err.status === 404) {
                    alert('Please save the Roster for the selected week.');
                }
                else {
                    alert('Failed to export roster. Please try again.');
                }
            }
        });
    });

    $("#logoutLink").click(function (e) {
        e.preventDefault();
        $.post("/Login/Logout")
            .done(function () {
                window.location.href = "/Login";
            })
            .fail(function () {
                alert("Logout failed, please try again.");
            });
    });

    // Auto logout on tab/window close
    $(window).on("beforeunload", function () {
        // Send a synchronous request before closing
        $.ajax({
            type: "POST",
            url: "/Login/Logout",
            async: false
        });
    });

    $(document).on("click", ".btn-download-roster", function () {

        const weekStart = $(this).data("week");

        if (!weekStart) return;

        $.ajax({
            url: '/api/roster/excel?weekStart=' + encodeURIComponent(weekStart),
            type: 'POST',
            xhrFields: {
                responseType: 'blob'
            },
            success: function (data, status, xhr) {
                var filename = "";
                var disposition = xhr.getResponseHeader('Content-Disposition');
                if (disposition && disposition.indexOf('attachment') !== -1) {
                    var matches = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
                    if (matches != null && matches[1]) filename = matches[1].replace(/['"]/g, '');
                }
                var blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                var link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = filename || "Roster.xlsx";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            },
            error: function () {
                alert('Failed to export roster. Please try again.');
            }
        });
    });

});
