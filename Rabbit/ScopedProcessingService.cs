using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using MongoService.Rabbit.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoService.Models;
using MongoService.Repositories.Interfaces;
using MongoService.DAL;

namespace MongoService.Rabbit
{
    internal class ScopedProcessingService : IScopedProcessingService
    {
        private int executionCount = 0;
        private readonly double hoursTillUpdate = 500;
        private readonly ILogger _logger;
        private readonly IUserRepository userRepository;
        private Dal dal = new Dal();
        public ScopedProcessingService(ILogger<ScopedProcessingService> logger/*, IUserRepository _userRepository*/ )
        {
            _logger = logger;
/*            userRepository = _userRepository;
*/        }
        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                "Scoped Processing Service is working for SHOWSSERVICE");
                var factory = new ConnectionFactory()
                {
                    HostName = "rabbitmq",
                    Port = 5672,
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(15)
                };
                IConnection connection = factory.CreateConnection();
                IModel channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: "topic_exchange", type: "topic");         //EXCHANGE creation

                var queueName = channel.QueueDeclare().QueueName;                       //QUEUE creation with random name

                channel.QueueBind(queue: queueName,
                                  exchange: "topic_exchange",
                                  routingKey: "user.forget");                    //BINDING creation

                Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>                                     //MESSAGE RECEIVING HANDLER
                {
                    Console.WriteLine("[!!!RECEIVING RECEIVING!!!]");
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;

                    User userToDelete = JsonConvert.DeserializeObject<User>(message);//TODO: Alles  behalve title en url is null maar in message niet???

                    await dal.DeleteUser(userToDelete.Name);

                    Console.WriteLine(" [x] Received and deleted '{0}':'{1}'",
                                      routingKey,
                                      message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
            await Task.Delay(TimeSpan.FromHours(hoursTillUpdate), stoppingToken);
        }
    }
}
