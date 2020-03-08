using System;
using System.Collections.Generic;
using System.Linq;

namespace hatenagroup
{
    class Program
    {
        static void Main(string[] args)
        {
            var records = Record.GetRecords("hatenagroup.matarillo.csv");
            Console.WriteLine(records.Count);
            var entries = new List<Entry>();
            foreach (var record in records)
            {
                entries.AddRange(Record.ToEntries(record));
            }
            var es = entries.Where(e => e.Comments.Count > 0);
            var wp = WordPressExporter.ToXml(es);
            Console.WriteLine(wp);
        }
    }
}
