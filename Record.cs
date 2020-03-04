using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Supremes;

namespace hatenagroup
{
    public class Record
    {
        [Name("date")]
        public String Date { get; set; }
        [Name("title")]
        public string Title { get; set; }
        [Name("body")]
        public string Body { get; set; }
        [Name("comment")]
        public string Comment { get; set; }
        [Name("text")]
        public string Text { get; set; }

        public static IList<Record> GetRecords(string path)
        {
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<Record>();
                return records.ToList();
            }
        }

        public static IList<Comment> ToComments(DateTime entryDate, string commentString)
        {
            const string sep = "\ufffd\u0078\u000a";
            string[] namecomments = commentString.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            Comment[] comments = new Comment[namecomments.Length];
            for (var i = 0; i < namecomments.Length; i++)
            {
                var cd = entryDate.AddMinutes(i + 1);
                comments[i] = ToComment(cd, namecomments[i]);
            }
            return comments;
        }
        public static Comment ToComment(DateTime commentDate, string commentString)
        {
            const string sep = "\ufffd\u0077";
            string[] namecomment = commentString.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            if (namecomment.Length != 2)
            {
                throw new ArgumentException(commentString);
            }
            var doc = Dcsoup.ParseBodyFragment(namecomment[1]);
            var lines = doc.Body.TextNodes.Select(x => x.Text);
            return new Comment
            {
                Date = commentDate,
                Name = namecomment[0],
                Body = string.Join("\n", lines)
            };
        }

        public static IList<Entry> ToEntries(Record record)
        {
            var entries = HatenaNotation.ToMarkdownEntries(record.Body);
            var date = DateTime.ParseExact(record.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat);
            var comments = ToComments(date, record.Comment);
            if (entries.Count > 0)
            {
                entries[0].Comments = comments;
            }
            return entries;
        }
    }
}
