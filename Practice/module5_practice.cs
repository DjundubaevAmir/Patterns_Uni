using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public enum LogLevel
{
    Info = 1,
    Warning = 2,
    Error = 3
}

public sealed class Logger
{
    private static Logger _instance;
    private static readonly object _instanceLock = new object();
    private readonly object _fileLock = new object();

    private LogLevel _currentLevel = LogLevel.Info;
    private string _logPath = "logs/app.log";

    private Logger()
    {
        CreateFolder();
    }

    public static Logger GetInstance()
    {
        if (_instance == null)
        {
            lock (_instanceLock)
            {
                if (_instance == null)
                {
                    _instance = new Logger();
                }
            }
        }

        return _instance;
    }

    public void LoadConfig(string path)
    {
        lock (_fileLock)
        {
            if (!File.Exists(path))
            {
                List<string> lines = new List<string>();
                lines.Add("LogLevel=Info");
                lines.Add("LogFilePath=logs/app.log");
                File.WriteAllLines(path, lines);
            }

            string[] config = File.ReadAllLines(path);
            for (int i = 0; i < config.Length; i++)
            {
                string line = config[i].Trim();
                if (line == "" || !line.Contains("="))
                {
                    continue;
                }

                string[] parts = line.Split('=');
                if (parts.Length != 2)
                {
                    continue;
                }

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (key == "LogLevel")
                {
                    LogLevel parsed;
                    if (Enum.TryParse(value, true, out parsed))
                    {
                        _currentLevel = parsed;
                    }
                }

                if (key == "LogFilePath")
                {
                    _logPath = value;
                }
            }

            CreateFolder();
        }
    }

    public void SetLogLevel(LogLevel level)
    {
        lock (_fileLock)
        {
            _currentLevel = level;
        }
    }

    public void Log(string message, LogLevel level)
    {
        lock (_fileLock)
        {
            if (level < _currentLevel)
            {
                return;
            }

            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "|" + level + "|" + message;
            File.AppendAllText(_logPath, line + Environment.NewLine);
        }
    }

    public string GetFolder()
    {
        string folder = Path.GetDirectoryName(_logPath);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return ".";
        }

        return folder;
    }

    private void CreateFolder()
    {
        string folder = GetFolder();
        Directory.CreateDirectory(folder);
    }
}

public class LogRecord
{
    public DateTime Time;
    public LogLevel Level;
    public string Message;
}

public class LogReader
{
    public List<LogRecord> Read(string folder, LogLevel? minLevel = null)
    {
        List<LogRecord> records = new List<LogRecord>();

        if (!Directory.Exists(folder))
        {
            return records;
        }

        string[] files = Directory.GetFiles(folder, "*.log");
        for (int i = 0; i < files.Length; i++)
        {
            string[] lines = File.ReadAllLines(files[i]);
            for (int j = 0; j < lines.Length; j++)
            {
                LogRecord record;
                if (!TryParse(lines[j], out record))
                {
                    continue;
                }

                if (minLevel.HasValue && record.Level < minLevel.Value)
                {
                    continue;
                }

                records.Add(record);
            }
        }

        return records;
    }

    private bool TryParse(string line, out LogRecord record)
    {
        record = new LogRecord();

        string[] parts = line.Split('|');
        if (parts.Length < 3)
        {
            return false;
        }

        DateTime time;
        if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
        {
            return false;
        }

        LogLevel level;
        if (!Enum.TryParse(parts[1], true, out level))
        {
            return false;
        }

        record.Time = time;
        record.Level = level;
        record.Message = parts[2];
        return true;
    }
}


public class ReportStyle
{
    public string BackgroundColor;
    public string FontColor;
    public int FontSize;
}

public class Report
{
    public string Header;
    public string Content;
    public string Footer;
    public List<string> Sections;
    public ReportStyle Style;
    public string Format;

    public Report()
    {
        Header = "";
        Content = "";
        Footer = "";
        Sections = new List<string>();
        Style = new ReportStyle();
        Format = "TEXT";
    }

