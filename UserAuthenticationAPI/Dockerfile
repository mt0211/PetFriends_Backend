FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install required library
RUN apt-get update && apt-get install -y libkrb5-dev

WORKDIR /src
COPY UserAuthenticationAPI.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install required library
RUN apt-get update && apt-get install -y libkrb5-dev

WORKDIR /app
COPY --from=build /app .

RUN mkdir -p /app/cert
COPY cert/certificate.pfx /app/cert/

ENTRYPOINT ["dotnet", "UserAuthenticationAPI.dll"]
