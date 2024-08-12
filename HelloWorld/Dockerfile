FROM mcr.microsoft.com/dotnet/sdk:8.0 as build

WORKDIR /app

COPY HelloWorld.csproj .

RUN dotnet restore "HelloWorld.csproj"

COPY . .

RUN dotnet publish "HelloWorld.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

WORKDIR /app

COPY --from=build /app/publish .

CMD ["dotnet", "HelloWorld.dll"]