using Orleans.Streams;

namespace WebApplication1;

[ImplicitStreamSubscription("InputStream")]
public class ListenerGrain(ILogger<ListenerGrain> log) : Grain, IGrainWithIntegerKey, IAsyncObserver<string>
{
    IAsyncStream<string> outputStream = null!;
    
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        var streamProvider = this.GetStreamProvider("MemoryStream");
        var inputStream = streamProvider.GetStream<string>("InputStream", this.GetPrimaryKeyLong());
        outputStream = streamProvider.GetStream<string>("OutputStream", this.GetPrimaryKeyLong());
        
        await inputStream.SubscribeAsync(this);
    }

    public async Task OnNextAsync(string item, StreamSequenceToken? token = null)
    {
        log.LogInformation("Received: {Item}", item);
        await outputStream.OnNextAsync(item);
    }

    public Task OnCompletedAsync() => Task.CompletedTask;

    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;
}
