# Use the official .NET SDK image for building and running tests
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project files
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application
COPY . ./

# Build the project
RUN dotnet build --configuration Release --no-restore

# Run tests
FROM build AS test
WORKDIR /app
ENTRYPOINT ["dotnet", "test", "--configuration", "Release", "--no-build", "--verbosity", "normal"]

# Optional: Create a runtime image for just running tests
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "test", "--configuration", "Release", "--verbosity", "normal"]