﻿using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common.Forms;
using RESTworld.AspNetCore.Controller;
using System;
using System.Linq;

namespace RESTworld.AspNetCore.Forms;

/// <summary>
/// Provides links which point to the GetList endpoint of the given <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
/// This is used to generate the URIs in HAL-Forms options elements.
/// </summary>
/// <typeparam name="TListDto">The type of the list endpoint to which the foreign key may point.</typeparam>
public class CrudForeignKeyLinkFactory<TListDto> : IForeignKeyLinkFactory
{
    private static readonly Type _type = typeof(TListDto);
    private const string getListAction = "GetList";
    private readonly ILinkFactory _linkFactory;
    private static readonly string controller = RestControllerNameConventionAttribute.CreateNameFromType<TListDto>();

    /// <summary>
    /// Creates a new instance of the <see cref="CrudForeignKeyLinkFactory{TListDto}"/> class.
    /// </summary>
    /// <param name="linkFactory">The Link factory.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CrudForeignKeyLinkFactory(ILinkFactory linkFactory)
    {
        _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
    }

    /// <inheritdoc/>
    public bool CanCreateLink(Type listDtoType) => listDtoType == _type;

    /// <inheritdoc/>
    public OptionsLink? CreateLink(Type listDtoType)
    {
        var halLinks = _linkFactory.CreateTemplated(getListAction, controller);
        var halLink = halLinks.FirstOrDefault();
        if (halLink is null)
            return null;

        var optionsLink = new OptionsLink(halLink.Href)
        {
            Templated = halLink.Templated,
            Type = "application/hal+json"
        };

        return optionsLink;
    }
}
