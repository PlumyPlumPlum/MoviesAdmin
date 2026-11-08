FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Restore project dependencies
COPY ["MoviesAdmin/MoviesAdmin.csproj", "MoviesAdmin/"]
RUN dotnet restore "MoviesAdmin/MoviesAdmin.csproj"

# Copy source and publish
COPY . .
WORKDIR "/src/MoviesAdmin"

RUN dotnet publish "MoviesAdmin.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# Run with the smaller ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MoviesAdmin.dll"]
