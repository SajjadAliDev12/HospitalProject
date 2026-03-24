using System.Security.Cryptography;
using System.Text;

public static class EncryptionHelper
{
    // "مُلح" إضافي لزيادة قوة التشفير (اختياري)
    private static readonly byte[] OptionalEntropy = { 1, 2, 3, 4, 5 };

    public static string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        // التشفير بنطاق "المستخدم الحالي" فقط
        byte[] encryptedBytes = ProtectedData.Protect(plainBytes, OptionalEntropy, DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(encryptedBytes);
    }

    public static string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return string.Empty;

        try
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, OptionalEntropy, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            
            return string.Empty;
        }
    }
}