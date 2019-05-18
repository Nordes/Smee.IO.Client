using System.Threading;

namespace Smee.IO.Client
{
    public interface ISmeeClient
    {
        void Start();
        void Start (CancellationToken cancellationToken);
        void Stop();
    }
}