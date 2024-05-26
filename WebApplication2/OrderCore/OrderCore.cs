
namespace WebApplication2.OrderCore
{
    public class OrderCore : IOrderCore
    {
        public Task<string[]> ProcessAllAsync()
        {
            return Task.WhenAll(PushNotifyToShopAsync(), TrackActivityAsync());
        }

        public async Task<string> PushNotifyToShopAsync()
        {
            await Task.Run(() => Fib(40));
            return "Data from Notification";
        }

        public async Task<string> TrackActivityAsync()
        {
            await Task.Run(() => Fib(39));
            return "Data from Activity";
        }

        long Fib(long n)
        {
            if (n <= 1)
                return n;
            else
                return Fib(n - 1) + Fib(n - 2);
        }
    }
}
