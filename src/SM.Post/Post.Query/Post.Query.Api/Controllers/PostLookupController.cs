using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Common.DTOs;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Controllers;

[ApiController]
[Route("api/v1/post-lookup")]
public class PostLookupController : ControllerBase
{
   private readonly ILogger<PostLookupController> _logger;
   private readonly IQueryDispatcher<PostEntity> _queryDispatcher;

   public PostLookupController(ILogger<PostLookupController> logger,
      IQueryDispatcher<PostEntity> queryDispatcher)
   {
      _logger = logger;
      _queryDispatcher = queryDispatcher;
   }

   [HttpGet]
   public async Task<ActionResult> GetAllPostAsync()
   {
      try
      {
         List<PostEntity>? posts = await _queryDispatcher
            .SendAsync(new FindAllPostQuery());

         return NormalResponse(posts);
      }
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve all posts!";
         return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
      }
   }
   
   [HttpGet("{postId}")]
   public async Task<ActionResult> GetByPostIdAsync(Guid postId)
   {
      try
      {
         List<PostEntity>? posts = await _queryDispatcher
            .SendAsync(new FindPostByIdQuery {Id = postId});

         if (posts == null || !posts.Any())
         {
            return NoContent();
         }

         // int count = posts.Count;
         return Ok(new PostLookupResponse
         {
            Posts = posts,
            Message = "Successfully returned post"
         });
      }
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve post by ID!";
         return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
      }
   }
   
   [HttpGet("Author/{author}")]
   public async Task<ActionResult> GetByAuthorAsync(string author)
   {
      try
      {
         List<PostEntity>? posts = await _queryDispatcher
            .SendAsync(new FindPostByAuthorQuery {Author = author});

         return NormalResponse(posts);
      }
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve posts by Author!";
         return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
      }
   }
   
   [HttpGet("with-comments")]
   public async Task<ActionResult> GetPostsWithCommentsAsync()
   {
      try
      {
         List<PostEntity>? posts = await _queryDispatcher
            .SendAsync(new FindPostWithCommentQuery());

         return NormalResponse(posts);
      }
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve posts with comments!";
         return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
      }
   }

   [HttpGet("with-likes/{numberOfLikes}")]
   public async Task<ActionResult> GetPostsWithLikesAsync(int numberOfLikes)
   {
      try
      {
         List<PostEntity>? posts = await _queryDispatcher
            .SendAsync(new FindPostWithLikesQuery {NumberOfLikes = numberOfLikes});

         return NormalResponse(posts);
      }
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve posts with likes!";
         return ErrorResponse(ex, SAFE_ERROR_MESSAGE);
      }
   }

   private ActionResult ErrorResponse(Exception ex, string safeErrorMessage)
   {
      
      _logger.LogError(ex, safeErrorMessage);

      return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
      {
         Message = safeErrorMessage
      });
   }

   private ActionResult NormalResponse(List<PostEntity> posts)
   {
      if (posts == null || !posts.Any())
      {
         return NoContent();
      }

      int count = posts.Count;
      return Ok(new PostLookupResponse
      {
         Posts = posts,
         Message = $"Successfully returned {count} post{(count > 1 ? "s" : string.Empty)}!"
      });
   }
}