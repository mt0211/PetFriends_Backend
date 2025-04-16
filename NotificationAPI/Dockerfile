FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Install required library
RUN apt-get update && apt-get install -y libkrb5-dev

WORKDIR /src
COPY NotificationAPI.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Install required library
RUN apt-get update && apt-get install -y libkrb5-dev

WORKDIR /app
COPY --from=build /app .

# Expose port
EXPOSE 80

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "NotificationAPI.dll"]
