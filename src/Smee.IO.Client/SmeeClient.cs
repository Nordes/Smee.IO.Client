using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Smee.IO.Client
{
    public class SmeeClient : ISmeeClient
    {
        private readonly Uri _eventSourceChannel;

        private CancellationToken _cancellationToken;
        private bool _started;
        private Stream _smeeStream;

        // Some documentation can be found at : 
        // https://hpbn.co/server-sent-events-sse/
        // https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format

        /// <summary>
        /// Create a new client targeting a specific EventSource
        /// </summary>
        /// <param name="eventSourceChannel">URI to the event source channel</param>
        public SmeeClient(Uri eventSourceChannel) // Add the http factory
        {
            _eventSourceChannel = eventSourceChannel;
        }

        public event EventHandler OnConnect;
        public event EventHandler OnPing;
        public event EventHandler OnDisconnect;
        public event EventHandler<Exception> OnError;
        public event EventHandler<SmeeEvent> OnMessage;

        public void Start()
        {
            Start(CancellationToken.None);
        }

        public void Start(CancellationToken cancellationToken)
        {
            if (_started)
            {
                throw new SmeeException("The client is already started.");
            }

            _cancellationToken = cancellationToken;
            _started = true;
#pragma warning disable 4014
            StartStreamingAsync(); // Fire and forget about it.
#pragma warning restore 4014
        }

        public void Stop()
        {
            if (_started)
            {
                _smeeStream.Close();
                _smeeStream.Dispose();
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
                    _smeeStream = smeeChannelResponse.GetResponseStream();

                    await ReadChannelStreamAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    OnDisconnect?.Invoke(this, EventArgs.Empty);
                    OnError?.Invoke(this, ex);
                }
            }
        }

        private async Task ReadChannelStreamAsync()
        {
            var streamReader = new StreamReader(_smeeStream, Encoding.UTF8);
            var sb = new StringBuilder();

            while (_started && !_cancellationToken.IsCancellationRequested)
            {
                // TODO: The ReadLineAsync does not get cancelled. To ensure such a behavior we need to change this code.
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
#pragma warning disable 4014
                    // Trigger and forget
                    var rawSmeeEvent = sb.ToString().Substring(0, sb.Length - 1);
                    Task.Run(() => ProcessRequest(rawSmeeEvent), _cancellationToken);
#pragma warning restore 4014
                    sb = new StringBuilder();
                }
            }
        }

        private void ProcessRequest(string rawSmeeEvent)
        {
            var eventRegEx = new Regex("^(.*event: )(.*)(,.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            rawSmeeEvent = eventRegEx.Replace(rawSmeeEvent, "$1\"$2\"$3");
            var smeeEvent = JsonConvert.DeserializeObject<SmeeEvent>("{" + rawSmeeEvent + "}");
            switch (smeeEvent.Event)
            {
                case SmeeEventType.Message:
                    OnMessage?.Invoke(this, smeeEvent);
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
