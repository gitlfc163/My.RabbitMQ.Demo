// 消息确认

using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

class Program
{
    private const int MESSAGE_COUNT = 50_000;

    public static void Main()
    {
        PublishMessagesIndividually();
        PublishMessagesInBatch();
        HandlePublishConfirmsAsynchronously();
    }

    /// <summary>
    /// 实例化连接
    /// </summary>
    /// <returns></returns>
    private static IConnection CreateConnection()
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

    /// <summary>
    /// 逐一发布消息
    /// </summary>
    private static void PublishMessagesIndividually()
    {
        //建立连接
        using (var connection = CreateConnection())
        //创建信道
        using (var channel = connection.CreateModel())
        {
            // 申明队列,队列名称为空
            var queueName = channel.QueueDeclare(queue: "").QueueName;
            channel.ConfirmSelect();

            var timer = new Stopwatch();
            timer.Start();
            //循环逐一发布消息
            for (int i = 0; i < MESSAGE_COUNT; i++)
            {
                //构建byte消息数据包
                var body = Encoding.UTF8.GetBytes(i.ToString());
                //发送数据包
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
            }
            timer.Stop();
            Console.WriteLine($"逐一发布{MESSAGE_COUNT:N0} 条消息,共花费了{timer.ElapsedMilliseconds:N0} 毫秒");
        }
    }

    /// <summary>
    /// 批量发布消息
    /// </summary>
    private static void PublishMessagesInBatch()
    {
        //建立连接
        using (var connection = CreateConnection())
        //创建信道
        using (var channel = connection.CreateModel())
        {
            // 申明队列,队列名称
            var queueName = channel.QueueDeclare(queue: "").QueueName;
            channel.ConfirmSelect();

            var batchSize = 100;
            //未完成的消息计数
            var outstandingMessageCount = 0;
            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < MESSAGE_COUNT; i++)
            {
                //构建byte消息数据包
                var body = Encoding.UTF8.GetBytes(i.ToString());
                //发送数据包
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
                outstandingMessageCount++;

                if (outstandingMessageCount == batchSize)
                {
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                    outstandingMessageCount = 0;
                }
            }

            if (outstandingMessageCount > 0)
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));

            timer.Stop();
            Console.WriteLine($"批量发布 {MESSAGE_COUNT:N0}  条消息,共花费了 {timer.ElapsedMilliseconds:N0} 毫秒");
        }
    }
    /// <summary>
    /// 异步处理发布确认
    /// </summary>
    /// <exception cref="Exception"></exception>
    private static void HandlePublishConfirmsAsynchronously()
    {
        //建立连接
        using (var connection = CreateConnection())
        //创建信道
        using (var channel = connection.CreateModel())
        {
            //申明队列,队列名称
            var queueName = channel.QueueDeclare(queue: "").QueueName;
            channel.ConfirmSelect();

            var outstandingConfirms = new ConcurrentDictionary<ulong, string>();

            void cleanOutstandingConfirms(ulong sequenceNumber, bool multiple)
            {
                if (multiple)
                {
                    var confirmed = outstandingConfirms.Where(k => k.Key <= sequenceNumber);
                    foreach (var entry in confirmed)
                        outstandingConfirms.TryRemove(entry.Key, out _);
                }
                else
                    outstandingConfirms.TryRemove(sequenceNumber, out _);
            }

            channel.BasicAcks += (sender, ea) => cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            channel.BasicNacks += (sender, ea) =>
            {
                outstandingConfirms.TryGetValue(ea.DeliveryTag, out string body);
                Console.WriteLine($"Message with body {body} has been nack-ed. Sequence number: {ea.DeliveryTag}, multiple: {ea.Multiple}");
                cleanOutstandingConfirms(ea.DeliveryTag, ea.Multiple);
            };

            var timer = new Stopwatch();
            timer.Start();
            for (int i = 0; i < MESSAGE_COUNT; i++)
            {
                var body = i.ToString();
                outstandingConfirms.TryAdd(channel.NextPublishSeqNo, i.ToString());
                channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: Encoding.UTF8.GetBytes(body));
            }

            if (!WaitUntil(60, () => outstandingConfirms.IsEmpty))
                throw new Exception("All messages could not be confirmed in 60 seconds");

            timer.Stop();
            Console.WriteLine($"发布 {MESSAGE_COUNT:N0} 条消息,并异步处理确认花费了 {timer.ElapsedMilliseconds:N0} 毫秒");
        }
    }

    private static bool WaitUntil(int numberOfSeconds, Func<bool> condition)
    {
        int waited = 0;
        while (!condition() && waited < numberOfSeconds * 1000)
        {
            Thread.Sleep(100);
            waited += 100;
        }

        return condition();
    }
}