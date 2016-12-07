using SqlToOracle.Core;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SqlToOracle.Console
{
    class Program
    {
        static void Main(string[] args)
        {

            ConsoleKeyInfo keyinfo;
            do
            {
                System.Console.WriteLine("--------------------------------------");
                System.Console.WriteLine("0. Sync without parameter");
                System.Console.WriteLine("1. Sync with truncate table");
                System.Console.WriteLine("2. Sync with date");
                System.Console.WriteLine("3. Sync with hierarchy");
                System.Console.WriteLine("4. Sync with source");
                System.Console.WriteLine("Choose:");
                string index = System.Console.ReadLine();

                var sync = new Synchronize();
                if (index == "0")
                {
                    sync.Run();
                }
                else
                {
                    System.Console.WriteLine("Enter tableName:");
                    string tableName = System.Console.ReadLine();
                    System.Console.WriteLine("Enter primary key:");
                    string primaryKey = System.Console.ReadLine();
                    if (index == "1")
                    {
                        System.Console.WriteLine("Do you want to truncate table in destination source (Y|N):");
                        string temp = System.Console.ReadLine();
                        bool isTruncated = string.Equals(temp, "Y", StringComparison.OrdinalIgnoreCase);
                        System.Console.Write("\nStart working….");
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        sync.Run(tableName, primaryKey, isTruncated);
                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        System.Console.WriteLine(String.Format("Excute time: {0} minutes", TimeSpan.FromMilliseconds(elapsedMs).TotalMinutes));
                    }
                    else if (index == "2")
                    {
                        System.Console.WriteLine("Date column:");
                        string dateColumn = System.Console.ReadLine();
                        System.Console.WriteLine("Start date:");
                        string startDate = System.Console.ReadLine();
                        System.Console.WriteLine("End date:");
                        string endDate = System.Console.ReadLine();

                        System.Console.Write("\nStart working….");
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        sync.Run(tableName, primaryKey, dateColumn, startDate, endDate);

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        System.Console.WriteLine(String.Format("Excute time: {0} minutes", TimeSpan.FromMilliseconds(elapsedMs).TotalMinutes));
                    }
                    else if (index == "3")
                    {
                        System.Console.WriteLine("Enter parent table name:");
                        string parentTableName = System.Console.ReadLine();
                        System.Console.WriteLine("Enter parent primary key:");
                        string parentPrimaryKey = System.Console.ReadLine();
                        System.Console.WriteLine("Date column:");
                        string dateColumn = System.Console.ReadLine();
                        System.Console.WriteLine("Start date:");
                        string startDate = System.Console.ReadLine();
                        System.Console.WriteLine("End date:");
                        string endDate = System.Console.ReadLine();

                        System.Console.Write("\nStart working….");
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        sync.Run(tableName, primaryKey, parentTableName, parentPrimaryKey, dateColumn, startDate, endDate);

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        System.Console.WriteLine(String.Format("Excute time: {0} minutes", TimeSpan.FromMilliseconds(elapsedMs).TotalMinutes));
                    }
                    else if (index == "4")
                    {
                        System.Console.WriteLine("Enter path:");
                        string path = System.Console.ReadLine();
                        string source = File.ReadAllText(path);

                        System.Console.Write("\nStart working….");
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        sync.Run(source, tableName, primaryKey);

                        watch.Stop();
                        var elapsedMs = watch.ElapsedMilliseconds;
                        System.Console.WriteLine(String.Format("Excute time: {0} minutes", TimeSpan.FromMilliseconds(elapsedMs).TotalMinutes));
                    }
                }
                System.Console.WriteLine("Press [ESC] to close...");
                keyinfo = System.Console.ReadKey();
                System.Console.WriteLine(keyinfo.Key + " was pressed");
            }
            while (keyinfo.Key != ConsoleKey.Escape);


            //Task t = Task.Factory.StartNew(() =>
            //                               {
            //                                   var sync = new Synchronize();
            //                                   sync.Run();
            //                               });
            //var watch = System.Diagnostics.Stopwatch.StartNew();
            //Thread t1 = new Thread(() =>
            //{
            //    for (int i = 0; i < 25; i++)
            //    {
            //        var sync = new Synchronize();
            //        sync.Run();
            //    }

            //});
            //Thread t2 = new Thread(() =>
            //{
            //    Thread.Sleep(5000);
            //    for (int i = 0; i < 25; i++)
            //    {
            //        var sync = new Synchronize();
            //        sync.Run();
            //    }
            //});
            //t1.Start();
            //t2.Start();
            //watch.Stop();
            //var elapsedMs = watch.ElapsedMilliseconds;
            //System.Console.WriteLine(String.Format("Excute time: {0} minutes", TimeSpan.FromMilliseconds(elapsedMs).TotalMinutes));
            //var sync = new Synchronize();
            //sync.Run();
            //System.Console.WriteLine("Press [ESC] to close...");
            //System.Console.ReadKey();

        }


    }

    public class ConsoleSpiner
    {
        int counter;
        public ConsoleSpiner()
        {
            counter = 0;
        }
        public void Turn()
        {
            counter++;
            switch (counter % 4)
            {
                case 0: System.Console.Write("/"); break;
                case 1: System.Console.Write("-"); break;
                case 2: System.Console.Write("\\"); break;
                case 3: System.Console.Write("-"); break;
            }
            System.Console.SetCursorPosition(0, System.Console.CursorTop);
        }
    }
}
