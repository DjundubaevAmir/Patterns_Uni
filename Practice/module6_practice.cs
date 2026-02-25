using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Practice
{
    // ==========================================================
    // ЧАСТЬ 1. ПАТТЕРН STRATEGY: СИСТЕМА БРОНИРОВАНИЯ ПУТЕШЕСТВИЙ
    // ==========================================================

    public enum ServiceClass
    {
        Economy = 1,
        Business = 2
    }

    public class TripRequest
    {
        public decimal DistanceKm { get; set; }
        public int Passengers { get; set; }
        public ServiceClass ServiceClass { get; set; }
        public bool HasChildDiscount { get; set; }
        public bool HasSeniorDiscount { get; set; }
        public int BaggageCount { get; set; }
        public bool HasTransfer { get; set; }
        public bool IsGroupBooking { get; set; }
        public decimal RegionalCoefficient { get; set; }
    }

    public interface ICostCalculationStrategy
    {
        string TransportType { get; }
        decimal CalculateCost(TripRequest request);
    }

    public static class TripPricingHelper
    {
        public static void ValidateRequest(TripRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request", "Запрос на поездку не задан.");
            }

            if (request.DistanceKm <= 0)
            {
                throw new ArgumentException("Дистанция должна быть больше 0 км.");
            }

            if (request.Passengers <= 0)
            {
                throw new ArgumentException("Количество пассажиров должно быть больше 0.");
            }

            if (request.BaggageCount < 0)
            {
                throw new ArgumentException("Количество багажа не может быть отрицательным.");
            }

            if (request.RegionalCoefficient <= 0)
            {
                throw new ArgumentException("Региональный коэффициент должен быть больше 0.");
            }
        }

        public static decimal GetServiceClassMultiplier(ServiceClass serviceClass)
        {
            if (serviceClass == ServiceClass.Business)
            {
                return 1.6m;
            }

            return 1.0m;
        }

        public static decimal ApplyDiscounts(decimal cost, TripRequest request)
        {
            decimal discount = 0m;

            if (request.HasChildDiscount)
            {
                discount += 0.10m;
            }

            if (request.HasSeniorDiscount)
            {
                discount += 0.15m;
            }

            if (request.IsGroupBooking && request.Passengers >= 4)
            {
                discount += 0.07m;
            }

            if (discount > 0.30m)
            {
                discount = 0.30m;
            }

            return cost * (1m - discount);
        }

        public static decimal AddTransferFee(decimal cost, bool hasTransfer, decimal transferFee)
        {
            if (hasTransfer)
            {
                return cost + transferFee;
            }

            return cost;
        }

        public static decimal AddBaggageFee(decimal cost, int baggageCount, decimal baggagePrice)
        {
            return cost + (baggageCount * baggagePrice);
        }
    }

    public class PlaneCostStrategy : ICostCalculationStrategy
    {
        public string TransportType
        {
            get { return "Самолет"; }
        }

        public decimal CalculateCost(TripRequest request)
        {
            TripPricingHelper.ValidateRequest(request);

            decimal basePerKm = 8.0m;
            decimal fuelCoefficient = 1.12m;
            decimal airportTax = 900m;
            decimal baggagePrice = 350m;

            decimal cost = request.DistanceKm * basePerKm;
            cost = cost * fuelCoefficient;
            cost = cost * TripPricingHelper.GetServiceClassMultiplier(request.ServiceClass);
            cost = cost * request.Passengers;
            cost = cost * request.RegionalCoefficient;
            cost = cost + airportTax;
            cost = TripPricingHelper.AddBaggageFee(cost, request.BaggageCount, baggagePrice);
            cost = TripPricingHelper.AddTransferFee(cost, request.HasTransfer, 500m);
            cost = TripPricingHelper.ApplyDiscounts(cost, request);

            return Math.Round(cost, 2);
        }
    }

    public class TrainCostStrategy : ICostCalculationStrategy
    {
        public string TransportType
        {
            get { return "Поезд"; }
        }

        public decimal CalculateCost(TripRequest request)
        {
            TripPricingHelper.ValidateRequest(request);

            decimal basePerKm = 3.5m;
            decimal railwayServiceFee = 250m;
            decimal baggagePrice = 120m;

            decimal cost = request.DistanceKm * basePerKm;
            cost = cost * TripPricingHelper.GetServiceClassMultiplier(request.ServiceClass);
            cost = cost * request.Passengers;
            cost = cost * request.RegionalCoefficient;
            cost = cost + railwayServiceFee;
            cost = TripPricingHelper.AddBaggageFee(cost, request.BaggageCount, baggagePrice);
            cost = TripPricingHelper.AddTransferFee(cost, request.HasTransfer, 220m);
            cost = TripPricingHelper.ApplyDiscounts(cost, request);

            return Math.Round(cost, 2);
        }
    }

    public class BusCostStrategy : ICostCalculationStrategy
    {
        public string TransportType
        {
            get { return "Автобус"; }
        }

        public decimal CalculateCost(TripRequest request)
        {
            TripPricingHelper.ValidateRequest(request);

            decimal basePerKm = 2.2m;
            decimal roadTax = 90m;
            decimal baggagePrice = 80m;

            decimal cost = request.DistanceKm * basePerKm;

            if (request.ServiceClass == ServiceClass.Business)
            {
                cost = cost * 1.25m;
            }

            cost = cost * request.Passengers;
            cost = cost * request.RegionalCoefficient;
            cost = cost + roadTax;
            cost = TripPricingHelper.AddBaggageFee(cost, request.BaggageCount, baggagePrice);
            cost = TripPricingHelper.AddTransferFee(cost, request.HasTransfer, 120m);
            cost = TripPricingHelper.ApplyDiscounts(cost, request);

            return Math.Round(cost, 2);
        }
    }

    public class TravelBookingContext
    {
        private ICostCalculationStrategy _strategy;

        public void SetStrategy(ICostCalculationStrategy strategy)
        {
            _strategy = strategy;
        }

        public decimal CalculateTripCost(TripRequest request)
        {
            if (_strategy == null)
            {
                throw new InvalidOperationException("Стратегия расчета не выбрана.");
            }

            return _strategy.CalculateCost(request);
        }

        public string CurrentTransport
        {
            get
            {
                if (_strategy == null)
                {
                    return "Не выбран";
                }

                return _strategy.TransportType;
            }
        }
    }

    // ==========================================================
    // ЧАСТЬ 2. ПАТТЕРН OBSERVER: БИРЖЕВЫЕ ТОРГИ
    // ==========================================================

    public class StockPriceUpdate
    {
        public string StockSymbol { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime Time { get; set; }
        public string Reason { get; set; }
    }

    public interface IObserver
    {
        string Name { get; }
        Task OnPriceChangedAsync(StockPriceUpdate update);
    }

    public interface ISubject
    {
        void Subscribe(string stockSymbol, IObserver observer, decimal? minNotifyPrice = null);
        void Unsubscribe(string stockSymbol, IObserver observer);
        Task UpdateStockPriceAsync(string stockSymbol, decimal newPrice, string reason);
    }

    public class Subscription
    {
        public IObserver Observer { get; set; }
        public decimal? MinNotifyPrice { get; set; }
        public int NotificationCount { get; set; }
    }

    public class StockExchange : ISubject
    {
        private readonly Dictionary<string, decimal> _prices = new Dictionary<string, decimal>();
        private readonly Dictionary<string, List<Subscription>> _subscriptions = new Dictionary<string, List<Subscription>>();
        private readonly List<string> _eventLog = new List<string>();

        public void Subscribe(string stockSymbol, IObserver observer, decimal? minNotifyPrice = null)
        {
            if (string.IsNullOrWhiteSpace(stockSymbol))
            {
                throw new ArgumentException("Символ акции не задан.");
            }

            if (observer == null)
            {
                throw new ArgumentNullException("observer", "Наблюдатель не задан.");
            }

            string symbol = stockSymbol.ToUpper();

            if (!_subscriptions.ContainsKey(symbol))
            {
                _subscriptions[symbol] = new List<Subscription>();
            }

            bool alreadySubscribed = _subscriptions[symbol].Any(s => s.Observer == observer);
            if (alreadySubscribed)
            {
                Log("Повторная подписка пропущена: " + observer.Name + " -> " + symbol);
                return;
            }

            _subscriptions[symbol].Add(new Subscription
            {
                Observer = observer,
                MinNotifyPrice = minNotifyPrice,
                NotificationCount = 0
            });

            Log("Добавлена подписка: " + observer.Name + " -> " + symbol +
                (minNotifyPrice.HasValue ? " (фильтр от " + minNotifyPrice.Value + ")" : ""));
        }

        public void Unsubscribe(string stockSymbol, IObserver observer)
        {
            if (string.IsNullOrWhiteSpace(stockSymbol) || observer == null)
            {
                return;
            }

            string symbol = stockSymbol.ToUpper();

            if (!_subscriptions.ContainsKey(symbol))
            {
                return;
            }

            Subscription subscription = _subscriptions[symbol].FirstOrDefault(s => s.Observer == observer);
            if (subscription != null)
            {
                _subscriptions[symbol].Remove(subscription);
                Log("Удалена подписка: " + observer.Name + " -> " + symbol);
            }
        }

        public async Task UpdateStockPriceAsync(string stockSymbol, decimal newPrice, string reason)
        {
            if (string.IsNullOrWhiteSpace(stockSymbol))
            {
                throw new ArgumentException("Символ акции не задан.");
            }

            if (newPrice <= 0)
            {
                throw new ArgumentException("Цена акции должна быть больше 0.");
            }

            string symbol = stockSymbol.ToUpper();
            _prices[symbol] = newPrice;

            Log("Цена обновлена: " + symbol + " = " + newPrice + " | Причина: " + reason);

            if (!_subscriptions.ContainsKey(symbol) || _subscriptions[symbol].Count == 0)
            {
                Log("Подписчиков для " + symbol + " нет.");
                return;
            }

            List<Task> notifications = new List<Task>();

            foreach (Subscription sub in _subscriptions[symbol])
            {
                if (sub.MinNotifyPrice.HasValue && newPrice < sub.MinNotifyPrice.Value)
                {
                    continue;
                }

                StockPriceUpdate update = new StockPriceUpdate
                {
                    StockSymbol = symbol,
                    NewPrice = newPrice,
                    Time = DateTime.Now,
                    Reason = reason
                };

                notifications.Add(SendNotificationAsync(sub, update));
            }

            await Task.WhenAll(notifications);
        }

        private async Task SendNotificationAsync(Subscription sub, StockPriceUpdate update)
        {
            await Task.Delay(100);

            await sub.Observer.OnPriceChangedAsync(update);
            sub.NotificationCount++;

            Log("Уведомлен: " + sub.Observer.Name + " по акции " + update.StockSymbol);
        }

        public void PrintSubscriptionsReport()
        {
            Console.WriteLine("\n=== Отчет по подпискам ===");

            if (_subscriptions.Count == 0)
            {
                Console.WriteLine("Подписок нет.");
                return;
            }

            foreach (KeyValuePair<string, List<Subscription>> item in _subscriptions)
            {
                Console.WriteLine("Акция: " + item.Key);
                if (item.Value.Count == 0)
                {
                    Console.WriteLine("  Подписчиков нет.");
                    continue;
                }

                foreach (Subscription sub in item.Value)
                {
                    string filterText = sub.MinNotifyPrice.HasValue
                        ? "фильтр >= " + sub.MinNotifyPrice.Value
                        : "без фильтра";

                    Console.WriteLine("  " + sub.Observer.Name + " | " + filterText + " | уведомлений: " + sub.NotificationCount);
                }
            }
        }

        public void PrintLog()
        {
            Console.WriteLine("\n=== Лог событий биржи ===");

            if (_eventLog.Count == 0)
            {
                Console.WriteLine("Лог пуст.");
                return;
            }

            foreach (string logLine in _eventLog)
            {
                Console.WriteLine(logLine);
            }
        }

        private void Log(string text)
        {
            string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text;
            _eventLog.Add(line);
        }
    }

    public class TraderObserver : IObserver
    {
        public string Name { get; private set; }

        public TraderObserver(string name)
        {
            Name = name;
        }

        public Task OnPriceChangedAsync(StockPriceUpdate update)
        {
            Console.WriteLine("[Трейдер " + Name + "] " + update.StockSymbol + " -> " + update.NewPrice + " | " + update.Reason);
            return Task.CompletedTask;
        }
    }

    public class TradingRobotObserver : IObserver
    {
        public string Name { get; private set; }

        private readonly Dictionary<string, decimal> _buyThreshold;
        private readonly Dictionary<string, decimal> _sellThreshold;

        public TradingRobotObserver(string name,
            Dictionary<string, decimal> buyThreshold,
            Dictionary<string, decimal> sellThreshold)
        {
            Name = name;
            _buyThreshold = buyThreshold;
            _sellThreshold = sellThreshold;
        }

        public Task OnPriceChangedAsync(StockPriceUpdate update)
        {
            decimal buyLevel;
            decimal sellLevel;

            bool hasBuy = _buyThreshold.TryGetValue(update.StockSymbol, out buyLevel);
            bool hasSell = _sellThreshold.TryGetValue(update.StockSymbol, out sellLevel);

            if (hasBuy && update.NewPrice <= buyLevel)
            {
                Console.WriteLine("[Робот " + Name + "] BUY " + update.StockSymbol + " по цене " + update.NewPrice);
            }
            else if (hasSell && update.NewPrice >= sellLevel)
            {
                Console.WriteLine("[Робот " + Name + "] SELL " + update.StockSymbol + " по цене " + update.NewPrice);
            }
            else
            {
                Console.WriteLine("[Робот " + Name + "] HOLD " + update.StockSymbol + " (" + update.NewPrice + ")");
            }

            return Task.CompletedTask;
        }
    }

    public class MobileAppObserver : IObserver
    {
        public string Name { get; private set; }

        public MobileAppObserver(string name)
        {
            Name = name;
        }

        public Task OnPriceChangedAsync(StockPriceUpdate update)
        {
            Console.WriteLine("[Mobile " + Name + "] Push: " + update.StockSymbol + " = " + update.NewPrice);
            return Task.CompletedTask;
        }
    }

    public class Program
    {
        public static async Task Main()
        {
            RunTravelBookingDemo();
            await RunStockExchangeDemoAsync();
        }

        private static void RunTravelBookingDemo()
        {
            Console.WriteLine("=== Модуль 06: Strategy (Бронирование путешествий) ===");

            TravelBookingContext context = new TravelBookingContext();
            TripRequest request = ReadTripRequestFromUser();

            ICostCalculationStrategy strategy = SelectTransportStrategyFromUser();
            context.SetStrategy(strategy);

            try
            {
                decimal totalCost = context.CalculateTripCost(request);
                Console.WriteLine("Транспорт: " + context.CurrentTransport);
                Console.WriteLine("Итоговая стоимость: " + totalCost + " руб.");

                Console.WriteLine("\nПереключаем стратегию на Поезд с теми же данными...");
                context.SetStrategy(new TrainCostStrategy());
                decimal trainCost = context.CalculateTripCost(request);
                Console.WriteLine("Новая стоимость (Поезд): " + trainCost + " руб.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка расчета: " + ex.Message);
            }
        }

        private static TripRequest ReadTripRequestFromUser()
        {
            decimal distance = ReadDecimal("Введите расстояние (км): ", 1, 100000);
            int passengers = ReadInt("Введите количество пассажиров: ", 1, 200);

            Console.Write("Класс обслуживания (1 - Эконом, 2 - Бизнес): ");
            string classText = Console.ReadLine();
            ServiceClass serviceClass = classText == "2" ? ServiceClass.Business : ServiceClass.Economy;

            bool childDiscount = ReadYesNo("Есть дети (скидка 10%)? (y/n): ");
            bool seniorDiscount = ReadYesNo("Есть пенсионеры (скидка 15%)? (y/n): ");
            int baggage = ReadInt("Количество мест багажа: ", 0, 100);
            bool transfer = ReadYesNo("Есть пересадка? (y/n): ");
            bool groupBooking = ReadYesNo("Групповое бронирование? (y/n): ");
            decimal regionCoef = ReadDecimal("Региональный коэффициент (например 1.0, 1.2): ", 0.5m, 3.0m);

            TripRequest request = new TripRequest
            {
                DistanceKm = distance,
                Passengers = passengers,
                ServiceClass = serviceClass,
                HasChildDiscount = childDiscount,
                HasSeniorDiscount = seniorDiscount,
                BaggageCount = baggage,
                HasTransfer = transfer,
                IsGroupBooking = groupBooking,
                RegionalCoefficient = regionCoef
            };

            return request;
        }

        private static ICostCalculationStrategy SelectTransportStrategyFromUser()
        {
            Console.WriteLine("Выберите тип транспорта:");
            Console.WriteLine("1 - Самолет");
            Console.WriteLine("2 - Поезд");
            Console.WriteLine("3 - Автобус");
            Console.Write("Ваш выбор: ");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                return new PlaneCostStrategy();
            }

            if (choice == "2")
            {
                return new TrainCostStrategy();
            }

            if (choice == "3")
            {
                return new BusCostStrategy();
            }

            Console.WriteLine("Неверный выбор. Используется стратегия Поезд по умолчанию.");
            return new TrainCostStrategy();
        }

        private static async Task RunStockExchangeDemoAsync()
        {
            Console.WriteLine("\n=== Модуль 06: Observer (Биржевые торги) ===");

            StockExchange exchange = new StockExchange();

            IObserver traderIvan = new TraderObserver("Амир");
            IObserver mobileNina = new MobileAppObserver("Алихан");
            IObserver robot = new TradingRobotObserver(
                "RBT-01",
                new Dictionary<string, decimal>
                {
                    { "AAPL", 170m },
                    { "TSLA", 210m }
                },
                new Dictionary<string, decimal>
                {
                    { "AAPL", 210m },
                    { "TSLA", 280m }
                });

            exchange.Subscribe("AAPL", traderIvan);
            exchange.Subscribe("TSLA", traderIvan, 200m);
            exchange.Subscribe("AAPL", mobileNina, 180m);
            exchange.Subscribe("TSLA", robot);
            exchange.Subscribe("AAPL", robot);

            await exchange.UpdateStockPriceAsync("AAPL", 175m, "Обычная торговая сессия");
            await exchange.UpdateStockPriceAsync("TSLA", 205m, "Позитивные новости компании");
            await exchange.UpdateStockPriceAsync("AAPL", 215m, "Публикация квартального отчета");

            exchange.Unsubscribe("AAPL", mobileNina);

            await exchange.UpdateStockPriceAsync("AAPL", 168m, "Коррекция рынка");
            await exchange.UpdateStockPriceAsync("TSLA", 290m, "Рост спроса на электромобили");

            exchange.PrintSubscriptionsReport();
            exchange.PrintLog();
        }

        private static decimal ReadDecimal(string message, decimal min, decimal max)
        {
            while (true)
            {
                Console.Write(message);
                string text = Console.ReadLine();
                decimal value;
                bool ok = decimal.TryParse(text, out value);

                if (ok && value >= min && value <= max)
                {
                    return value;
                }

                Console.WriteLine("Некорректное значение. Повторите ввод.");
            }
        }

        private static int ReadInt(string message, int min, int max)
        {
            while (true)
            {
                Console.Write(message);
                string text = Console.ReadLine();
                int value;
                bool ok = int.TryParse(text, out value);

                if (ok && value >= min && value <= max)
                {
                    return value;
                }

                Console.WriteLine("Некорректное значение. Повторите ввод.");
            }
        }

        private static bool ReadYesNo(string message)
        {
            while (true)
            {
                Console.Write(message);
                string text = Console.ReadLine();

                if (string.Equals(text, "y", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(text, "n", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                Console.WriteLine("Введите только y или n.");
            }
        }
    }
}
