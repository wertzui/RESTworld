﻿using System.ComponentModel.DataAnnotations;

namespace RESTworld.Common.Dtos;

/// <summary>
/// A base class for all DTOs which have once been stored in the database.
/// </summary>
public class DtoBase
{
    /// <summary>
    /// Gets or sets the ID.
    /// </summary>
    [Display(Name = "ID", Order = -1000)]
    public virtual long Id { get; set; }
}