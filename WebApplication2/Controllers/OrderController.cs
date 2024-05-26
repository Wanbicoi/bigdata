using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using WebApplication2.DTOs;
using WebApplication2.OrderCore;
using RabbitMQ.Client;
using System.Threading.Channels;

namespace WebApplication2.Controllers
{
    [Route("[controller]")]
    public class OrderController(IOrderCore orderCore) : Controller
    {
        private readonly IOrderCore _orderCore = orderCore;

        [HttpPost]
        public async Task<IActionResult> IndexAsync([FromBody] TestOrder testOrder)
        {
            var watch = new Stopwatch();
            watch.Start();

            for (int i = 0; i < testOrder.Times; i++)
            {
                if (testOrder.UsingRabbitMQ)
                {
                    ExecuteTasksWithRabbitMQ(testOrder.Times);
                }
                else
                {
                    await ExecuteTasksAsync(testOrder.Times);
                }
            }

            watch.Stop();

            return Ok("Number of proceed order: " + testOrder.Times + Environment.NewLine
                + "Total executed time: " + watch.ElapsedMilliseconds + "ms");
        }

        async Task ExecuteTasksAsync(int times)
        {
            await _orderCore.ProcessAllAsync();
        }

        void ExecuteTasksWithRabbitMQ(int times)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };

            using var connection = factory.CreateConnection();
            using var channel1 = connection.CreateModel();
            channel1.QueueDeclare(queue: "notifications",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var message1 = "Hello World!"; // should be data for nofitication
            var body1 = Encoding.UTF8.GetBytes(message1);

            var properties1 = channel1.CreateBasicProperties();
            properties1.Persistent = true;

            channel1.BasicPublish(exchange: string.Empty,
                                 routingKey: "task_queue",
                                 basicProperties: properties1,
                                 body: body1);

            using var channel2 = connection.CreateModel();
            channel2.QueueDeclare(queue: "activities",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

            var message2 = "Hello World!"; // should be data for nofitication
            var body2 = Encoding.UTF8.GetBytes(message2);

            var properties2 = channel1.CreateBasicProperties();
            properties2.Persistent = true;
            channel2.BasicPublish(exchange: string.Empty,
                                 routingKey: "hello",
                                 basicProperties: properties2,
                                 body: body2);
        }
    }
}
