using ClosedXML.Excel;
using ShiftPlanner.Models;
using System.Globalization;

public static class ExcelExportHelper
{
    public static byte[] ExportRosterToExcel(Roster roster)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Roster");

        var days = roster.Entries
            .Select(e => e.RosterDate.Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var staffList = roster.Entries
            .Select(e => $"ID:{e.StaffId}")
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        // Header
        worksheet.Cell(1, 1).Value = "Staff";
        worksheet.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < days.Count; i++)
        {
            var day = days[i];
            worksheet.Cell(1, i + 2).Value = $"{day:yyyy-MM-dd} ({day:dddd})";
            worksheet.Cell(1, i + 2).Style.Font.Bold = true;
        }

        // Data rows
        for (int row = 0; row < staffList.Count; row++)
        {
            worksheet.Cell(row + 2, 1).Value = staffList[row];

            for (int col = 0; col < days.Count; col++)
            {
                var day = days[col];
                var cell = worksheet.Cell(row + 2, col + 2);

                var entry = roster.Entries.FirstOrDefault(e =>
                    $"ID:{e.StaffId}" == staffList[row] &&
                    e.RosterDate.Date == day);

                // Defaults
                cell.Value = "N/A";
                cell.Style.Fill.BackgroundColor = XLColor.LightYellow;

                if (entry != null)
                {
                    if (entry.IsLeave)
                    {
                        cell.Value = string.IsNullOrEmpty(entry.LeaveType)
                            ? "Leave"
                            : entry.LeaveType;

                        cell.Style.Fill.BackgroundColor = XLColor.LightCoral; // Red
                        cell.Style.Font.FontColor = XLColor.White;
                    }
                    else if (entry.FromTime.HasValue && entry.ToTime.HasValue)
                    {
                        cell.Value = $"{entry.FromTime:hh\\:mm}-{entry.ToTime:hh\\:mm}";

                        cell.Style.Fill.BackgroundColor = XLColor.LightGreen; // Green
                        cell.Style.Font.FontColor = XLColor.Black;
                    }
                }

                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
        }

        worksheet.Columns().AdjustToContents();
        worksheet.Rows().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
