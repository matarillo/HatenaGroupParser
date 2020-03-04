using System;
using System.Linq;

namespace hatenagroup
{
    class Program
    {
        static void Main(string[] args)
        {
            var entries = Record.GetRecords("hatenagroup.matarillo.csv");
            var es = entries.Where(x => !string.IsNullOrEmpty(x.Comment));
            foreach (var e in es)
            {
                Console.WriteLine("----date");
                Console.WriteLine(e.Date);
                var comments = Record.ToComments(DateTime.Now, e.Comment);
                foreach (var c in comments)
                {
                    Console.WriteLine("----name");
                    Console.WriteLine(c.Name);
                    Console.WriteLine("----body");
                    Console.WriteLine(c.Body);
                }
                Console.WriteLine();
            }
        }
    }
}
