using HAL.Client.Net;
using HAL.Common;
using HAL.Common.Forms;

namespace RESTworld.Client.Net;

/// <summary>
/// A client to call a RESTworld backend.
/// </summary>
public interface IRestWorldClient : IHalClient
{
    /// <summary>
    /// Gets all links from the home resource.
    /// </summary>
    /// <returns>All links from the home resource.</returns>
    IDictionary<string, ICollection<Link>> GetAllLinksFromHome();

    /// <summary>
    /// Gets all pages from a list resource. Depending on the total count and page size, this may
    /// take some time.
    /// </summary>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="curie">An optional curie. If none is given, the default curie is used.</param>
    /// <param name="uriParameters">Optional parameters to fill into the URI, like $filter or $orderby.</param>
    /// <param name="headers">
    /// Optional headers to include. If you are calling a private endpoint, you probably need to
    /// include an Authorization header here.
    /// </param>
    /// <param name="version">An optional version to include in the Accept header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All items as if they where in a single page.</returns>
    Task<HalResponse<Resource<Page>>> GetAllPagesListAsync(string rel, string? curie = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pages from a list forms resource. Depending on the total count and page size, this may
    /// take some time.
    /// </summary>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="curie">An optional curie. If none is given, the default curie is used.</param>
    /// <param name="uriParameters">Optional parameters to fill into the URI, like $filter or $orderby.</param>
    /// <param name="headers">
    /// Optional headers to include. If you are calling a private endpoint, you probably need to
    /// include an Authorization header here.
    /// </param>
    /// <param name="version">An optional version to include in the Accept header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>All items as if they where in a single page including form templates from the first page.</returns>
    Task<HalResponse<FormsResource<Page>>> GetAllPagesFormListAsync(string rel, string? curie = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first link from the home resource that matches the given parameters.
    /// </summary>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="name">
    /// An optional name to further identify the link if more than one link exists with the same <paramref name="rel"/>.
    /// </param>
    /// <param name="curie">An optional curie. If none is given, the default curie is used.</param>
    /// <returns>The first link that matches the given parameters.</returns>
    Link? GetLinkFromHome(string rel, string? name = null, string? curie = null);

    /// <summary>
    /// Gets all links from the home resource that match the given parameters.
    /// </summary>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="curie">An optional curie. If none is given, the default curie is used.</param>
    /// <returns>All link that match the given parameters.</returns>
    IEnumerable<Link> GetLinksFromHome(string rel, string? curie = null);

    /// <summary>
    /// Gets one page from the list resource. If no <paramref name="uriParameters"/> for $skip are
    /// given then this will be the first page.
    /// </summary>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="curie">An optional curie. If none is given, the default curie is used.</param>
    /// <param name="uriParameters">
    /// Optional parameters to fill into the URI, like $filter, $orderby, $skip and $take.
    /// </param>
    /// <param name="headers">
    /// Optional headers to include. If you are calling a private endpoint, you probably need to
    /// include an Authorization header here.
    /// </param>
    /// <param name="version">An optional version to include in the Accept header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The specified page from the list resource.</returns>
    Task<HalResponse<Resource<Page>>> GetListAsync(string rel, string? curie = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets one page from the list form resource. If no <paramref name="uriParameters"/> for $skip are
    /// given then this will be the first page.
    /// </summary>
    /// <param name="rel">The relation of the link.</param>
    /// <param name="curie">An optional curie. If none is given, the default curie is used.</param>
    /// <param name="uriParameters">
    /// Optional parameters to fill into the URI, like $filter, $orderby, $skip and $take.
    /// </param>
    /// <param name="headers">
    /// Optional headers to include. If you are calling a private endpoint, you probably need to
    /// include an Authorization header here.
    /// </param>
    /// <param name="version">An optional version to include in the Accept header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The specified page from the list resource including the form templates.</returns>
    Task<HalResponse<FormsResource<Page>>> GetFormListAsync(string rel, string? curie = null, IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null, string? version = null, CancellationToken cancellationToken = default);
}