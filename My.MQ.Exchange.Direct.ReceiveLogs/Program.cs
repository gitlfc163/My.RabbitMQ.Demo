// 接收日志--直连型交换机

using My.RabbitMQ.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

//建立连接
using (var connection = MQConnection.CreateConnection())
//创建信道
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "direct_logs",type: "direct");
    var queueName = channel.QueueDeclare().QueueName;

    if (args.Length < 1)
    {
        Console.Error.WriteLine("Usage: {0} [info] [warning] [error]",
                                Environment.GetCommandLineArgs()[0]);
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
        Environment.ExitCode = 1;
        return;
    }
    //只可以是'info', 'warning', 'error'其中一种
    foreach (var severity in args)
    {
        channel.QueueBind(queue: queueName,
                          exchange: "direct_logs",
                          routingKey: severity);
    }

    Console.WriteLine(" [*] Waiting for messages.");

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        var routingKey = ea.RoutingKey;
        Console.WriteLine(" [x] Received '{0}':'{1}'",
                          routingKey, message);
    };
    channel.BasicConsume(queue: queueName,
                         autoAck: true,
                         consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}