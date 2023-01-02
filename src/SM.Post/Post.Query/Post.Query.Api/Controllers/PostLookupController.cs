﻿using CQRS.Core.Infrastructure;
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
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve all posts!";
         _logger.LogError(ex, SAFE_ERROR_MESSAGE);

         return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
         {
            Message = SAFE_ERROR_MESSAGE
         });
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

         int count = posts.Count;
         return Ok(new PostLookupResponse
         {
            Posts = posts,
            Message = "Successfully returned post"
         });
      }
      catch (Exception ex)
      {
         const string SAFE_ERROR_MESSAGE = "Error while processing request to retrieve post by ID!";
         _logger.LogError(ex, SAFE_ERROR_MESSAGE);

         return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
         {
            Message = SAFE_ERROR_MESSAGE
         });
      }
   }
}