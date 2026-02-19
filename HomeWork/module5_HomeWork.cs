using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public sealed class ConfigurationManager
{
    private static readonly object _lock = new object();
    private static ConfigurationManager _instance;

    private readonly Dictionary<string, string> _settings = new Dictionary<string, string>();
    private bool _loaded = false;
    private string _filePath = "";

    private ConfigurationManager()
    {
    }

    public static ConfigurationManager GetInstance()
    {
        if (_instance == null)
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new ConfigurationManager();
                }
            }
        }

        return _instance;
    }

    public void LoadFromFile(string path)
    {
        if (_loaded)
        {
            throw new InvalidOperationException("Настройки уже загружены.");
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Файл конфигурации не найден.", path);
        }

        string[] lines = File.ReadAllLines(path);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string trimmed = line.Trim();
            if (trimmed.StartsWith("#"))
            {
                continue;
            }

            string[] parts = trimmed.Split('=');
            if (parts.Length != 2)
            {
                throw new FormatException("Неверная строка: " + line);
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            _settings[key] = value;
        }

        _loaded = true;
        _filePath = path;
    }

    public void LoadFromExternalSource(Dictionary<string, string> source)
    {
        if (_loaded)
        {
            throw new InvalidOperationException("Настройки уже загружены.");
        }

        foreach (KeyValuePair<string, string> kv in source)
        {
            _settings[kv.Key] = kv.Value;
        }

        _loaded = true;
        _filePath = "";
    }

    public void SaveToFile(string path = null)
    {
        string targetPath = path;
        if (string.IsNullOrWhiteSpace(targetPath))
        {
            targetPath = _filePath;
        }

        if (string.IsNullOrWhiteSpace(targetPath))
        {
            throw new InvalidOperationException("Нет пути для сохранения.");
        }

        List<string> lines = new List<string>();
        foreach (KeyValuePair<string, string> kv in _settings)
        {
            string line = kv.Key + "=" + kv.Value;
            lines.Add(line);
        }

        File.WriteAllLines(targetPath, lines.ToArray());
    }

    public string GetSetting(string key)
    {
        if (!_settings.ContainsKey(key))
        {
            throw new KeyNotFoundException("Настройка не найдена: " + key);
        }

        string value = _settings[key];
        return value;
    }

    public void SetSetting(string key, string value)
    {
        _settings[key] = value;
    }

    public bool TryGetSetting(string key, out string value)
    {
        if (_settings.ContainsKey(key))
        {
            value = _settings[key];
            return true;
        }

        value = "";
        return false;
    }
}



public interface IReportBuilder
{
    void SetHeader(string header);
    void SetContent(string content);
    void SetFooter(string footer);
    Report GetReport();
}

public class Report
{
    public string Header { get; private set; }
    public string Content { get; private set; }
    public string Footer { get; private set; }
    public string Format { get; private set; }

    public Report(string header, string content, string footer, string format)
    {
        Header = header;
        Content = content;
        Footer = footer;
        Format = format;
    }

    public string Render()
    {
        if (Format == "HTML")
        {
            string html = "<html>\n";
            html += "  <body>\n";
            html += "    <h1>" + Header + "</h1>\n";
            html += "    <p>" + Content + "</p>\n";
            html += "    <footer>" + Footer + "</footer>\n";
            html += "  </body>\n";
            html += "</html>";
            return html;
        }

        string text = Header + "\n" + Content + "\n" + Footer;
        return text;
    }
}

public class TextReportBuilder : IReportBuilder
{
    private string _header = "";
    private string _content = "";
    private string _footer = "";

    public void SetHeader(string header)
    {
        _header = header;
    }

    public void SetContent(string content)
    {
        _content = content;
    }

    public void SetFooter(string footer)
    {
        _footer = footer;
    }

    public Report GetReport()
    {
        Report report = new Report(_header, _content, _footer, "TEXT");
        return report;
    }
}

public class HtmlReportBuilder : IReportBuilder
{
    private string _header = "";
    private string _content = "";
    private string _footer = "";

    public void SetHeader(string header)
    {
        _header = header;
    }

    public void SetContent(string content)
    {
        _content = content;
    }

    public void SetFooter(string footer)
    {
        _footer = footer;
    }

    public Report GetReport()
    {
        Report report = new Report(_header, _content, _footer, "HTML");
        return report;
    }
}

public class ReportDirector
{
    public Report ConstructReport(IReportBuilder builder, string header, string content, string footer)
    {
        builder.SetHeader(header);
        builder.SetContent(content);
        builder.SetFooter(footer);

        Report report = builder.GetReport();
        return report;
    }
}



