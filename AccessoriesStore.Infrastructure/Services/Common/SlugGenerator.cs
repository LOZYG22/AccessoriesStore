using AccessoriesStore.Application.Abstractions.Common;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AccessoriesStore.Infrastructure.Services.Common
{
    public class SlugGenerator : ISlugGenerator
    {
        public string Generate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text
                .Trim()
                .ToLowerInvariant()
                .Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder();

            foreach (var character in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(character);

                if (category == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(character);
                }
                else
                {
                    builder.Append('-');
                }
            }

            var slug = Regex.Replace(
                builder.ToString(),
                "-+",
                "-");

            return slug.Trim('-');
        }
    }
}