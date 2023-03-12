using RESTworld.Business.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Business.Services
{
    public interface IPhotoService
    {
        Task<ServiceResponse<Stream>> GetPhotoAsync(string email, CancellationToken cancellationToken);
    }
}