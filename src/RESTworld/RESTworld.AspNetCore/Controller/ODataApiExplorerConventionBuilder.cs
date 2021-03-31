using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using RESTworld.EntityFrameworkCore.Models;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;

namespace RESTworld.AspNetCore.Controller
{
    public class ODataApiExplorerConventionBuilder
    {
        private static ICollection<Action<ODataApiExplorerOptions>> _setupActions = new HashSet<Action<ODataApiExplorerOptions>>();

        public static void AddController(Action<ODataApiExplorerOptions> setupAction)
        {
            if (setupAction is null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            _setupActions.Add(setupAction);
        }

        public static void AddCrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>()
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
        {
            AddController(options =>
            {
                options.QueryOptions
                    .Controller<CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>>()
                        .Action(c => c.GetListAsync(default, default, default, default, default))
                        .AllowFilter(0)
                        .AllowOrderBy()
                        .AllowSkip(0)
                        .AllowTop(0);
            });
        }

        public static void GetCombinedSetupAction(ODataApiExplorerOptions options)
        {
            foreach (var action in _setupActions)
            {
                action(options);
            }
        }
    }
}