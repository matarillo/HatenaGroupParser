using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace hatenagroup
{
    public static class HatenaNotation
    {
        public static IList<Entry> ToMarkdownEntries(string text)
        {
            var builder = new Builder();
            var lines = text.Split('\n');
            var state = BlockState.None;
            foreach (var line in lines)
            {
            if (line.StartsWith("><div"))
            {
                var d = 0;
            }


                switch (state)
                {
                    case BlockState.Definition:
                        state = ReadDefinition(line, builder);
                        break;
                    case BlockState.Table:
                        state = ReadTable(line, builder);
                        break;
                    case BlockState.Blockquote:
                        state = ReadBlockquote(line, builder);
                        break;
                    case BlockState.SuperPre:
                        state = ReadSuperPre(line, builder);
                        break;
                    case BlockState.Html:
                        state = ReadHtml(line, builder);
                        break;
                    case BlockState.List:
                        state = ReadList(line, builder);
                        break;
                    case BlockState.None:
                    default:
                        state = Read(line, builder);
                        break;
                }
            }
            return builder.GetEntries();
        }

        static BlockState ReadDefinition(string line, Builder builder)
        {
            var m = DefinitionMatcher.Match(line);
            if (m.Success)
            {
                return DoDefinition(m, line, builder);
            }
            DoEndDefinition(builder);
            return Read(line, builder);
        }

        static BlockState ReadTable(string line, Builder builder)
        {
            var m = TableMatcher.Match(line);
            if (m.Success)
            {
                DoTable(m, builder);
                return BlockState.Table;
            }
            return Read(line, builder);
        }

        private static BlockState ReadBlockquote(string line, Builder builder)
        {
            var m = EndBlockquoteMatcher.Match(line);
            if (m.Success)
            {
                return BlockState.None;
            }
            return DoBlockquote(line, builder);
        }

        private static BlockState ReadSuperPre(string line, Builder builder)
        {
            var m = EndSuprePreMatcher.Match(line);
            if (m.Success)
            {
                return DoEndSuprePre(builder);
            }
            builder.Append(line);
            return BlockState.SuperPre;
        }

        private static BlockState ReadHtml(string line, Builder builder)
        {
            var m = EndHtmlMatcher.Match(line);
            if (m.Success)
            {
                return DoEndHtml(m, line, builder);
            }
            return DoHtml(line, builder);
        }

        private static BlockState ReadList(string line, Builder builder)
        {
            var m = ListMatcher.Match(line);
            if (m.Success)
            {
                return DoList(m, line, builder);
            }
            DoEndList(builder);
            return Read(line, builder);
        }

        static BlockState Read(string line, Builder builder)
        {
            var m = HeaderMatcher.Match(line);
            if (m.Success)
            {
                return DoHeader(m, line, builder);
            }
            m = SubHeaderMatcher.Match(line);
            if (m.Success)
            {
                return DoSubHeader(m, line, builder);
            }
            m = ListMatcher.Match(line);
            if (m.Success)
            {
                DoBeginList(builder);
                return DoList(m, line, builder);
            }
            m = DefinitionMatcher.Match(line);
            if (m.Success)
            {
                DoBeginDefinition(builder);
                return DoDefinition(m, line, builder);
            }
            m = TableMatcher.Match(line);
            if (m.Success)
            {
                DoTable(m, builder);
                DoTableSeparator(m, builder);
                return BlockState.Table;
            }
            m = BeginBlockquoteMacther.Match(line);
            if (m.Success)
            {
                // ignore m.Groups[1] (link notation)
                return BlockState.Blockquote;
            }
            m = BeginSuprePreMacther.Match(line);
            if (m.Success)
            {
                return DoBeginSuprePre(m, builder);
            }
            m = BeginHtmlMacther.Match(line);
            if (m.Success)
            {
                return DoBeginHtml(m, line, builder);
            }
            var markdownLine = ParseInline(line);
            builder.Append(markdownLine);
            return BlockState.None;
        }

        private static string ParseInline(string line)
        {
            line = AsinMatcher.Replace(line, EvalAsin);
            line = IdMatcher.Replace(line, EvalId);
            line = FotolifeMatcher.Replace(line, EvalFotolife);
            line = HttpMatcher.Replace(line, EvalHttp);
            return line;
        }

        private static BlockState DoBeginHtml(Match m, string line, Builder builder)
        {
            var html = line.Substring(m.Length).Trim();
            builder.Append(html);
            return BlockState.Html;
        }

        private static BlockState DoHtml(string line, Builder builder)
        {
            var html = line.Trim();
            builder.Append(html);
            return BlockState.Html;
        }

        private static BlockState DoEndHtml(Match m, string line, Builder builder)
        {
            var html = line.Substring(0, line.Length - m.Length).Trim();
            builder.Append(html);
            return BlockState.None;
        }

        private static BlockState DoBeginSuprePre(Match m, Builder builder)
        {
            var lang = m.Groups[1].Value;
            builder.Append($"```{lang}");
            return BlockState.SuperPre;
        }

        private static BlockState DoEndSuprePre(Builder builder)
        {
            builder.Append("```");
            return BlockState.None;
        }

        private static BlockState DoBlockquote(string line, Builder builder)
        {
            line = ParseInline(line);
            builder.Append("> " + line);
            return BlockState.Blockquote;
        }

        private static void DoTableSeparator(Match m, Builder builder)
        {
            var sb = new StringBuilder();
            sb.Append("|");
            foreach (Capture c in m.Groups[2].Captures)
            {
                sb.Append(" --- |");
            }
            builder.Append(sb.ToString());
        }

        private static void DoTable(Match m, Builder builder)
        {
            var sb = new StringBuilder();
            sb.Append("|");
            foreach (Capture c in m.Groups[2].Captures)
            {
                var td = c.Value.Trim();
                td = ParseInline(td);
                sb.Append(" ").Append(td).Append(" |");
            }
            builder.Append(sb.ToString());
        }

        private static void DoBeginDefinition(Builder builder)
        {
            builder.Append("<dl>");
        }

        private static BlockState DoDefinition(Match m, string line, Builder builder)
        {
            var dt = m.Groups[1].Value;
            var dd = line.Substring(m.Length).Trim();
            dd = ParseInline(dd);
            builder.Append($"<dt>{dt}</dt><dd>{dd}</dd>");
            return BlockState.Definition;
        }

        private static void DoEndDefinition(Builder builder)
        {
            builder.Append("</dl>");
        }

        private static void DoBeginList(Builder builder)
        {
            builder.Append("");
        }

        private static BlockState DoList(Match m, string line, Builder builder)
        {
            var listMark =
                (m.Length == 1) ? "* " :
                (m.Length == 2) ? "* * " :
                (m.Length == 3) ? "* * * " :
                "";
            var listText = line.Substring(m.Length).Trim();
            listText = ParseInline(listText);
            builder.Append($"{listMark}{listText}");
            return BlockState.List;
        }

        private static void DoEndList(Builder builder)
        {
            builder.Append("");
        }

        private static BlockState DoSubHeader(Match m, string line, Builder builder)
        {
            var headerMark = new string('#', m.Length);
            var headerText = line.Substring(m.Length).Trim();
            headerText = ParseInline(headerText);
            builder.Append($"{headerMark} {headerText}");
            return BlockState.None;
        }

        static BlockState DoHeader(Match m, string line, Builder builder)
        {
            var unixtime = int.Parse(m.Groups[1].Value);
            var headerText = line.Substring(m.Length).Trim();
            var tags = new string[0];
            var mm = CategoryMatcher.Match(headerText);
            if (mm.Success)
            {
                tags = mm.Groups[1].Captures.Select(x => x.Value.Substring(1, x.Length - 2)).ToArray();
                headerText = headerText.Substring(mm.Length);
            }
            headerText = ParseInline(headerText);
            builder.CreateEntry(unixtime, headerText, tags);
            return BlockState.None;
        }

        static readonly Regex HeaderMatcher = new Regex("^\\*(\\d+)\\*");
        static readonly Regex SubHeaderMatcher = new Regex("^\\*{2,3}");
        static readonly Regex ListMatcher = new Regex("^-{1,3}");
        static readonly Regex DefinitionMatcher = new Regex("^:([^:]+):");
        static readonly Regex TableMatcher = new Regex("^\\|(([^|]*)\\|)+$");
        static readonly Regex BeginBlockquoteMacther = new Regex("^>([^<>]+)?>$");
        static readonly Regex EndBlockquoteMatcher = new Regex("^<<$");
        static readonly Regex BeginSuprePreMacther = new Regex("^>\\|([^|]*)\\|$");
        static readonly Regex EndSuprePreMatcher = new Regex("^\\|\\|<$");
        static readonly Regex BeginHtmlMacther = new Regex("^>(?=<)");
        static readonly Regex EndHtmlMatcher = new Regex("(?<=>)<$");
        static readonly Regex CategoryMatcher = new Regex("(\\[([^\\]]+)\\])+");
        static readonly Regex HttpMatcher = new Regex("\\[(https?://[^\\]:]+)(:title(=[^\\]]*)?)?\\]");

        static string EvalHttp(Match m)
        {
            // new Regex("\\[(https?://[^\\]:]+)(:title(=[^\\]]*)?)?\\]");
            var url = m.Groups[1].Value;
            var title =
                m.Groups[4].Success ? m.Groups[4].Value :
                url;
            return $"[{title}]({url})";
        }

        static readonly Regex FotolifeMatcher = new Regex("\\[f:id:([^:]+):([^:]+):image:w(\\d+)]");

        static string EvalFotolife(Match m)
        {
            // f:id:matarillo:20120817105158j:image
            var hatenaid = m.Groups[1].Value;
            var photoid = m.Groups[2].Value;
            var photoDate = photoid.Substring(0, 8);
            var photoDateTime = photoid.Substring(0, 14);
            var extchar = photoid[photoid.Length - 1];
            var photoext = (extchar == 'p') ? "png" : "jpg";
            // var width = int.Parse(m.Groups[3].Value);
            return $"![](https://cdn-ak.f.st-hatena.com/images/fotolife/{hatenaid[0]}/{hatenaid}/{photoDate}/{photoDateTime}.{photoext})";
            // <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/m/matarillo/20120817/20120817105158.jpg">
        }

        static readonly Regex IdMatcher = new Regex("id:([a-zA-Z][0-9a-zA-Z-_]*[0-9a-zA-Z])");

        static string EvalId(Match m)
        {
            return $"[{m.Value}](https://profile.hatena.ne.jp/{m.Groups[1].Value}/)";
        }

        static readonly Regex AsinMatcher = new Regex("(asin|ASIN|isbn|ISBN):([0-9a-zA-Z]+):detail");

        static string EvalAsin(Match m)
        {
            return "https://www.amazon.co.jp/dp/" + m.Groups[2].Value;
        }

        enum BlockState
        {
            None,
            Definition,
            Table,
            Blockquote,
            SuperPre,
            Html,
            List,
        }

        class Builder
        {
            private List<Entry> entries = new List<Entry>();
            private Entry currentEntry = null;
            private StringBuilder sb = new StringBuilder();
            public void CreateEntry(int unixtime, string title, IList<string> tags)
            {
                Flush();
                currentEntry = new Entry
                {
                    Date = new DateTime(1970, 1, 1).AddSeconds(unixtime),
                    Title = title,
                    Tags = tags
                };
            }

            private void Flush()
            {
                if (currentEntry != null)
                {
                    currentEntry.Body = sb.ToString();
                    sb.Clear();
                    entries.Add(currentEntry);
                    currentEntry = null;
                }
            }

            public void Append(string markdownLine)
            {
                sb.Append(markdownLine).Append('\n');
            }

            public IList<Entry> GetEntries()
            {
                Flush();
                return entries;
            }
        }
    }
}
