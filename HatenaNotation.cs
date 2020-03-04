using System;
using System.Collections.Generic;
using System.IO;
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
            var state = State.None;
            foreach (var line in lines)
            {
                switch (state)
                {
                    case State.List1:
                        break;
                    default:
                        break;
                }
            }
            return builder.GetEntries();
        }

        static State Read(string line, Builder builder)
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
                return DoList(m, line, builder);
            }
            m = DefinitionMatcher.Match(line);
            if (m.Success)
            {
                builder.Append("<dl>");
                DoDefinition(m, line, builder);
                return State.Definition;
            }
            m = BeginBlockquoteMacther.Match(line);
            if (m.Success)
            {
                return State.Blockquote;
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
            return State.None;
        }

        private static string ParseInline(string line)
        {
            line = AsinMatcher.Replace(line, EvalAsin);
            line = IdMatcher.Replace(line, EvalId);
            line = FotolifeMatcher.Replace(line, EvalFotolife);
            line = HttpMatcher.Replace(line, EvalHttp);
            return line;
        }

        private static State DoBeginHtml(Match m, string line, Builder builder)
        {
            var html = line.Substring(m.Length).Trim();
            builder.Append(html);
            return State.Html;
        }

        private static State DoBeginSuprePre(Match m, Builder builder)
        {
            var lang = m.Groups[1].Value;
            builder.Append($"```{lang}");
            return State.SuperPre;
        }

        private static void DoDefinition(Match m, string line, Builder builder)
        {
            var dt = m.Groups[1].Value;
            var dd = line.Substring(m.Length).Trim();
            dd = ParseInline(dd);
            builder.Append($"<dt>{dt}</dt><dd>{dd}</dd>");
        }

        private static State DoList(Match m, string line, Builder builder)
        {
            var listMark =
                (m.Length == 1) ? "* " :
                (m.Length == 2) ? "* * " :
                (m.Length == 3) ? "* * * " :
                "";
            var listText = line.Substring(m.Length).Trim();
            listText = ParseInline(listText);
            builder.Append($"{listMark}{listText}");
            return State.None;
        }

        private static State DoSubHeader(Match m, string line, Builder builder)
        {
            var headerMark = new string('#', m.Length);
            var headerText = line.Substring(m.Length).Trim();
            headerText = ParseInline(headerText);
            builder.Append($"{headerMark} {headerText}");
            return State.None;
        }

        static State DoHeader(Match m, string line, Builder builder)
        {
            var unixtime = int.Parse(m.Groups[1].Value);
            var headerText = line.Substring(m.Length).Trim();
            var tags = new string[0];
            var mm = CategoryMatcher.Match(headerText);
            if (mm.Success)
            {
                tags = mm.Groups[1].Captures.Select(x => x.Value).ToArray();
                headerText = headerText.Substring(mm.Length);
            }
            headerText = ParseInline(headerText);
            builder.CreateEntry(unixtime, headerText, tags);
            return State.None;
        }

        static State ReadList1(string line, Builder builder)
        {
            throw new NotImplementedException();
        }

        static readonly Regex HeaderMatcher = new Regex("^\\*(\\d+)\\*");
        static readonly Regex SubHeaderMatcher = new Regex("^\\*{2,3}");
        static readonly Regex ListMatcher = new Regex("^-{1,3}");
        static readonly Regex DefinitionMatcher = new Regex("^:([^:]+):");
        static readonly Regex BeginBlockquoteMacther = new Regex("^>>$");
        static readonly Regex EndBlockquoteMacther = new Regex("^<<$");
        static readonly Regex BeginSuprePreMacther = new Regex("^>\\|([^|]*)\\|$");
        static readonly Regex EndSuprePreMacther = new Regex("^\\|\\|<$");
        static readonly Regex BeginHtmlMacther = new Regex("^><");
        static readonly Regex EndHtmlMacther = new Regex("><$");
        static readonly Regex CategoryMatcher = new Regex("(\\[([^\\]]+)\\])+");
        static readonly Regex HttpMatcher = new Regex("\\[(https?://[^\\]]+)(:title(=[^\\]]*)?)?\\]");

        static string EvalHttp(Match m)
        {
            var url = m.Groups[1].Value;
            var title =
                m.Groups[4].Success ? m.Groups[4].Value :
                m.Groups[2].Success ? "'title'" : // TOOD
                "";
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

        enum State
        {
            None,
            List1,
            List2,
            List3,
            Definition,
            Table,
            Blockquote,
            SuperPre,
            Html,
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
