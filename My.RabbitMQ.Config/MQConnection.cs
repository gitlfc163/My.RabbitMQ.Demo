using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace My.RabbitMQ.Config;

public class MQConnection
{
    public static IConnection CreateConnection() {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection() //缓存
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
        var appSetting = config.GetSection("AppSetting:RabbitMQSetting");

        var factory = new ConnectionFactory
        {
            HostName = appSetting.GetSection("HostName").Value,
            UserName = appSetting.GetSection("UserName").Value,
            Password = appSetting.GetSection("Password").Value,
            Port = 5672,
            //VirtualHost= "myRabbit"
        };
        return factory.CreateConnection();
    }
}