public class Product : ICloneable
{
    public string Name;
    public decimal Price;
    public int Quantity;

    public Product(string name, decimal price, int quantity)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
    }

    public object Clone()
    {
        Product copy = new Product(Name, Price, Quantity);
        return copy;
    }
}

public class Discount : ICloneable
{
    public string Type;
    public decimal Amount;

    public Discount(string type, decimal amount)
    {
        Type = type;
        Amount = amount;
    }

    public object Clone()
    {
        Discount copy = new Discount(Type, Amount);
        return copy;
    }
}

public class Order : ICloneable
{
    public List<Product> Products = new List<Product>();
    public List<Discount> Discounts = new List<Discount>();
    public decimal ShippingCost;
    public string PaymentMethod = "";

    public object Clone()
    {
        Order copy = new Order();

        copy.ShippingCost = ShippingCost;
        copy.PaymentMethod = PaymentMethod;

        for (int i = 0; i < Products.Count; i++)
        {
            Product p = Products[i];
            copy.Products.Add((Product)p.Clone());
        }

        for (int i = 0; i < Discounts.Count; i++)
        {
            Discount d = Discounts[i];
            copy.Discounts.Add((Discount)d.Clone());
        }

        return copy;
    }
}

public static class Program
{
    public static void Main()
    {
        Console.WriteLine("=== Одиночка ===");

        string file = "config.txt";
        if (!File.Exists(file))
        {
            string[] defaultLines = new string[3];
            defaultLines[0] = "AppName=ShopApp";
            defaultLines[1] = "Theme=Light";
            defaultLines[2] = "MaxItems=50";
            File.WriteAllLines(file, defaultLines);
        }

        ConfigurationManager cfg = ConfigurationManager.GetInstance();
        cfg.LoadFromFile(file);

        string appName = cfg.GetSetting("AppName");
        Console.WriteLine("AppName: " + appName);

        List<int> ids = new List<int>();
        List<Task> tasks = new List<Task>();

        for (int i = 0; i < 5; i++)
        {
            Task t = Task.Run(() =>
            {
                ConfigurationManager inst = ConfigurationManager.GetInstance();
                lock (ids)
                {
                    ids.Add(inst.GetHashCode());
                }
            });
            tasks.Add(t);
        }

        Task.WaitAll(tasks.ToArray());
        int uniqueCount = ids.Distinct().Count();
        Console.WriteLine("Уникальных экземпляров: " + uniqueCount);

        cfg.SetSetting("Theme", "Dark");
        cfg.SaveToFile();

        Dictionary<string, string> external = new Dictionary<string, string>();
        external["FromDb"] = "Example";
        ConfigurationManager cfg2 = ConfigurationManager.GetInstance();
        if (cfg2.TryGetSetting("FromDb", out string _))
        {
            
        }

        Console.WriteLine("\n=== Строитель ===");

        ReportDirector director = new ReportDirector();

        Report textReport = director.ConstructReport(
            new TextReportBuilder(),
            "Отчет по продажам",
            "Всего продаж: 120",
            "Сформировано: " + DateTime.Now.ToString("u"));

        Report htmlReport = director.ConstructReport(
            new HtmlReportBuilder(),
            "Отчет по складу",
            "Товаров на складе: 340",
            "Сформировано: " + DateTime.Now.ToString("u"));

        Console.WriteLine("-- Текстовый отчет --");
        Console.WriteLine(textReport.Render());
        Console.WriteLine("-- HTML отчет --");
        Console.WriteLine(htmlReport.Render());

        Console.WriteLine("\n=== Прототип ===");

        Order baseOrder = new Order();
        baseOrder.ShippingCost = 9.99m;
        baseOrder.PaymentMethod = "Карта";
        baseOrder.Products.Add(new Product("Ноутбук", 1200m, 1));
        baseOrder.Products.Add(new Product("Мышь", 25m, 2));
        baseOrder.Discounts.Add(new Discount("Промо", 50m));

        Order cloneOrder = (Order)baseOrder.Clone();
        cloneOrder.Products[0].Price = 1100m;
        cloneOrder.PaymentMethod = "Наличные";
        cloneOrder.Discounts.Add(new Discount("VIP", 30m));

        Console.WriteLine("Оплата у оригинала: " + baseOrder.PaymentMethod);
        Console.WriteLine("Оплата у клона: " + cloneOrder.PaymentMethod);
        Console.WriteLine("Цена первого товара (оригинал): " + baseOrder.Products[0].Price);
        Console.WriteLine("Цена первого товара (клон): " + cloneOrder.Products[0].Price);
    }
}
