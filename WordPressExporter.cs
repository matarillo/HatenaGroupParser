using System.Collections.Generic;
using System.Xml.Linq;

using E = System.Xml.Linq.XElement;
using A = System.Xml.Linq.XAttribute;
using System.Linq;
using System;
using System.Globalization;

namespace hatenagroup
{
    public static class WordPressExporter
    {
        public static XDocument ToXml(IEnumerable<Entry> all)
        {
            var rss =
                new E("rss", new A("version", "2.0"),
                    new E("channel",
                        new E("title", "HaskellとF#について書くmatarilloの日記（アーカイブ）")
                    )
                );
            var channel = rss.Element("channel");
            foreach (var e in all)
            {
                var item =
                    new E("item",
                        new E("title", e.Title),
                        new E("pubDate", ToRfc1123(e.Date)),
                        new E("content:encoded",
                            new XCData(e.Body)
                        )
                    );
                foreach (var t in e.Tags)
                {
                    item.Add(
                        new E("category", new A("nicename", t),
                            new XCData(t)
                        )
                    );
                }
                foreach (var c in e.Comments)
                {
                    item.Add(
                        new E("wp:comment",
                            new E("comment_author", new XCData(c.Name)),
                            new E("wp:comment_date", ToIso8601(c.Date)),
                            new E("wp:comment_date_gmt", ToIso8601(c.Date, DateTimeKind.Utc)),
                            new E("wp:comment_content", new XCData(c.Body))
                        )
                    );
                }
                item.Add(
                    new E("wp:postmeta",
                        new E("wp:meta_key", "_wpcom_is_markdown"),
                        new E("wp:meta_value", new XCData("1"))
                    ),
                    new E("wp:postmeta",
                        new E("wp:meta_key", "_wpcom_is_markdown"),
                        new E("wp:meta_value", new XCData("1"))
                    ),
                    new E("")
                );
                channel.Add(item);
            }
            return rss.Document;
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
