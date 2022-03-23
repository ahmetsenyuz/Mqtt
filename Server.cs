
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;

MqttFactory mqttFactory = new MqttFactory();
IMqttClient client = mqttFactory.CreateMqttClient();
var options = new MqttClientOptionsBuilder()
    .WithClientId(Guid.NewGuid().ToString())
    .WithTcpServer("test.mosquitto.org", 1883)
    .WithCleanSession()
    .Build();


client.UseConnectedHandler(x =>
{
    Console.WriteLine("Servera Bağlanıldı");
    var topicFilter = new TopicFilterBuilder().WithTopic("NODEServer").Build();
    client.SubscribeAsync(topicFilter);
});

client.UseDisconnectedHandler(x =>
{
    Console.WriteLine("Bağlantı Sonlandırıldı");
});
client.UseApplicationMessageReceivedHandler(x =>
{
    Console.WriteLine($"Gelen Mesaj: {Encoding.UTF8.GetString(x.ApplicationMessage.Payload)}");
});

await client.ConnectAsync(options);

Console.WriteLine("Çıkış yapmak için bir boş mesaj gönderin");
bool devam = true;
while (devam)
{
    await PublishMessageAsync(client,devam);
}

await client.DisconnectAsync();

async Task PublishMessageAsync(IMqttClient client, bool b)
{
    string messagePayload = Console.ReadLine();
    if (messagePayload == "")
    {
        devam = false;
    }
    else
    {
        var message = new MqttApplicationMessageBuilder().WithTopic("NODEClient").WithPayload(messagePayload)
            .WithAtLeastOnceQoS().Build();
        if (client.IsConnected)
        {
            await client.PublishAsync(message);
        }
    }
    
}