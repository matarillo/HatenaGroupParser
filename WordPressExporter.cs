using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

using E = System.Xml.Linq.XElement;
using A = System.Xml.Linq.XAttribute;

namespace hatenagroup
{
    public static class WordPressExporter
    {
        public static XElement ToXml(IEnumerable<Entry> all)
        {
            XNamespace excerpt = "http://wordpress.org/export/1.2/excerpt/";
            XNamespace content = "http://purl.org/rss/1.0/modules/content/";
            XNamespace wfw = "http://wellformedweb.org/CommentAPI/";
            XNamespace dc = "http://purl.org/dc/elements/1.1/";
            XNamespace wp = "http://wordpress.org/export/1.2/";
            var rss =
                new E("rss",
                    new A("version", "2.0"),
                    new A("xmlns", ""),
                    new A(XNamespace.Xmlns + "wp", wp),
                    new A(XNamespace.Xmlns + "excerpt", excerpt),
                    new A(XNamespace.Xmlns + "wfw", wfw),
                    new A(XNamespace.Xmlns + "dc", dc),
                    new A(XNamespace.Xmlns + "content", content),
                    new E("channel",
                        new E("title", "HaskellとF#について書くmatarilloの日記（アーカイブ）"),
                        new E("link"),
                        new E("description"),
                        new E("pubDate", "Wed, 04 Mar 2020 09:33:30 +0000"),
                        new E("language", "ja"),
                        new E(wp + "wxr_version", "1.2"),
                        new E(wp + "base_site_url", "http://wordpress.com/"),
                        new E(wp + "base_blog_url", "https://matarillo.wordpress.com"),
                        new E("generator", "http://wordpress.com/")
                    )
                );
            var channel = rss.Element("channel");
            foreach (var e in all)
            {
                var item =
                    new E("item",
                        new E("title", e.Title),
                        new E("link"),
                        new E("pubDate", ToRfc1123(e.Date)),
                        new E(dc + "creator", "matarillo"),
                        new E("guid", new A("isPermaLink", "false")),
                        new E("description"),
                        new E(content + "encoded", new XCData(e.Body)),
                        new E(excerpt + "encoded", new XCData("")),
                        new E(wp + "post_id"),
                        new E(wp + "post_date", ToIso8601(e.Date)),
                        new E(wp + "post_date_gmt", ToIso8601(e.Date, DateTimeKind.Utc)),
                        new E(wp + "comment_status", "open"),
                        new E(wp + "ping_status", "open"),
                        new E(wp + "post_name"),
                        new E(wp + "status", "publish"),
                        new E(wp + "post_parent", "0"),
                        new E(wp + "menu_order", "0"),
                        new E(wp + "post_type", "post"),
                        new E(wp + "post_password"),
                        new E(wp + "is_sticky", "0")
                    );
                foreach (var t in e.Tags ?? new string[0])
                {
                    item.Add(
                        new E("category",
                            new A("domain", "category"),
                            new A("nicename", t),
                            new XCData(t)
                        )
                    );
                }
                foreach (var c in e.Comments ?? new Comment[0])
                {
                    item.Add(
                        new E(wp + "comment",
                            new E(wp + "comment_id"),
                            new E(wp + "comment_author", new XCData(c.Name)),
                            new E(wp + "comment_author_email"),
                            new E(wp + "comment_author_url"),
                            new E(wp + "comment_author_IP"),
                            new E(wp + "comment_approved", "1"),
                            new E(wp + "comment_type"),
                            new E(wp + "comment_parent", "0"),
                            new E(wp + "comment_date", ToIso8601(c.Date)),
                            new E(wp + "comment_date_gmt", ToIso8601(c.Date, DateTimeKind.Utc)),
                            new E(wp + "comment_content", new XCData(c.Body))
                        )
                    );
                }
                item.Add(
                    new E(wp + "postmeta",
                        new E(wp + "meta_key", "_wpcom_is_markdown"),
                        new E(wp + "meta_value", new XCData("1"))
                    )
                );
                channel.Add(item);
            }
            return rss;
        }
        private static string ToRfc1123(DateTime dt)
        {
            DateTimeOffset dto = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            return dto.ToString("ddd, dd MMM yyyy HH:mm:ss zzz", CultureInfo.InvariantCulture);
        }
        private static string ToIso8601(DateTime dt, DateTimeKind kind = DateTimeKind.Unspecified)
        {
            if (kind == DateTimeKind.Utc)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local).ToUniversalTime();
            }
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
