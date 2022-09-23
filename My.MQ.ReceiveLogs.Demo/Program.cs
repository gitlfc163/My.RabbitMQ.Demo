// 队列绑定交换机


//实例化连接
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

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
using (var channel = connection.CreateModel())
{
    //声明交换机类型为Fanout,交换机名称为"logs"的交换机
    channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout);

    // 声明一个以服务器命名的队列
    var queueName = channel.QueueDeclare(queue: "").QueueName;
    //交换机"logs"绑定一个队列
    channel.QueueBind(queue: queueName, exchange: "logs", routingKey: "");

    Console.WriteLine(" [*] Waiting for logs.");

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        byte[] body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] {0}", message);
    };
    //启动一个基本内容类消费者
    channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}
