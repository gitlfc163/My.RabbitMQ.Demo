// 发送日志--主题交换机

using My.RabbitMQ.Config;
using RabbitMQ.Client;
using System.Text;

//建立连接
using (var connection = MQConnection.CreateConnection())
//创建信道
using (var channel = connection.CreateModel())
{
    channel.ExchangeDeclare(exchange: "topic_logs",
                            type: "topic");

    var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
    var message = (args.Length > 1)
                  ? string.Join(" ", args.Skip(1).ToArray())
                  : "Hello World!";
    var body = Encoding.UTF8.GetBytes(message);
    channel.BasicPublish(exchange: "topic_logs",
                         routingKey: routingKey,
                         basicProperties: null,
                         body: body);
    Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
}