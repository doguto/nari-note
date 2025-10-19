FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    build-essential \
    git \
    vim && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

RUN mkdir -p /app
WORKDIR /app

EXPOSE 5243
