using HtmlAgilityPack;
using System.Xml;

namespace TicketingApp
{
    public static class HtmlUtils
    {
        public static string ToPlainText(string? html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
        }
    }
}