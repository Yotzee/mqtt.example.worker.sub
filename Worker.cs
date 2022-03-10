namespace MQTT.SUB;

using System;
using MQTTnet;
using MQTTnet.Client;
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttFactory();
        var mqttClient = mqttFactory.CreateMqttClient();
        var mqttConnectionString = Environment.GetEnvironmentVariable("MQTTHost") ?? "vernmq";
        await mqttClient.ConnectAsync(new MqttClientOptionsBuilder()
                                .WithTcpServer(mqttConnectionString)
                                .WithNoKeepAlive()
                                .Build(),
                                CancellationToken.None
                        );
        if (!mqttClient.IsConnected)
        {
            _logger.LogError("Not Connected To MQTT");
            stoppingToken.ThrowIfCancellationRequested();
            return;
        }

        await mqttClient.SubscribeAsync(new MqttClientSubscribeOptionsBuilder()
                                        .WithTopicFilter(e => e.WithTopic("test"))
                                        .Build(),
                                        stoppingToken
                                        );

        mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            _logger.LogInformation($"Received application message. from: {e.ClientId} {e.ApplicationMessage.ConvertPayloadToString()}");

            return Task.CompletedTask;
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(30000, stoppingToken);
        }
    }
}
