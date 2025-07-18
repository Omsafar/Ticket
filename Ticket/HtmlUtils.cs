using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Linq;

namespace TicketingApp
{
    public static class HtmlUtils
    {
        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
            // Convert the most common break tags to line breaks before
            // stripping the remaining HTML so that the resulting text
            // preserves the intended formatting.
            html = Regex.Replace(html, "<(br|BR)\\s*/?>", "\n");
            html = Regex.Replace(html, "</(p|P|div|DIV|li|LI)>", "\n");

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var text = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);

            // Normalize line endings and trim trailing whitespace.
            return string.Join("\n", text.Split('\n')
                .Select(t => t.TrimEnd()))
                .TrimEnd();
        }
    }
}