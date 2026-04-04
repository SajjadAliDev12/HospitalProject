using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Hospital.Desktop.Services
{
    public static class ReportGenerator
    {
        public static FlowDocument CreateShiftReport(
            string title,
            List<EmployeeReportDto> employees,
            Dictionary<int, List<int>> shiftDays)
        {
            FlowDocument doc = new FlowDocument();
            doc.FontFamily = new FontFamily("Segoe UI");
            doc.FlowDirection = FlowDirection.RightToLeft;

            // ضبط الصفحة على الوضع العرضي (Landscape) A4
            doc.PageWidth = 11.69 * 96;
            doc.PageHeight = 8.27 * 96;
            doc.PagePadding = new Thickness(30);
            doc.ColumnWidth = double.PositiveInfinity;

            var deptGroups = employees.GroupBy(e => e.DepartmentName ?? "قسم غير معرف");

            foreach (var dept in deptGroups)
            {
                Section deptSection = new Section();
                if (deptGroups.Count() > 1) deptSection.BreakPageBefore = true;

                // 1. عنوان التقرير والقسم
                Paragraph reportTitle = new Paragraph(new Run($"{title} - قسم: {dept.Key}"));
                reportTitle.FontSize = 18;
                reportTitle.FontWeight = FontWeights.Bold;
                reportTitle.TextAlignment = TextAlignment.Center;
                reportTitle.Margin = new Thickness(0, 0, 0, 15);
                deptSection.Blocks.Add(reportTitle);

                // --- أولاً: الدوام الصباحي ---
                var morningEmps = dept.Where(e => e.ShiftType == enShiftType.Morning).ToList();
                if (morningEmps.Any())
                {
                    var satEmps = morningEmps.Where(e => e.MorningShiftGroup == enMorningShifts.SaturdayGroup).ToList();
                    var thuEmps = morningEmps.Where(e => e.MorningShiftGroup == enMorningShifts.ThursdayGroup).ToList();

                    AddBalancedMorningTables(deptSection, satEmps, thuEmps);
                }

                // --- ثانياً: الخفارات المسائية ---
                var nightEmps = dept.Where(e => e.ShiftType == enShiftType.Night).ToList();

                // إضافة الشرط هنا: لا يتم بناء الجداول إذا كانت القائمة فارغة (مثلاً عند اختيار صباحي فقط)
                if (nightEmps.Any())
                {
                    AddBalancedNightTables(deptSection, nightEmps, shiftDays);
                }

                doc.Blocks.Add(deptSection);
            }

            return doc;
        }

        private static void AddBalancedMorningTables(Section section, List<EmployeeReportDto> satEmps, List<EmployeeReportDto> thuEmps)
        {
            Table containerTable = new Table { CellSpacing = 8, Margin = new Thickness(0, 0, 0, 10) };
            containerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            containerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup rowGroup = new TableRowGroup();
            TableRow mainRow = new TableRow();

            int maxCount = Math.Max(satEmps.Count, thuEmps.Count);

            mainRow.Cells.Add(CreateMorningCell("السبت", satEmps, maxCount,
                new SolidColorBrush(Color.FromRgb(37, 99, 235)),
                new SolidColorBrush(Color.FromRgb(239, 246, 255))));

            mainRow.Cells.Add(CreateMorningCell("الخميس", thuEmps, maxCount,
                new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                new SolidColorBrush(Color.FromRgb(254, 242, 242))));

            rowGroup.Rows.Add(mainRow);
            containerTable.RowGroups.Add(rowGroup);
            section.Blocks.Add(containerTable);
        }

        private static TableCell CreateMorningCell(string dayName, List<EmployeeReportDto> emps, int targetCount, Brush color, Brush bg)
        {
            TableCell cell = new TableCell();
            Table innerTable = new Table { CellSpacing = 0, BorderBrush = color, BorderThickness = new Thickness(1.5) };
            innerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup group = new TableRowGroup();
            TableRow titleRow = new TableRow { Background = color };
            titleRow.Cells.Add(new TableCell(new Paragraph(new Run(dayName)) { FontSize = 16, FontWeight = FontWeights.Bold, Foreground = Brushes.White, TextAlignment = TextAlignment.Center }) { Padding = new Thickness(4) });
            group.Rows.Add(titleRow);

            TableRow contentRow = new TableRow { Background = bg };
            Paragraph p = new Paragraph { TextAlignment = TextAlignment.Center, LineHeight = 18 };

            for (int i = 0; i < targetCount; i++)
            {
                if (i < emps.Count)
                    p.Inlines.Add(new Run($"{emps[i].JobTitle}/ {emps[i].Name}") { FontSize = 16, FontWeight = FontWeights.SemiBold });
                else
                    p.Inlines.Add(new Run(" "));

                p.Inlines.Add(new LineBreak());
            }

            contentRow.Cells.Add(new TableCell(p) { Padding = new Thickness(8) });
            group.Rows.Add(contentRow);
            innerTable.RowGroups.Add(group);
            cell.Blocks.Add(innerTable);
            return cell;
        }

        private static void AddBalancedNightTables(Section section, List<EmployeeReportDto> allNightEmps, Dictionary<int, List<int>> shiftDays)
        {
            int maxRowsNeeded = 0;
            for (int i = 1; i <= 4; i++)
            {
                int count = allNightEmps.Count(e => e.NightShiftId == i);
                int rows = (int)Math.Ceiling(count / 2.0);
                if (rows > maxRowsNeeded) maxRowsNeeded = rows;
            }
            if (maxRowsNeeded < 2) maxRowsNeeded = 2;

            Table nightContainer = new Table { CellSpacing = 8 };
            nightContainer.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            nightContainer.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup group = new TableRowGroup();
            TableRow row1 = new TableRow();
            row1.Cells.Add(CreateNightTeamCell(1, allNightEmps.Where(e => e.NightShiftId == 1).ToList(), maxRowsNeeded, shiftDays));
            row1.Cells.Add(CreateNightTeamCell(2, allNightEmps.Where(e => e.NightShiftId == 2).ToList(), maxRowsNeeded, shiftDays));
            group.Rows.Add(row1);

            TableRow row2 = new TableRow();
            row2.Cells.Add(CreateNightTeamCell(3, allNightEmps.Where(e => e.NightShiftId == 3).ToList(), maxRowsNeeded, shiftDays));
            row2.Cells.Add(CreateNightTeamCell(4, allNightEmps.Where(e => e.NightShiftId == 4).ToList(), maxRowsNeeded, shiftDays));
            group.Rows.Add(row2);

            nightContainer.RowGroups.Add(group);
            section.Blocks.Add(nightContainer);
        }

        private static TableCell CreateNightTeamCell(int teamId, List<EmployeeReportDto> emps, int targetRows, Dictionary<int, List<int>> shiftDays)
        {
            Brush color = GetTeamColor(teamId);
            Brush bg = GetTeamBg(teamId);
            List<int> days = shiftDays.ContainsKey(teamId) ? shiftDays[teamId] : new List<int>();

            TableCell cell = new TableCell();
            Table teamTable = new Table { CellSpacing = 0, BorderBrush = color, BorderThickness = new Thickness(1.5) };
            teamTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup group = new TableRowGroup();

            TableRow contentRow = new TableRow { Background = bg };
            TableCell namesCell = new TableCell();

            Table namesGrid = new Table { CellSpacing = 0 };
            namesGrid.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            namesGrid.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            TableRowGroup namesGroup = new TableRowGroup();

            for (int r = 0; r < targetRows; r++)
            {
                TableRow nameRow = new TableRow();
                for (int c = 0; c < 2; c++)
                {
                    int index = r * 2 + c;
                    string text = " ";
                    FontWeight weight = FontWeights.Normal;
                    if (index < emps.Count)
                    {
                        string label = (index == 0) ? "(المسؤول) " : "";
                        text = $"{emps[index].JobTitle}/ {emps[index].Name} {label}";
                        weight = FontWeights.SemiBold;
                    }
                    nameRow.Cells.Add(new TableCell(new Paragraph(new Run(text)) { FontSize = 16, FontWeight = weight, TextAlignment = TextAlignment.Center }) { Padding = new Thickness(2) });
                }
                namesGroup.Rows.Add(nameRow);
            }
            namesGrid.RowGroups.Add(namesGroup);
            namesCell.Blocks.Add(namesGrid);
            contentRow.Cells.Add(namesCell);
            group.Rows.Add(contentRow);

            TableRow datesRow = new TableRow();
            TableCell compositeCell = new TableCell();
            Table datesSubTable = new Table { CellSpacing = 0 };
            int dayCols = Math.Max(days.Count, 1);
            for (int d = 0; d < dayCols; d++) datesSubTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup datesGroup = new TableRowGroup();
            TableRow daysDataRow = new TableRow();
            if (days.Any())
            {
                foreach (int day in days)
                    daysDataRow.Cells.Add(new TableCell(new Paragraph(new Run(day.ToString())) { FontSize = 14, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center }) { BorderBrush = color, BorderThickness = new Thickness(0.5, 1, 0.5, 0), Padding = new Thickness(2) });
            }
            else
            {
                daysDataRow.Cells.Add(new TableCell(new Paragraph(new Run("-"))) { BorderBrush = color, BorderThickness = new Thickness(0, 1, 0, 0) });
            }
            datesGroup.Rows.Add(daysDataRow);
            datesSubTable.RowGroups.Add(datesGroup);
            compositeCell.Blocks.Add(datesSubTable);
            datesRow.Cells.Add(compositeCell);
            group.Rows.Add(datesRow);

            teamTable.RowGroups.Add(group);
            cell.Blocks.Add(teamTable);
            return cell;
        }

        private static Brush GetTeamColor(int id)
        {
            switch (id)
            {
                case 1: return new SolidColorBrush(Color.FromRgb(150, 50, 50));
                case 2: return new SolidColorBrush(Color.FromRgb(50, 100, 50));
                case 3: return new SolidColorBrush(Color.FromRgb(30, 80, 140));
                case 4: return new SolidColorBrush(Color.FromRgb(160, 100, 40));
                default: return Brushes.Black;
            }
        }

        private static Brush GetTeamBg(int id)
        {
            switch (id)
            {
                case 1: return new SolidColorBrush(Color.FromRgb(255, 245, 245));
                case 2: return new SolidColorBrush(Color.FromRgb(245, 255, 245));
                case 3: return new SolidColorBrush(Color.FromRgb(245, 250, 255));
                case 4: return new SolidColorBrush(Color.FromRgb(255, 252, 245));
                default: return Brushes.White;
            }
        }
    }
}