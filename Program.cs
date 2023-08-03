// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

// using IHost host = Host.CreateDefaultBuilder(args).Build();
//string dynamicconnectionString = "REMOVED_REPLACED_WITH_KEYVAULT";
// CHANGE THESE TO MATCH YOUR KEYVAULT
const string KEY_VAULT = "Rick-KeyVault";
const string SECRET_NAME = "connection-ServiceBus";
var kvURI = "https://" + KEY_VAULT + ".vault.azure.net";
var clientKeyV = new SecretClient(new Uri(kvURI), new DefaultAzureCredential());

// set the key value dynamically
//await client.SetSecretAsync(SECRET_NAME, dynamicconnectionString );
// connection string to your Service Bus namespace  
Console.WriteLine($"Retrieving your secret from {KEY_VAULT}.");
var connectionString = await clientKeyV.GetSecretAsync(SECRET_NAME);

//Console.WriteLine($"setting up local queue connection");

// name of your Service Bus topic
string queueName = "ridedata  ";
//
// the client that owns the connection and can be used to create senders and receivers
ServiceBusClient client;

// the sender used to publish messages to the queue
ServiceBusSender sender;

// Create the clients that we'll use for sending and processing messages.
client = new ServiceBusClient(connectionString.Value.Value);

//client = new Azure.Messaging.ServiceBus.client QueueClient(new Uri("https://127.0.0.1:10001/devstoreaccount1/queue-name"), new DefaultAzureCredential());
//var storageconnectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
// string queueconnectionString = ConfigurationManager.AppSettings["StorageConnectionString"];
//string queueconnectionString = "UseDevelopmentStorage=true";

////QueueClient queueClient = new QueueClient(queueconnectionString, queueName);

// Create the queue
//queueClient.CreateIfNotExists();
sender = client.CreateSender(queueName);

// create a batch 
using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

//var id = new Guid();
string guid = Guid.NewGuid().ToString();
string messageToSend;

for (int i = 1; i <= 50; i++)
{
    // set message
    messageToSend = $"Message {i} RIDE DATA for " + Guid.NewGuid().ToString();
    Console.WriteLine($"Adding message {messageToSend}");

    // try adding a message to the batch
    if (!messageBatch.TryAddMessage(new ServiceBusMessage(messageToSend)))
    {
        // if an exception occurs
        throw new Exception($"Exception {i} has occurred.");
    }
}
/*
if (queueClient.Exists())
{
    QueueProperties properties = queueClient.GetProperties();

    // Retrieve the cached approximate message count.
    int cachedMessagesCount = properties.ApproximateMessagesCount;

    // Display number of messages.
    Console.WriteLine($"Number of messages in queue: {cachedMessagesCount}");
}
*/
try
{
    // Use the producer client to send the batch of messages to the Service Bus queue
    await sender.SendMessagesAsync(messageBatch);
    Console.WriteLine($"A batch of  messages has been published to the queue.");
}
finally
{
    // Calling DisposeAsync on client types is required to ensure that network
    // resources and other unmanaged objects are properly cleaned up.
    await sender.DisposeAsync();
    await client.DisposeAsync();
}

Console.WriteLine("Have a look at your new messages in the Azure portal.");
Console.WriteLine("Press any key to continue");
Console.ReadKey();