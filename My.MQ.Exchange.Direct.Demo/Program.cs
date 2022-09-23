// 发出日志--直连型交换机

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

//实例化连接
static IConnection CreateConnection()
{
    var factory = new ConnectionFactory
    {
        HostName = "localhost",
        UserName = "admin",
        Password = "admin",
        Port = 5672,
        //VirtualHost= "myRabbit"
    };
    return factory.CreateConnection();
}
//建立连接
using (var connection = CreateConnection())
//创建信道
using (var channel = connection.CreateModel())
{
    //创建一个交换机
    channel.ExchangeDeclare(exchange: "direct_logs",
                                    type: "direct");
    //消息类型,假设严重等级只可以是'info', 'warning', 'error'其中一种
    var severity = (args.Length > 0) ? args[0] : "info";
    //构建byte消息数据包
    var message = (args.Length > 1)
                  ? string.Join(" ", args.Skip(1).ToArray())
                  : "Hello World!";
    var body = Encoding.UTF8.GetBytes(message);

    //发送数据包
    channel.BasicPublish(exchange: "direct_logs",
                         routingKey: severity,
                         basicProperties: null,
                         body: body);
    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}