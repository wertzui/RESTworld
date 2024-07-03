using ExampleBlog.Common.Dtos;
using ExampleBlog.Data.Models;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.Forms.Customization;
using HAL.AspNetCore.Utils;
using HAL.Common.Forms;
using RESTworld.AspNetCore.Controller;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExampleBlog.HalFormsCustomizations
{
    public class AuthorForPostCustomization : IFormsResourceGenerationCustomization
    {
        private readonly ILinkFactory _linkFactory;

        public AuthorForPostCustomization(ILinkFactory linkFactory)
        {
            _linkFactory = linkFactory ?? throw new System.ArgumentNullException(nameof(linkFactory));
        }
        public bool Exclusive => false;

        public int Order => 1;

        public bool AppliesTo<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues)
            => value is PostWithAuthorDto;

        public async ValueTask ApplyAsync<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues, IFormFactory formFactory)
        {
            if (value is PostWithAuthorDto dto)
            {
                // When getting a form, instead of a link to the author, we just add another form with the author already filled in
                var authorLink = _linkFactory.Create(ActionHelper.StripAsyncSuffix(nameof(ReadController<Author, AuthorDto, AuthorDto>.GetAsync)), RestControllerNameConventionAttribute.CreateNameFromType<AuthorDto>(), new { id = dto.AuthorId }).Href ?? "";
                var authorForm = await formFactory.CreateFormAsync(dto.Author, authorLink, HttpMethod.Get, "Author");
                formsResource.Templates["The author"] = authorForm;
            }
        }
    }
}
