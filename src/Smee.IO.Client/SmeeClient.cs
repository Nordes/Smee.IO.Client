using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Smee.IO.Client.Exceptions;

namespace Smee.IO.Client
{
    public class SmeeClient : ISmeeClient
    {
        private readonly Uri _eventSourceChannel;

        private bool _started;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Create a new client targeting a specific EventSource
        /// </summary>
        /// <param name="eventSourceChannel">URI to the event source channel</param>
        public SmeeClient(Uri eventSourceChannel)
        {
            _eventSourceChannel = eventSourceChannel;
        }

        public event EventHandler OnConnect;
        public event EventHandler OnPing;
        public event EventHandler OnDisconnect;
        public event EventHandler<Exception> OnError;
        public event EventHandler<Dto.SmeeEvent> OnMessage;

        public Task StartAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            return StartAsync(_cancellationTokenSource.Token);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_started)
            {
                throw new SmeeException("The client is already started.");
            }

            _cancellationToken = cancellationToken;
            _started = true;
            await StartStreamingAsync(); // Fire and forget about it.
        }

        public void Stop()
        {
            if (_started)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = null;
                OnDisconnect?.Invoke(this, EventArgs.Empty);
            }

            _started = false;
            _cancellationToken = CancellationToken.None;
        }

        private async Task StartStreamingAsync()
        {
            while (_started && !_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var smeeChannelRequest = WebRequest.CreateHttp(_eventSourceChannel);
                    smeeChannelRequest.AllowReadStreamBuffering = false;
                    smeeChannelRequest.Accept = "text/event-stream";
                    var smeeChannelResponse = await smeeChannelRequest.GetResponseAsync().ConfigureAwait(false);
                    var smeeStream = smeeChannelResponse.GetResponseStream();

                    await ReadChannelStreamAsync(smeeStream).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnDisconnect?.Invoke(this, EventArgs.Empty);
                    OnError?.Invoke(this, ex);
                }
            }
        }

        private async Task ReadChannelStreamAsync(Stream smeeStream)
        {
            var streamReader = new StreamReader(smeeStream, Encoding.UTF8);
            var sb = new StringBuilder();

            _cancellationToken.Register(() => streamReader.Close());

            while (_started)
            {
                var line = await streamReader.ReadLineAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.IndexOf(":", StringComparison.Ordinal) > 0)
                    {
                        sb.Append(line + ",");
                    }
                }
                else
                {
                    var rawSmeeEvent = sb.ToString();
#pragma warning disable 4014 // Trigger and forget
                    Task.Run(() => ProcessRequest(rawSmeeEvent), _cancellationToken);
#pragma warning restore 4014
                    sb = new StringBuilder();
                }
            }
        }

        private void ProcessRequest(string rawSmeeEvent)
        {
            var eventRegEx = new Regex("^(.*event: )(.*?)(,.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            var jsonSmeeEvent = eventRegEx.Replace(rawSmeeEvent, "$1\"$2\"$3");
            var smeeEvent = JsonConvert.DeserializeObject<Models.SmeeEvent>("{" + jsonSmeeEvent + "}");

            switch (smeeEvent.Event)
            {
                case SmeeEventType.Message:
                    // Convert to Dto (Could use AutoMapper, but it would add a dependency.
                    var dtoSmeeEvent = new Dto.SmeeEvent(smeeEvent);
                    OnMessage?.Invoke(this, dtoSmeeEvent);
                    break;
                case SmeeEventType.Ping:
                    OnPing?.Invoke(this, EventArgs.Empty);
                    break;
                case SmeeEventType.Ready:
                    OnConnect?.Invoke(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
