using System;
using System.Collections.Generic;

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
            Console.WriteLine(entries.Count);
        }
    }
}
