using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Post.Command.Api.Controllers;

[ApiController]
[Route("api/v1/add-comment")]
public class AddCommentController : ControllerBase
{
    private readonly ILogger<LikePostController> _logger;
    private readonly ICommandDispatcher _commandDispatcher;
    
    public AddCommentController(ILogger<LikePostController> logger, ICommandDispatcher commandDispatcher)
    {
        _logger = logger;
        _commandDispatcher = commandDispatcher;
    }
}