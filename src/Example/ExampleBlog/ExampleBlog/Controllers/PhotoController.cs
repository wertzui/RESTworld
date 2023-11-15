using ExampleBlog.Business.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IPhotoService _service;

    public PhotoController(IPhotoService service)
    {
        _service = service;
    }

    [HttpGet("{email}")]
    public async Task<FileResult?> GetAsync(string email, CancellationToken cancellationToken)
    {
        var response = await _service.GetPhotoAsync(email, cancellationToken);

        if (response.Succeeded)
            return File(response.ResponseObject, "image/*");

        return File(Array.Empty<byte>(), "image/*");
    }
}
