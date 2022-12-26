using System.Formats.Asn1;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Queries;

public interface IQueryHandler
{
    Task<List<PostEntity>> HandleAsync(FindAllPostQuery query);
    Task<List<PostEntity>> HandleAsync(FindPostByIdQuery query);
    Task<List<PostEntity>> HandleAsync(FindPostByAuthorQuery query);
    Task<List<PostEntity>> HandleAsync(FindPostWithCommentQuery query);
    Task<List<PostEntity>> HandleAsync(FindPostWithLikesQuery query);
}