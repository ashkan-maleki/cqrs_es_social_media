using CQRS.Core.Queries;

namespace Post.Query.Api.Queries;

public class FindPostByAuthorQuery : BaseQuery
{
    public string? Author { get; set; }
}