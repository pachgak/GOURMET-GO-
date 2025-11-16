using System.Globalization;

public static class StringExtensions
{
    /// <summary>
    /// แปลง string เป็น bool โดยยอมรับ "TRUE", "true", "1" และเทียบเคียง
    /// </summary>
    public static bool ToBool(this string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return false;
        }

        string cleanStr = str.Trim().ToUpperInvariant();

        // ตรวจสอบค่าที่เป็น True
        if (cleanStr == "TRUE" || cleanStr == "T" || cleanStr == "YES" || cleanStr == "1")
        {
            return true;
        }

        // ลองใช้ bool.TryParse เป็นทางเลือกสุดท้าย
        if (bool.TryParse(str, out bool result))
        {
            return result;
        }

        return false;
    }
}