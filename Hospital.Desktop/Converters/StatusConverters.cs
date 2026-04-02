using Hospital.Core.Enums;
using Newtonsoft.Json;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Hospital.Desktop.Converters
{
    // 1. محول التاريخ (ضروري جداً لعمل الـ DatePicker مع الـ DateOnly)
    public class DateOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly d)
                return d.ToDateTime(TimeOnly.MinValue);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return DateOnly.FromDateTime(dt);

            return value;
        }
    }

    // 2. محول الجنس
    public class GenderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is enGender gender)
                return gender == enGender.Male ? "ذكر" : "أنثى";
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 3. محول نوع المناوبة
    public class ShiftTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is enShiftType shift)
                return shift == enShiftType.Morning ? "صباحي" : "مسائي";
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 4. محول التحصيل الدراسي
    public class CertificateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is enCertificate cert)
            {
                return cert switch
                {
                    enCertificate.HighSchool => "ثانوية عامة",
                    enCertificate.institute => "معهد",
                    enCertificate.Collage => "بكالوريوس",
                    enCertificate.Master => "ماجستير",
                    enCertificate.PHD => "دكتوراه",
                    enCertificate.Prof => "بروفيسور",
                    _ => cert.ToString()
                };
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 5. محول الحالة الوظيفية
    public class JobStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is enJobStatus status)
            {
                return status switch
                {
                    enJobStatus.Continuous => "مستمر",
                    enJobStatus.OnVacation => "في إجازة",
                    enJobStatus.DisContinue => "منقطع",
                    enJobStatus.Retired => "متقاعد",
                    enJobStatus.Departed => "غادر الخدمة",
                    _ => status.ToString()
                };
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 6. محول حالة الحساب (نشط/غير نشط)
    public class BooleanToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? "نشط" : "غير نشط";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 7. محول الحذف والاستعادة (نص ولون)
    public class DeleteRestoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isDeleted)
            {
                if (parameter?.ToString() == "Color")
                    return isDeleted ? Brushes.Green : Brushes.Red;

                return isDeleted ? "استعادة" : "حذف";
            }
            return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 8. محول القيم المنطقية للظهور (Boolean to Visibility)
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 9. محول القيم المنطقية العكسية
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : value;
    }

    // 10. محول القيم المنطقية العكسية للظهور
    public class InverseBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 11. محول Json لتواريخ DateOnly
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
            => writer.WriteValue(value.ToString("yyyy-MM-dd"));

        public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
            => DateOnly.Parse(reader.Value.ToString());
    }

    // --- المحولات الجديدة المضافة لشاشة الموظفين والجداول ---

    // 12. محول خيارات الصباحي (السبت/الخميس)
    public class MorningShiftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is enMorningShifts shift)
                return shift == enMorningShifts.SaturdayGroup ? "مجموعة السبت" : "مجموعة الخميس";
            return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 13. محول Enum للظهور (يستخدم لإخفاء/إظهار حقول بناءً على نوع المناوبة المختار)
    public class EnumToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            // مقارنة القيمة الحالية بالباراميتر المرسل من XAML
            return value.ToString() == parameter.ToString() ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 14. محول القيم الفارغة للظهور (يستخدم لإظهار رسائل التحذير)
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // إذا كانت القيمة فارغة، اجعل العنصر مرئياً (لإظهار التنبيه)
            return value == null ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    // 15. محول ألوان الحالة (لتلوين خلفية خلايا الـ DataGrid)
    public class StatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isActive)
            {
                // أخضر للحالة النشطة، أحمر للغير نشطة (ألوان عصرية)
                return isActive ? new SolidColorBrush(Color.FromRgb(34, 197, 94)) : new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}