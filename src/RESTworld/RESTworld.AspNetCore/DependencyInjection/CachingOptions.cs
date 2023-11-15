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
        Add(nameof(Get), TimeSpan.FromSeconds(30));
        Add(nameof(GetList), TimeSpan.FromSeconds(30));
    }

    /// <summary>
    /// The cache time for the get endpoint.
    /// </summary>
    public TimeSpan Get => this[nameof(Get)];

    /// <summary>
    /// The cache time for the get list endpoint.
    /// </summary>
    public TimeSpan GetList => this[nameof(GetList)];
}