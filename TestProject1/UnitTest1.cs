using Orleans.Streams;
using Orleans.TestingHost;

namespace TestProject1;

public class Tests
{
    async Task<TestCluster> SetUp()
    {
        var builder = new TestClusterBuilder()
            .AddSiloBuilderConfigurator<SiloConfigurator>();
        var cluster = builder.Build();
        await cluster.DeployAsync();
        return cluster;
    }
    
    async Task TearDown(TestCluster cluster)
    {
        await cluster.StopAllSilosAsync();
    }

    [Test]
    public async Task Test()
    {
        var cluster = await SetUp();
        
        // This will throw exception
        // System.Collections.Generic.KeyNotFoundException : Stream provider 'MemoryStream' not found
        var streamProvider = cluster.Client.GetStreamProvider("MemoryStream");
        
        var inputStream = streamProvider.GetStream<string>("InputStream", 0);
        var outputStream = streamProvider.GetStream<string>("OutputStream", 0);
        var mockReader = new MockReader<string>();
        await outputStream.SubscribeAsync(mockReader);
        var items = new[] { "Hello", "World" };
        
        foreach (var item in items)
        {
            // Act
            await inputStream.OnNextAsync(item);
        }

        await Task.Delay(1000);
        
        // Assert
        Assert.That(mockReader.ReceivedItems.ToArray(), Is.EqualTo(items));
        await TearDown(cluster);
    }
}

file class SiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder hostBuilder)
    {
        hostBuilder.AddMemoryGrainStorage("PubSubStore");
        // We add memory streams here
        hostBuilder.AddMemoryStreams("MemoryStream");
    }
}

file class MockReader<T> : IAsyncObserver<T>
{
    public List<T> ReceivedItems { get; } = new();

    public Task OnNextAsync(T item, StreamSequenceToken? token = null)
    {
        ReceivedItems.Add(item);
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }
}