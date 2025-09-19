using System;
using System.Collections.Generic;

namespace RESTworld.AspNetCore.DependencyInjection;

/// <summary>
/// Options for caching of service results inside of controllers.
/// </summary>
public class CachingOptions : Dictionary<string, TimeSpan>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CachingOptions"/> class.
    /// </summary>
    public CachingOptions()
    {
        Add(nameof(Get), TimeSpan.FromSeconds(10));
        Add(nameof(GetList), TimeSpan.FromSeconds(10));
        Add(nameof(GetHistory), TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// The cache time for a get operation.
    /// </summary>
    public TimeSpan Get => this[nameof(Get)];

    /// <summary>
    /// The cache time for a get list operation.
    /// </summary>
    public TimeSpan GetList => this[nameof(GetList)];

    /// <summary>
    /// The cache time for a get history operation.
    /// </summary>
    public TimeSpan GetHistory => this[nameof(GetHistory)];
}