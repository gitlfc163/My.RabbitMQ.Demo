//消息接收

//1.实例化连接工厂
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using My.RabbitMQ.Config;


//建立连接
using (var connection = MQConnection.CreateConnection())
//3. 创建信道
using (var channel = connection.CreateModel())
{
    //4. 申明队列,队列名称（queue）为"hello"
    channel.QueueDeclare(queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);
    //5. 构造消费者实例
    var consumer = new EventingBasicConsumer(channel);
    //6. 绑定消息接收后的事件委托
    consumer.Received += (model, ea) =>
    {
        //报错
        //var message = Encoding.UTF8.GetString(ea.Body);

        var body = ea.Body.ToArray(); // 将内存区域的内容复制到一个新的数组中
                                      //var body = ea.Body.Span; // 从内存区域获取一个跨度
        var message = Encoding.UTF8.GetString(body);

        Console.WriteLine(" [x] Received {0}", message);
        Thread.Sleep(6000);//模拟耗时
        Console.WriteLine(" [x] Done");
    };
    //7. 启动消费者
    channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}
