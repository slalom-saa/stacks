﻿using System;
using System.Linq;

namespace Slalom.Stacks.Services.Registry
{
    /// <summary>
    /// Indicates the path the endPoint.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class EndPointAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EndPointAttribute"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public EndPointAttribute(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>The name.</value>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the endpoint timeout in milliseconds.
        /// </summary>
        /// <value>The endpoint timeout in milliseconds.</value>
        public double Timeout { get; set; }

        /// <summary>
        /// Gets or sets the version number.
        /// </summary>
        /// <value>The version number.</value>
        public int Version { get; set; } = 1;
    }
}