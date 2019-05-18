![License](https://img.shields.io/github/license/Nordes/Smee.IO.Client.svg)
[![Build status](https://ci.appveyor.com/api/projects/status/m63s6b2188b20bkc/branch/master?svg=true)](https://ci.appveyor.com/project/Nordes/Smee.IO.Client/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Smee.IO.Client.svg)](https://www.nuget.org/packages/Smee.IO.Client/)

# Smee.IO.Client
[Smee.IO](https://smee.io/) Client library implementation for Dotnet. 

> A Nuget package will soon be released.

More details on the official project can be found at https://github.com/probot/smee.io/. Please note that I am not a maintainer of [Smee.IO](https://smee.io).

## How to use the client
It is really simple, you simply have to connect to the Smee URL and then attach your listeners. Since [Smee.IO](https://smee.io) uses EventSources sent through HTTP Streaming, the site or server, currently push the notifications. This work in a similar way as the WebSocket except without a TCP socket.

### Code Sample 

#### Web Application
Within a Web Application, I recommend you to use the `BackgroundService` Host Service in order to receive the requests. This will create a service running beind the scene and then it will trigger action within your application. The code will be shortened since it will duplicate the console code.

```csharp
public class WatchMySmee : BackgroundService
{
    private readonly ILogger<WatchMySmee> _logger;

    public WatchMySmee(ILogger<WatchMySmee> logger) {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        _logger.LogWarning("Start my smee");
        var smeeCli = new SmeeClient(new Uri("https://smee.io/bbD7RyIMjQ2LCV9"));

        smeeCli.OnConnect += (sender, args) => _logger.LogInformation("Connected to SMEE.io");
        smeeCli.OnMessage += (sender, smeeEvent) => {
            _logger.LogInformation($"Message: {JsonConvert.SerializeObject(smeeEvent)}")
        };
        //////////////////////////////////////////
        // See Console application for other events
        //////////////////////////////////////////
        smeeCli.Start(stoppingToken); // Token is optional here
        await Task.Delay(-1, stoppingToken);
    }
    
    // etc.
}
```

#### Console Application
```csharp
var smeeUri = new Uri("https://smee.io/bbD7RyIMjQ2LCV9"); // Random URI
var smeeCli = new SmeeClient(smeeUri);

// Register to the existing events, you should pick only what you require.
smeeCli.OnConnect += (sender, a) => Console.WriteLine($"Connected to Smee.io ({smeeUri})");
smeeCli.OnDisconnect += (sender, a) => Console.WriteLine($"Disconnected from Smee.io ({smeeUri})");
smeeCli.OnMessage += (sender, smeeEvent) =>
{
    Console.Write("Message received: ");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write(JsonConvert.SerializeObject(smeeEvent)); // This is a typed object.
    Console.ResetColor();
    Console.WriteLine();
};
smeeCli.OnPing += (sender, a) => Console.WriteLine("Ping from Smee");
smeeCli.OnError += (sender, e) => Console.WriteLine("Error was raised (Disconnect/Anything else: " + e.Message);
smeeCli.Start(token); // Token is optional here
```

## Events
Currently, here are the existing events:

- OnMessage
- OnError (Usually happens at the same time as a disconnection)
- OnPing
- OnConnect
- OnDisconnect (Usually happens at the same time as an error)

## Demo Project
I wrote a small demo application (console) in order to show that it actually works. Not all the possible cases have been tested and some errors might rise. In such case, please simply create an issue.

The demo project is included in this repository, simply navigate to the src folder or open directly the solution.

## Contributor(s)
> None at the moment.

## License
MIT