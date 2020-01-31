using System;
using System.Text.Json;
using StackExchange.Redis;

namespace RedisTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var redis = RedisStore.RedisCache;

           
            ConnectionMultiplexer connection = ConnectionMultiplexer.Connect("127.0.0.1:6379");

            int db = connection.GetDatabase().Database;

            string notificationChannel = $"__keyevent@{db}__:*";

            ISubscriber subscriber = connection.GetSubscriber();

            //for redis notification to work MUST run Redis Server and ENABLE notification on Redis-Cli
            //RUN THE FOLLOWING ON REDIS CLI
            //CONFIG SET ENABLE-KEYSPACE-EVENTS "KEA" 
            // KEA = All Events
            subscriber.Subscribe(notificationChannel, (notificationType, key) =>
            {
                var eventTrigger = GetKey(notificationType);
                switch (eventTrigger)
                {
                    case "expire":
                        Console.WriteLine($"Expiration set for key: {key}");
                        break;
                    case "expired":
                        Console.WriteLine($"Expired key: {key}");
                        break;
                    case "rename_from":
                        Console.WriteLine($"Rename Key from: {key}");
                        break;
                    case "rename_to":
                        Console.WriteLine($"Rename Key to: {key}");
                        break;
                    case "del":
                        Console.WriteLine($"Deleted Key: {key}");
                        break;
                    case "evicted":
                        Console.WriteLine($"Evicted Key: {key}");
                        break;
                    case "set":
                        Console.WriteLine($"Set key: {key}");
                        Console.WriteLine($"Value : {redis.StringGet(key.ToString())}");
                        break;
                    default:
                        Console.WriteLine($"Unhandled notification type : {key}");
                        break;
                };
            });

            Console.WriteLine("Subscribed to notifications...");
            Console.ReadKey();
        }

        private static string GetKey(string channel)
        {
            var index = channel.IndexOf(':');
            if (index >= 0 && index < channel.Length - 1)
                return channel.Substring(index + 1);

            //we didn't find the delimeter, so just return the whole thing
            return channel;
        }
    }

    public class RedisStore
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;

        static RedisStore()
        {
            var configurationOptions = new ConfigurationOptions
            {
                EndPoints = { "127.0.0.1:6379" }
            };

            LazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(configurationOptions));
        }

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public static IDatabase RedisCache => Connection.GetDatabase();
    }

    public class Basket
    {
        public string Id { get; set; }

        public decimal TransactionDiscounts { get; set; }
        public decimal DeliveryFee { get; set; }
        public bool IsSubmitted { get; set; }
        public string CreatedBy { get; set; }
        public string Country { get; set; }
        public DateTime CreateDate { get; set; }
        public bool IsCancelled { get; set; }
        public string PricingStoreCode { get; set; }

        public bool HasDiscountProfile { get; set; }

        public bool HasPickupStoreAssigned { get; set; }
    }
}
