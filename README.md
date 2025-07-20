# This [![Build and Deploy](https://github.com/Myxelium/HomeScreen/actions/workflows/build.yml/badge.svg)](https://github.com/Myxelium/HomeScreen/actions/workflows/build.yml)
Core api and Esp32 code for displaying weather data and public transport information on a e-ink display.

<img width="800" height="480" alt="image" src="https://github.com/user-attachments/assets/ef5af0c6-ea3a-494d-b2af-3de6e70b3e6a" />

## Features
- Display current weather data
- Display public transport information
- Display time and date

## Requirements
- ESP32 board
- E-ink display (e.g. Waveshare 7.5 inch)

# Installation

This section provides instructions for setting up and running the HomeApi project.

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- Docker (optional, for containerized deployment)
- Git (to clone the repository)

## Option 1: Local Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/Myxelium/HomeScreen.git
   cd HomeApi
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5000`.

## Option 2: Docker Deployment

1. Build the Docker image:
   ```bash
   docker build -t homeapi .
   ```

2. Run the container:
   ```bash
   docker run -d -p 5000 --name homeapi homeapi
   ```

The API will be accessible at `http://localhost:5000`.

## Configuration

The application uses the standard .NET configuration system. You can modify settings in:

- `appsettings.json` - Default configuration
- `appsettings.Development.json` - Development environment configuration

API endpoints:
- Weather data: GET `/home`
- Generated image: GET `/home/default.jpg`
- Configuration data: GET `/home/configuration`
- Departure board: GET `/home/departure-board`

## API Documentation

When running, API documentation is available through Scalar at `/scalar`.

flowchart TD
subgraph ESP32 Device
ESP[ESP32 E-Ink Display]
ESP -->|HTTP GET /home/configuration| API
ESP -->|HTTP GET /home/default.jpg| API
end

    subgraph HomeApi
        API[HomeController (API)]
        API -->|MediatR| Handlers
        Handlers -->|Service Calls| Services
        Services -->|Refit Clients| Clients
        Clients -->|External APIs| ExtAPIs
        API -->|Returns JSON/JPEG| ESP
    end

    subgraph ExternalAPIs
        WeatherAPI[Weather API]
        AuroraAPI[Aurora API]
        NominatimAPI[Nominatim API]
        ResRobotAPI[ResRobot API]
    end

    ExtAPIs -.-> WeatherAPI
    ExtAPIs -.-> AuroraAPI
    ExtAPIs -.-> NominatimAPI
    ExtAPIs -.-> ResRobotAPI