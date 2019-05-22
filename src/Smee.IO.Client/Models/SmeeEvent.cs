using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Smee.IO.Client.Models
{
  /// <summary>
  /// Detailed in
  /// https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format
  /// </summary>
  internal class SmeeEvent
    {
        /// <summary>
        /// The event ID to set the EventSource object's last event ID value.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The reconnection time to use when attempting to send the event. This must be an
        /// integer, specifying the reconnection time in milliseconds. If a non-integer value
        /// is specified, the field is ignored.
        /// </summary>
        public long Retry { get; set; }

        /// <summary>
        /// A string identifying the type of event described.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public SmeeEventType Event { get; set; } = SmeeEventType.Message;

        /// <summary>
        /// Make the data as SMEE format. Since it is a web request which is transferred, it will send many http info.
        /// In this case, we are deserializing the object as a well-known object.
        /// </summary>
        /// <remarks>
        /// ORIGINAL
        /// The data field for the message. When the EventSource receives multiple consecutive lines that begin with
        /// data:, it will concatenate them, inserting a newline character between each one. Trailing newlines are
        /// removed.
        /// </remarks>
        public SmeeData Data { get; set; }
    }
}
