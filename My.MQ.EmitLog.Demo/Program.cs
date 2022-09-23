// 日志消息示例

using RabbitMQ.Client;
using System.Text;

//建立连接
using (var connection = CreateConnection())
//创建信道
using (var channel = connection.CreateModel())
{
    //声明交换机类型为Fanout,交换机名称为"logs"的交换机
    channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

    //构建byte消息数据包
    var message = GetMessage(args);
    var body = Encoding.UTF8.GetBytes(message);
    //发送数据包
    channel.BasicPublish(exchange: "logs", routingKey: "", basicProperties: null, body: body);
    Console.WriteLine(" [x] Sent {0}", message);
}

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();

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
static string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "info: Hello World!");
}