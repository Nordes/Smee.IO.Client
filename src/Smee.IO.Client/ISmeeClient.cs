using System;
using System.Threading;
using System.Threading.Tasks;

namespace Smee.IO.Client
{
    public interface ISmeeClient
    {
        event EventHandler OnConnect;
        event EventHandler OnPing;
        event EventHandler OnDisconnect;
        event EventHandler<Exception> OnError;
        event EventHandler<Dto.SmeeEvent> OnMessage;

        Task StartAsync();
        Task StartAsync(CancellationToken cancellationToken);
        void Stop();
    }
}