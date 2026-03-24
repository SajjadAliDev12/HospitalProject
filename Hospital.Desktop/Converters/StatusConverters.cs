using Hospital.Core.Enums;
using Newtonsoft.Json;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hospital.Desktop.Converters
{
    // محول التاريخ (ضروري جداً لعمل الـ DatePicker مع الـ DateOnly)
    public class DateOnlyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateOnly d)
                return d.ToDateTime(TimeOnly.MinValue);

            return null; // إرجاع فارغ بدلاً من خطأ إذا كانت القيمة افتراضية
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
                return DateOnly.FromDateTime(dt);

            return value;
        }
    }

    // محول الجنس
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

    // محول نوع المناوبة
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

    // محول التحصيل الدراسي
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

    // محول الحالة الوظيفية
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

    // --- المحولات السابقة (تم استبدال Exception بـ DoNothing لضمان عدم توقف البرنامج) ---

    public class BooleanToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? "نشط" : "غير نشط";
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : value;
    }

    public class InverseBoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => (bool)value ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer)
            => writer.WriteValue(value.ToString("yyyy-MM-dd"));

        public override DateOnly ReadJson(JsonReader reader, Type objectType, DateOnly existingValue, bool hasExistingValue, JsonSerializer serializer)
            => DateOnly.Parse(reader.Value.ToString());
    }
}