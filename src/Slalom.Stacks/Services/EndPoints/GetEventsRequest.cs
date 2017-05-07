using System;

namespace Slalom.Stacks.Services.EndPoints
{
    /// <summary>
    /// Message to get the events that have occurred within the service context.
    /// </summary>
    public class GetEventsRequest
    {
        /// <summary>
        /// Gets the end of the requested range.
        /// </summary>
        /// <value>The end of the requested range.</value>
        public DateTimeOffset? End { get; set; }

        /// <summary>
        /// Gets the start of the requested range.
        /// </summary>
        /// <value>The start of the requested range.</value>
        public DateTimeOffset? Start { get; set; }
    }
}