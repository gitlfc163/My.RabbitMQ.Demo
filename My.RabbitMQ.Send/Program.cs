//发送消息
using My.RabbitMQ.Config;
using RabbitMQ.Client;
using System.Text;

//建立连接
using (var connection = MQConnection.CreateConnection())

//创建信道
using (var channel = connection.CreateModel())
{
    //申明队列,队列名称（queue）为"hello"
    channel.QueueDeclare("hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
    //构建byte消息数据包
    string message = args.Length > 0 ? args[0] : "Hello RabbitMQ!";
    var body = Encoding.UTF8.GetBytes(message);
    //发送数据包
    channel.BasicPublish(exchange: "", routingKey: "hello", basicProperties: null, body: body);
    Console.WriteLine(" [x] Sent {0}", message);

}
