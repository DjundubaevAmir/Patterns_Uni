using System;
using System;
using System.Collections.Generic;
using System.IO;

namespace SamplesUni
{
    static class Config
    {
        public static string ConnectionString = "Server=myServer;Database=myDb;User Id=myUser;Password=myPass;";
    }


    static class Log
    {
        public static void LogMessage(string level, string message)
        {
            Console.WriteLine($"{level}: {message}");
        }

        public static void Info(string m) => LogMessage("INFO", m);
        public static void Warn(string m) => LogMessage("WARN", m);
        public static void Error(string m) => LogMessage("ERROR", m);
    }

    class DatabaseService
    {
        string _conn;
        public DatabaseService(string conn) { _conn = conn; }
        public void Connect() { Console.WriteLine("Connect to: " + _conn); }
    }

    class LoggingService
    {
        string _conn;
        public LoggingService(string conn) { _conn = conn; }
        public void LogToDb(string text) { Console.WriteLine("Log to DB: " + text); }
    }

    static class SimpleProcessing
    {
        public static void ProcessNumbers(int[] numbers)
        {
            if (numbers == null) return;
            foreach (var n in numbers)
                if (n > 0) Console.WriteLine(n);
        }

        public static void PrintPositiveNumbers(int[] numbers)
        {
            if (numbers == null) return;
            var list = new List<int>();
            foreach (var n in numbers) if (n > 0) list.Add(n);
            list.Sort();
            foreach (var n in list) Console.WriteLine(n);
        }

        public static int Divide(int a, int b)
        {
            if (b == 0) return 0;
            return a / b;
        }
    }


    class User
    {
        public string Name;
        public string Email;
    }

    class UserRepository
    {
        string _conn;
        public UserRepository(string conn) { _conn = conn; }
        public void Save(User u) { Console.WriteLine("Save: " + (u?.Name ?? "<null>")); }
    }

    class FileReader
    {
        public string Read(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            try { return File.ReadAllText(path); }
            catch { return ""; }
        }
    }

    class ReportGenerator
    {
        public void GeneratePdf(string content)
        {
            Console.WriteLine("PDF len=" + (content?.Length ?? 0));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var conn = Config.ConnectionString;

            Log.Info("start");

            var db = new DatabaseService(conn);
            db.Connect();

            var logSvc = new LoggingService(conn);
            logSvc.LogToDb("hello");

            int[] nums = { 3, -1, 5, 0, 2 };
            SimpleProcessing.ProcessNumbers(nums);
            SimpleProcessing.PrintPositiveNumbers(nums);
            Console.WriteLine("10/2=" + SimpleProcessing.Divide(10, 2));
            Console.WriteLine("10/0=" + SimpleProcessing.Divide(10, 0));

            var user = new User { Name = "Ivan", Email = "ivan@example.com" };
            var repo = new UserRepository(conn);
            repo.Save(user);

            var reader = new FileReader();
            var content = reader.Read("nonexistent.txt");

            var rpt = new ReportGenerator();
            rpt.GeneratePdf(content);

            Log.Info("done");
        }
    }
}