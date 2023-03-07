using Confluent.Kafka;
using CQRS.Core.Consumers;
using CQRS.Core.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;
using Post.Query.Domain.Repositories;
using Post.Query.Infrastructure.Consumers;
using Post.Query.Infrastructure.DataAccess;
using Post.Query.Infrastructure.Dispatchers;
using Post.Query.Infrastructure.Handlers;
using Post.Query.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Action<DbContextOptionsBuilder> configureDbContext = 
    o => o.UseLazyLoadingProxies().UseSqlServer(
        builder.Configuration.GetConnectionString("SqlServer"));
//
// Action<DbContextOptionsBuilder> configureDbContext = 
//     o => o.UseLazyLoadingProxies().UseNpgsql(
//         builder.Configuration.GetConnectionString("Postgresql"));

builder.Services.AddDbContext<DatabaseContext>(configureDbContext);
builder.Services.AddSingleton<DatabaseContextFactory>(
    new DatabaseContextFactory(configureDbContext));


DatabaseContext dataContext = builder.Services.BuildServiceProvider()
    .GetRequiredService<DatabaseContext>();
dataContext.Database.EnsureCreated();

builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IQueryHandler, QueryHandler>();
builder.Services.AddScoped<IEventHandler, Post.Query.Infrastructure.Handlers.EventHandler>();

builder.Services.Configure<ConsumerConfig>
    (builder.Configuration.GetSection(nameof(ConsumerConfig)));
builder.Services.AddScoped<IEventConsumer, EventConsumer>();

IQueryHandler queryHandler = builder.Services
    .BuildServiceProvider()
    .GetRequiredService<IQueryHandler>();

QueryDispatcher dispatcher = new();
dispatcher.RegisterHandler<FindAllPostQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostByIdQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostByAuthorQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostWithCommentQuery>(queryHandler.HandleAsync);
dispatcher.RegisterHandler<FindPostWithLikesQuery>(queryHandler.HandleAsync);
builder.Services.AddSingleton<IQueryDispatcher<PostEntity>>(_ => dispatcher);

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddHostedService<ConsumerHostedService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// https://blog.logrocket.com/docker-sql-server/
// https://github.com/Microsoft/mssql-docker/issues/283