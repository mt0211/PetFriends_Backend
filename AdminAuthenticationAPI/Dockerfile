FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install required library
RUN apt-get update && apt-get install -y libkrb5-dev

WORKDIR /src
COPY AdminAuthenticationAPI.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install required library
RUN apt-get update && apt-get install -y libkrb5-dev

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "AdminAuthenticationAPI.dll"]
