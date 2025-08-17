using System.Globalization;
using System.Text;

namespace Shared.Utilities
{
    public static class TextNormalization
    {
        public static string RemoveDiacritics(this string? text)
        {
            if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(capacity: normalized.Length);
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark && uc != UnicodeCategory.SpacingCombiningMark)
                    sb.Append(ch);
            }
            // Replace any remaining special Romanian letters that might not decompose on some collations
            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace('?', 't').Replace('?', 'T')
                     .Replace('?', 's').Replace('?', 'S')
                     .Replace('?', 'a').Replace('?', 'A')
                     .Replace('�', 'a').Replace('�', 'A')
                     .Replace('�', 'i').Replace('�', 'I');
        }
    }
}
