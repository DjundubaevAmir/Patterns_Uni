using System;
using System.Collections.Generic;

namespace HomeWork
{
    public interface IPaymentStrategy
    {
        void Pay(decimal amount);
    }

    public class CardPaymentStrategy : IPaymentStrategy
    {
        public void Pay(decimal amount)
        {
            Console.WriteLine("Оплата банковской картой: " + amount + " тг.");
        }
    }

    public class PayPalPaymentStrategy : IPaymentStrategy
    {
        public void Pay(decimal amount)
        {
            Console.WriteLine("Оплата через PayPal: " + amount + " тг.");
        }
    }

    public class CryptoPaymentStrategy : IPaymentStrategy
    {
        public void Pay(decimal amount)
        {
            Console.WriteLine("Оплата криптовалютой: " + amount + " тг.");
        }
    }

    public class PaymentContext
    {
        private IPaymentStrategy _paymentStrategy;

        public void SetStrategy(IPaymentStrategy strategy)
        {
            _paymentStrategy = strategy;
        }

        public void ExecutePayment(decimal amount)
        {
            if (_paymentStrategy == null)
            {
                Console.WriteLine("Стратегия оплаты не выбрана.");
                return;
            }

            _paymentStrategy.Pay(amount);
        }
    }
    public interface IObserver
    {
        void Update(string currency, decimal rate);
    }

    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify(string currency, decimal rate);
    }

    public class CurrencyExchange : ISubject
    {
        private readonly List<IObserver> _observers = new List<IObserver>();
        private readonly Dictionary<string, decimal> _rates = new Dictionary<string, decimal>();

        public void Attach(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(string currency, decimal rate)
        {
            foreach (IObserver observer in _observers)
            {
                observer.Update(currency, rate);
            }
        }

        public void SetRate(string currency, decimal rate)
        {
            _rates[currency] = rate;
            Console.WriteLine("Новый курс: 1 " + currency + " = " + rate + " KZT");
            Notify(currency, rate);
        }
    }

    public class MobileObserver : IObserver
    {
        public void Update(string currency, decimal rate)
        {
            Console.WriteLine("[Mobile] Уведомление: 1 " + currency + " = " + rate + " KZT");
        }
    }

    public class EmailObserver : IObserver
    {
        public void Update(string currency, decimal rate)
        {
            Console.WriteLine("[Email] Отправлено письмо: 1 " + currency + " = " + rate + " KZT");
        }
    }

    public class AnalystObserver : IObserver
    {
        public void Update(string currency, decimal rate)
        {
            if (rate > 1000)
            {
                Console.WriteLine("[Analyst] Внимание: высокий курс " + currency + " = " + rate + " KZT");
            }
            else
            {
                Console.WriteLine("[Analyst] Курс в норме: " + currency + " = " + rate + " KZT");
            }
        }
    }

    public class Program
    {
        public static void Main()
        {
            RunStrategyDemo();
            Console.WriteLine();
            RunObserverDemo();
        }

        private static void RunStrategyDemo()
        {
            Console.WriteLine("=== Паттерн Стратегия ===");

            Console.Write("Введите сумму в тенге: ");
            string amountInput = Console.ReadLine();
            decimal amount;
            if (!decimal.TryParse(amountInput, out amount) || amount <= 0)
            {
                amount = 1000;
            }

            Console.WriteLine("Выберите способ оплаты:");
            Console.WriteLine("1 - Банковская карта");
            Console.WriteLine("2 - PayPal");
            Console.WriteLine("3 - Криптовалюта");
            Console.Write("Ваш выбор: ");

            string choice = Console.ReadLine();

            PaymentContext context = new PaymentContext();

            if (choice == "1")
            {
                context.SetStrategy(new CardPaymentStrategy());
            }
            else if (choice == "2")
            {
                context.SetStrategy(new PayPalPaymentStrategy());
            }
            else if (choice == "3")
            {
                context.SetStrategy(new CryptoPaymentStrategy());
            }
            else
            {
                Console.WriteLine("Неверный выбор. По умолчанию используется карта.");
                context.SetStrategy(new CardPaymentStrategy());
            }

            context.ExecutePayment(amount);

            Console.WriteLine("Оплата выполнена выбранным способом.");
        }

        private static void RunObserverDemo()
        {
            Console.WriteLine("=== Паттерн Наблюдатель ===");

            CurrencyExchange exchange = new CurrencyExchange();

            IObserver mobile = new MobileObserver();
            IObserver email = new EmailObserver();
            IObserver analyst = new AnalystObserver();

            exchange.Attach(mobile);
            exchange.Attach(email);
            exchange.Attach(analyst);

            // Демонстрационные курсы: 1 единица валюты в KZT.
            exchange.SetRate("USD", 495m);
            exchange.SetRate("EUR", 588m);

            Console.WriteLine("Удаляем EmailObserver...");
            exchange.Detach(email);

            exchange.SetRate("BTC", 33400000m);
        }
    }
}
