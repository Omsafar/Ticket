using HtmlAgilityPack;
using System.Xml;
using System.Linq;

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

            var tags = new[] { "br", "p", "div", "li" };
            foreach (var tag in tags)
            {
                var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
                if (nodes == null)
                    continue;
                foreach (var n in nodes)
                {
                    n.ParentNode.InsertBefore(doc.CreateTextNode("\n"), n);
                }
            }

            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText);
        }
    }
}