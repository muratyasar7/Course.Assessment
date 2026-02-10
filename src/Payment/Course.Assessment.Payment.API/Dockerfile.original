# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Course.Assessment.Payment.API/Course.Assessment.Payment.API.csproj", "Course.Assessment.Payment.API/"]
COPY ["Course.Assessment.Payment.Application/Course.Assessment.Payment.Application.csproj", "Course.Assessment.Payment.Application/"]
COPY ["Course.Assessment.Payment.Domain/Course.Assessment.Payment.Domain.csproj", "Course.Assessment.Payment.Domain/"]
COPY ["Shared.Contracts/Shared.Contracts.csproj", "Shared.Contracts/"]
COPY ["Course.Assessment.Payment.Infrastructure/Course.Assessment.Payment.Infrastructure.csproj", "Course.Assessment.Payment.Infrastructure/"]
COPY ["Course.Assessment.ServiceDefaults/Course.Assessment.ServiceDefaults.csproj", "Course.Assessment.ServiceDefaults/"]
RUN dotnet restore "./Course.Assessment.Payment.API/Course.Assessment.Payment.API.csproj"
COPY . .
WORKDIR "/src/Course.Assessment.Payment.API"
RUN dotnet build "./Course.Assessment.Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Course.Assessment.Payment.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Course.Assessment.Payment.API.dll"]