    public string Export()
    {
        if (Format == "HTML")
        {
            string html = "<html><body style='background:" + Style.BackgroundColor + ";color:" + Style.FontColor + ";font-size:" + Style.FontSize + "px'>";
            html += "<h1>" + Header + "</h1>";
            html += "<p>" + Content + "</p>";
            for (int i = 0; i < Sections.Count; i++)
            {
                html += "<div>" + Sections[i] + "</div>";
            }
            html += "<footer>" + Footer + "</footer></body></html>";
            return html;
        }

        if (Format == "PDF")
        {
            string pdf = "PDF\n" + Header + "\n" + Content + "\n";
            for (int i = 0; i < Sections.Count; i++)
            {
                pdf += Sections[i] + "\n";
            }
            pdf += Footer;
            return pdf;
        }

        string text = Header + "\n" + Content + "\n";
        for (int i = 0; i < Sections.Count; i++)
        {
            text += Sections[i] + "\n";
        }
        text += Footer;
        return text;
    }
}

public interface IReportBuilder
{
    void SetHeader(string header);
    void SetContent(string content);
    void SetFooter(string footer);
    void AddSection(string sectionName, string sectionContent);
    void SetStyle(ReportStyle style);
    Report GetReport();
}

public class TextReportBuilder : IReportBuilder
{
    private Report _report = new Report();

    public TextReportBuilder()
    {
        _report.Format = "TEXT";
    }

    public void SetHeader(string header) { _report.Header = header; }
    public void SetContent(string content) { _report.Content = content; }
    public void SetFooter(string footer) { _report.Footer = footer; }
    public void AddSection(string sectionName, string sectionContent) { _report.Sections.Add(sectionName + ": " + sectionContent); }
    public void SetStyle(ReportStyle style) { _report.Style = style; }
    public Report GetReport() { return _report; }
}

public class HtmlReportBuilder : IReportBuilder
{
    private Report _report = new Report();

    public HtmlReportBuilder()
    {
        _report.Format = "HTML";
    }

    public void SetHeader(string header) { _report.Header = header; }
    public void SetContent(string content) { _report.Content = content; }
    public void SetFooter(string footer) { _report.Footer = footer; }
    public void AddSection(string sectionName, string sectionContent) { _report.Sections.Add(sectionName + ": " + sectionContent); }
    public void SetStyle(ReportStyle style) { _report.Style = style; }
    public Report GetReport() { return _report; }
}

public class PdfReportBuilder : IReportBuilder
{
    private Report _report = new Report();

    public PdfReportBuilder()
    {
        _report.Format = "PDF";
    }

    public void SetHeader(string header) { _report.Header = header; }
    public void SetContent(string content) { _report.Content = content; }
    public void SetFooter(string footer) { _report.Footer = footer; }
    public void AddSection(string sectionName, string sectionContent) { _report.Sections.Add(sectionName + ": " + sectionContent); }
    public void SetStyle(ReportStyle style) { _report.Style = style; }
    public Report GetReport() { return _report; }
}

public class ReportDirector
{
    public Report ConstructReport(IReportBuilder builder, ReportStyle style)
    {
        builder.SetStyle(style);
        builder.SetHeader("Отчет");
        builder.SetContent("Основные данные");
        builder.AddSection("Раздел 1", "Значение 1");
        builder.AddSection("Раздел 2", "Значение 2");
        builder.SetFooter("Дата: " + DateTime.Now.ToString("dd.MM.yyyy"));
        return builder.GetReport();
    }
}


public class Weapon : ICloneable
{
    public string Name;
    public int Damage;

    public object Clone()
    {
        Weapon copy = new Weapon();
        copy.Name = Name;
        copy.Damage = Damage;
        return copy;
    }
}

public class Armor : ICloneable
{
    public string Name;
    public int Defense;

    public object Clone()
    {
        Armor copy = new Armor();
        copy.Name = Name;
        copy.Defense = Defense;
        return copy;
    }
}

public class Skill : ICloneable
{
    public string Name;
    public int Power;

