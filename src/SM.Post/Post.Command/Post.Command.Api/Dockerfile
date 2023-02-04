#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SM.Post/Post.Command/Post.Command.Api/Post.Command.Api.csproj", "SM.Post/Post.Command/Post.Command.Api/"]
COPY ["CQRS.ES/CQRS.Core/CQRS.Core.csproj", "CQRS.ES/CQRS.Core/"]
COPY ["SM.Post/Post.Command/Post.Command.Infrastructure/Post.Command.Infrastructure.csproj", "SM.Post/Post.Command/Post.Command.Infrastructure/"]
COPY ["SM.Post/Post.Command/Post.Command.Domain/Post.Command.Domain.csproj", "SM.Post/Post.Command/Post.Command.Domain/"]
COPY ["SM.Post/Post.Common/Post.Common.csproj", "SM.Post/Post.Common/"]
RUN dotnet restore "SM.Post/Post.Command/Post.Command.Api/Post.Command.Api.csproj"
COPY . .
WORKDIR "/src/SM.Post/Post.Command/Post.Command.Api"
RUN dotnet build "Post.Command.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Post.Command.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Post.Command.Api.dll"]
