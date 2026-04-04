using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using System.Linq;
using System.Collections.Generic;

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
            doc.PagePadding = new Thickness(50);

            // 1. الترويسة الرسمية (وزارة الصحة)
            Table headerTable = new Table() { CellSpacing = 0, Margin = new Thickness(0, 0, 0, 10) };
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });
            headerTable.Columns.Add(new TableColumn() { Width = new GridLength(2, GridUnitType.Star) });

            TableRowGroup headerGroup = new TableRowGroup();
            TableRow headerRow = new TableRow();

            TableCell rightCell = new TableCell();
            rightCell.Blocks.Add(new Paragraph(new Run("وزارة الصحة")) { FontSize = 12, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Left });
            rightCell.Blocks.Add(new Paragraph(new Run("دائرة صحة ذي قار")) { FontSize = 12, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Left });
            rightCell.Blocks.Add(new Paragraph(new Run("مستشفى الحبوبي التعليمي")) { FontSize = 12, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Left });
            rightCell.Blocks.Add(new Paragraph(new Run("العدد/")) { FontSize = 11, TextAlignment = TextAlignment.Left });
            rightCell.Blocks.Add(new Paragraph(new Run("التاريخ/")) { FontSize = 11, TextAlignment = TextAlignment.Left });

            TableCell leftCell = new TableCell();
            leftCell.Blocks.Add(new Paragraph(new Run("[Logo]")) { FontSize = 10, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Right });

            headerRow.Cells.Add(leftCell);
            headerRow.Cells.Add(rightCell);
            headerGroup.Rows.Add(headerRow);
            headerTable.RowGroups.Add(headerGroup);
            doc.Blocks.Add(headerTable);

            // 2. العنوان الرئيسي
            Paragraph mainTitle = new Paragraph(new Run(title));
            mainTitle.FontSize = 16;
            mainTitle.FontWeight = FontWeights.Bold;
            mainTitle.TextAlignment = TextAlignment.Center;
            mainTitle.Margin = new Thickness(0, 10, 0, 20);
            doc.Blocks.Add(mainTitle);

            // تجميع الموظفين حسب القسم
            var deptGroups = employees.GroupBy(e => e.DepartmentName);

            foreach (var dept in deptGroups)
            {
                // عنوان القسم بارز جداً للفصل بين الأقسام
                Paragraph deptHeader = new Paragraph(new Run($"قسم: {dept.Key}"));
                deptHeader.FontSize = 18;
                deptHeader.FontWeight = FontWeights.ExtraBold;
                deptHeader.TextAlignment = TextAlignment.Center;
                deptHeader.Background = new SolidColorBrush(Color.FromRgb(235, 235, 235));
                deptHeader.Padding = new Thickness(8);
                deptHeader.Margin = new Thickness(0, 20, 0, 15);
                doc.Blocks.Add(deptHeader);

                Section deptSection = new Section();

                // --- أولاً: الدوام الصباحي (فصل يدوي قاطع للمجموعات) ---
                var morningEmps = dept.Where(e => e.ShiftType == enShiftType.Morning).ToList();
                if (morningEmps.Any())
                {
                    // 1. مجموعة السبت فقط
                    var satEmps = morningEmps.Where(e => e.MorningShiftGroup == enMorningShifts.SaturdayGroup).ToList();
                    if (satEmps.Any())
                    {
                        AddMorningTable(deptSection, "السبت", satEmps,
                            new SolidColorBrush(Color.FromRgb(37, 99, 235)),
                            new SolidColorBrush(Color.FromRgb(239, 246, 255)));
                    }

                    // 2. مجموعة الخميس فقط
                    var thuEmps = morningEmps.Where(e => e.MorningShiftGroup == enMorningShifts.ThursdayGroup).ToList();
                    if (thuEmps.Any())
                    {
                        AddMorningTable(deptSection, "الخميس", thuEmps,
                            new SolidColorBrush(Color.FromRgb(220, 38, 38)),
                            new SolidColorBrush(Color.FromRgb(254, 242, 242)));
                    }
                }

                // --- ثانياً: الخفارات المسائية (على كبر الورقة) ---
                var nightEmps = dept.Where(e => e.ShiftType == enShiftType.Night).ToList();
                if (nightEmps.Any())
                {
                    for (int i = 1; i <= 4; i++)
                    {
                        var teamEmps = nightEmps.Where(e => e.NightShiftId == i).ToList();
                        if (teamEmps.Any())
                        {
                            List<int> days = shiftDays.ContainsKey(i) ? shiftDays[i] : new List<int>();
                            Brush teamColor = GetTeamColor(i);
                            Brush teamBg = GetTeamBg(i);

                            AddNightTable(deptSection, i, teamEmps, days, teamColor, teamBg);
                        }
                    }
                }

                doc.Blocks.Add(deptSection);
                doc.Blocks.Add(new Paragraph(new Run("------------------------------------------------------------------------------------------------"))
                { TextAlignment = TextAlignment.Center, Foreground = Brushes.LightGray, Margin = new Thickness(0, 15, 0, 15) });
            }

            return doc;
        }

        private static void AddMorningTable(Section section, string dayName, IEnumerable<EmployeeReportDto> emps, Brush color, Brush bg)
        {
            Table table = new Table() { CellSpacing = 0, BorderBrush = color, BorderThickness = new Thickness(2), Margin = new Thickness(0, 0, 0, 25) };
            table.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup group = new TableRowGroup();

            TableRow titleRow = new TableRow() { Background = color };
            titleRow.Cells.Add(new TableCell(new Paragraph(new Run(dayName)) { FontSize = 20, FontWeight = FontWeights.Bold, Foreground = Brushes.White, TextAlignment = TextAlignment.Center }) { Padding = new Thickness(8) });
            group.Rows.Add(titleRow);

            TableRow namesRow = new TableRow() { Background = bg };
            Paragraph p = new Paragraph() { TextAlignment = TextAlignment.Center, LineHeight = 24 };
            foreach (var emp in emps)
            {
                p.Inlines.Add(new Run($"{emp.JobTitle}/ {emp.Name}") { FontSize = 14, FontWeight = FontWeights.Bold });
                p.Inlines.Add(new LineBreak());
            }
            namesRow.Cells.Add(new TableCell(p) { Padding = new Thickness(20) });
            group.Rows.Add(namesRow);

            table.RowGroups.Add(group);
            section.Blocks.Add(table);
        }

        private static void AddNightTable(Section section, int teamId, List<EmployeeReportDto> emps, List<int> days, Brush color, Brush bg)
        {
            Table mainTable = new Table() { CellSpacing = 0, BorderBrush = color, BorderThickness = new Thickness(2), Margin = new Thickness(0, 0, 0, 15) };
            mainTable.Columns.Add(new TableColumn() { Width = new GridLength(1, GridUnitType.Star) });

            TableRowGroup group = new TableRowGroup();

            TableRow contentRow = new TableRow() { Background = bg };
            Paragraph p = new Paragraph() { TextAlignment = TextAlignment.Center, LineHeight = 22 };
            bool first = true;
            foreach (var emp in emps)
            {
                string label = first ? "(المسؤول) " : "";
                p.Inlines.Add(new Run($"{emp.JobTitle}/ {emp.Name} {label}") { FontSize = 13, FontWeight = FontWeights.Bold });
                p.Inlines.Add(new LineBreak());
                first = false;
            }
            contentRow.Cells.Add(new TableCell(p) { Padding = new Thickness(15) });
            group.Rows.Add(contentRow);

            TableRow datesRow = new TableRow();
            TableCell compositeCell = new TableCell();

            Table datesSubTable = new Table() { CellSpacing = 0 };
            foreach (var day in days) datesSubTable.Columns.Add(new TableColumn());

            TableRowGroup datesGroup = new TableRowGroup();
            TableRow daysDataRow = new TableRow();
            foreach (int day in days)
            {
                daysDataRow.Cells.Add(new TableCell(new Paragraph(new Run(day.ToString())) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center })
                { BorderBrush = color, BorderThickness = new Thickness(0.5, 1, 0.5, 0), Padding = new Thickness(6) });
            }
            datesGroup.Rows.Add(daysDataRow);
            datesSubTable.RowGroups.Add(datesGroup);

            compositeCell.Blocks.Add(datesSubTable);
            datesRow.Cells.Add(compositeCell);
            group.Rows.Add(datesRow);

            mainTable.RowGroups.Add(group);
            section.Blocks.Add(mainTable);
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