    public object Clone()
    {
        Skill copy = new Skill();
        copy.Name = Name;
        copy.Power = Power;
        return copy;
    }
}

public class Character : ICloneable
{
    public string Name;
    public int Health;
    public int Strength;
    public int Agility;
    public int Intelligence;
    public Weapon Weapon;
    public Armor Armor;
    public List<Skill> Skills;

    public Character()
    {
        Skills = new List<Skill>();
    }

    public object Clone()
    {
        Character copy = new Character();
        copy.Name = Name;
        copy.Health = Health;
        copy.Strength = Strength;
        copy.Agility = Agility;
        copy.Intelligence = Intelligence;
        copy.Weapon = (Weapon)Weapon.Clone();
        copy.Armor = (Armor)Armor.Clone();

        for (int i = 0; i < Skills.Count; i++)
        {
            copy.Skills.Add((Skill)Skills[i].Clone());
        }

        return copy;
    }
}

public static class Program
{
    public static void Main()
    {
        TaskSingleton();


        TaskBuilder();


        TaskPrototype();
    }

    private static void TaskSingleton()
    {
        Console.WriteLine("Задание 1: Singleton");

        Logger logger = Logger.GetInstance();
        logger.LoadConfig("logger_config.txt");

        List<int> hashes = new List<int>();
        object hashesLock = new object();
        List<Task> tasks = new List<Task>();

        for (int i = 1; i <= 5; i++)
        {
            int number = i;
            tasks.Add(Task.Run(() =>
            {
                Logger log = Logger.GetInstance();
                lock (hashesLock)
                {
                    hashes.Add(log.GetHashCode());
                }

                log.Log("Поток " + number + " info", LogLevel.Info);
                log.Log("Поток " + number + " warning", LogLevel.Warning);
                log.Log("Поток " + number + " error", LogLevel.Error);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        logger.SetLogLevel(LogLevel.Error);
        logger.Log("Это info не запишется", LogLevel.Info);
        logger.Log("Это error запишется", LogLevel.Error);

        HashSet<int> unique = new HashSet<int>(hashes);
        Console.WriteLine("Уникальных экземпляров Logger: " + unique.Count);

        LogReader reader = new LogReader();
        List<LogRecord> errors = reader.Read(logger.GetFolder(), LogLevel.Error);
        Console.WriteLine("Найдено ошибок: " + errors.Count);
    }

    private static void TaskBuilder()
    {
        Console.WriteLine("Задание 2: Builder");

        ReportStyle style = new ReportStyle();
        style.BackgroundColor = "White";
        style.FontColor = "Black";
        style.FontSize = 14;

        ReportDirector director = new ReportDirector();

        Report text = director.ConstructReport(new TextReportBuilder(), style);
        Report html = director.ConstructReport(new HtmlReportBuilder(), style);
        Report pdf = director.ConstructReport(new PdfReportBuilder(), style);

        File.WriteAllText("report.txt", text.Export());
        File.WriteAllText("report.html", html.Export());
        File.WriteAllText("report.pdf", pdf.Export());

        Console.WriteLine("Созданы: report.txt, report.html, report.pdf");
    }

    private static void TaskPrototype()
    {
        Console.WriteLine("Задание 3: Prototype");

        Character original = new Character();
        original.Name = "Воин";
        original.Health = 100;
        original.Strength = 20;
        original.Agility = 12;
        original.Intelligence = 8;
        original.Weapon = new Weapon { Name = "Меч", Damage = 25 };
        original.Armor = new Armor { Name = "Щит", Defense = 15 };
        original.Skills.Add(new Skill { Name = "Удар", Power = 10 });

        Character clone = (Character)original.Clone();
        clone.Name = "Воин Клон";
        clone.Weapon.Name = "Топор";
        clone.Skills[0].Power = 30;

        Console.WriteLine("Оригинал: " + original.Name + ", оружие: " + original.Weapon.Name + ", сила: " + original.Skills[0].Power);
        Console.WriteLine("Клон: " + clone.Name + ", оружие: " + clone.Weapon.Name + ", сила: " + clone.Skills[0].Power);
    }
}
