﻿namespace Slalom.Stacks.Services.Inventory
{
    /// <summary>
    /// An available endpoint in a remote service.
    /// </summary>
    public class RemoteEndPoint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteEndPoint" /> class.
        /// </summary>
        /// <param name="path">The relative path the endpoint.</param>
        /// <param name="fullPath">The full path the endpoint.</param>
        public RemoteEndPoint(string path, string fullPath)
        {
            this.Path = path;
            this.FullPath = fullPath;
        }

        /// <summary>
        /// Gets the full path or address of the endpoint.
        /// </summary>
        /// <value>The full path the endpoint.</value>
        public string FullPath { get; internal set; }

        /// <summary>
        /// Gets the relative path the endpoint.  Used to identify the endpoint.
        /// </summary>
        /// <value>The relative path the endpoint.</value>
        public string Path { get; }
    }
}