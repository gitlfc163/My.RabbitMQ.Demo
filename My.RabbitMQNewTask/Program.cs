//发送消息
using RabbitMQ.Client;
using System.Text;

//建立连接
using (var connection = CreateConnection())

//创建信道
using (var channel = connection.CreateModel())
{
    //申明队列,队列名称（queue）为"hello"
    //channel.QueueDeclare(queue: "hello", 
    //                    durable: false, 
    //                    exclusive: false, 
    //                    autoDelete: false, 
    //                    arguments: null);

    //申明队列,队列名称（queue）为"hello",指定durable:true,告知rabbitmq对消息进行持久化
    channel.QueueDeclare(queue: "durable_task",
                 durable: true,
                 exclusive: false,
                 autoDelete: false,
                 arguments: null);

    //构建byte消息数据包
    //string message = args.Length > 0 ? args[0] : "Hello RabbitMQ!";
    //消息从控制台输入后
    var message = GetMessage(args);
    var body = Encoding.UTF8.GetBytes(message);

    //将消息标记为持久性 - 将IBasicProperties.SetPersistent设置为true
    var properties = channel.CreateBasicProperties();
    properties.Persistent = true;

    //发送数据包
    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
    Console.WriteLine(" [x] Sent {0}", message);

}

/// <summary>
/// 实例化连接
/// </summary>
/// <returns></returns>
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
//获取消息
string GetMessage(string[] args)
{
    return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
}