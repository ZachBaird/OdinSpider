using AngleSharp.Html.Dom;
using OdinSpider.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OdinSpider.Extensions
{
    public static class LinkExtensions
    {
        public static bool CanAdd(IHtmlAnchorElement link, string course, IEnumerable<string> ignoreList, IEnumerable<Link> completedList)
        {
            if (!string.IsNullOrWhiteSpace(link.TextContent)
                && !ignoreList.Contains(link.Href)
                && !completedList.Select(l => l.Href).Contains(link.Href))
                return true;
            else
                return false;
        }

        public static bool CanAdd(Link link, string course, IEnumerable<string> ignoreList, IEnumerable<Link> completedList)
        {
            if (link.Href.Contains(course)
                || (!string.IsNullOrWhiteSpace(link.LinkText)
                && !ignoreList.Contains(link.Href)
                && !link.WasParsed
                && !completedList.Select(l => l.Href).Contains(link.Href)))
                return true;
            else
                return false;
        }

        public static bool ShouldParse(Link link, IEnumerable<Link> currentLinks)
        {
            var linkHrefs = currentLinks.Select(l => l.Href).ToList();

            if (linkHrefs.Contains(link.Href))
                return false;

            return true;
        }
    }
}
