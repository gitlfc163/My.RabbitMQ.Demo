//消息接收

//1.实例化连接工厂
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using My.RabbitMQ.Config;

//建立连接
using (var connection = MQConnection.CreateConnection())

//创建信道
using (var channel = connection.CreateModel())
{
    //申明队列,队列名称（queue）为"hello"
    //channel.QueueDeclare(queue: "hello", 
    //                    durable: false, 
    //                    exclusive: false, 
    //                    autoDelete: false, 
    //                    arguments: null);

    channel.QueueDeclare(queue: "durable_task",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

    //设置prefetchCount : 1来告知RabbitMQ，在未收到消费端的消息确认时，不再分发消息，也就确保了当消费端处于忙碌状态时
    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

    //构造消费者实例
    var consumer = new EventingBasicConsumer(channel);
    //绑定消息接收后的事件委托
    consumer.Received += (model, ea) =>
    {
        //报错
        //var message = Encoding.UTF8.GetString(ea.Body);

        var body = ea.Body.ToArray(); // 将内存区域的内容复制到一个新的数组中
                                      //var body = ea.Body.Span; // 从内存区域获取一个跨度
        var message = Encoding.UTF8.GetString(body);

        Console.WriteLine(" [x] Received {0}", message);

        //添加假任务以模拟执行时间
        int dots = message.Split('.').Length - 1;
        Thread.Sleep(dots * 6000);

        Console.WriteLine(" [x] Done");

        // 发送消息确认信号（手动消息确认）
        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    };
    //启动消费者
    //autoAck:true；自动进行消息确认，当消费端接收到消息后，就自动发送ack信号，不管消息是否正确处理完毕
    //autoAck:false；关闭自动消息确认，通过调用BasicAck方法手动进行消息确认
    channel.BasicConsume(queue: "hello", autoAck: false, consumer: consumer);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}
