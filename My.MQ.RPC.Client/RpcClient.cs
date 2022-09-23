﻿using System;
using System.Collections.Concurrent;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


// RPC--RPC客户端
public class RpcClient
{
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
    private readonly IBasicProperties props;

    public RpcClient()
    {
        connection = CreateConnection();
        channel = connection.CreateModel();
        replyQueueName = channel.QueueDeclare().QueueName;
        consumer = new EventingBasicConsumer(channel);

        props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                respQueue.Add(response);
            }
        };

        channel.BasicConsume(
            consumer: consumer,
            queue: replyQueueName,
            autoAck: true);
    }

    public string Call(string message)
    {
        var messageBytes = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(
            exchange: "",
            routingKey: "rpc_queue",
            basicProperties: props,
            body: messageBytes);

        return respQueue.Take();
    }

    public void Close()
    {
        connection.Close();
    }
    //实例化连接
    IConnection CreateConnection()
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
}