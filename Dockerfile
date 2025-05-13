﻿# Base dotnet image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Add curl to template.
# CDP PLATFORM HEALTHCHECK REQUIREMENT
RUN apt update && \
    apt install curl -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Build stage image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY src/Stub/Stub.csproj src/Stub/Stub.csproj
COPY Defra.TradeImportsDataApiStub.sln Defra.TradeImportsDataApiStub.sln

RUN dotnet restore Defra.TradeImportsDataApiStub.sln

COPY src src

FROM build AS publish
RUN dotnet publish src/Stub -c Release -o /app/publish /p:UseAppHost=false

ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8085
ENTRYPOINT ["dotnet", "Defra.TradeImportsDataApiStub.Stub.dll"]
