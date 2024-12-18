FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install required library and remove cache
RUN apt-get update && apt-get install -y libkrb5-dev && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy project files for restore
COPY PetManagementAPI.csproj ./
RUN dotnet restore

# Copy entire source code and build
COPY . . 
RUN dotnet publish -c release -o /app

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "PetManagementAPI.dll"]