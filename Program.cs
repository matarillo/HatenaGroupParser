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
            var entries = new List<Entry>();
            foreach (var record in records)
            {
                entries.AddRange(Record.ToEntries(record));
            }
            var wp = WordPressExporter.ToXml(entries);
            Console.WriteLine(wp);
        }
    }
}
