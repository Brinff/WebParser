using Microsoft.AspNetCore.Mvc;
using WebParser.Models;
using WebParser.Services;

namespace WebParser.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebParserController : ControllerBase
{
    private readonly Service _service;

    public WebParserController(Service service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ResponseModel>> Post(RequestModel request)
    {
        var response = await _service.ProcessAsync(request);

        return Ok(response);
    }
}