FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY . .

#TODO: get rid of private nuget repo
COPY "./nuget" "/nuget"

RUN dotnet build "./NtFreX.Blog/NtFreX.Blog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./NtFreX.Blog/NtFreX.Blog.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NtFreX.Blog.dll"]
