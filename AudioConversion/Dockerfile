FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["AudioConversion.csproj", ""]
RUN dotnet restore "./AudioConversion.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AudioConversion.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AudioConversion.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AudioConversions.dll"]

RUN apt-get update && apt-get install -y mpg123 && apt-get install -y sox && apt-get install -y lame && apt-get install -y ffmpeg
