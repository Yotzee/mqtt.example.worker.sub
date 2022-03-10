FROM mcr.microsoft.com/dotnet/runtime:6.0-focal AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /src
COPY ["MQTT.SUB.csproj", "./"]
RUN dotnet restore "MQTT.SUB.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "MQTT.SUB.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MQTT.SUB.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
ENV MQTTHost vernemq
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MQTT.SUB.dll"]
