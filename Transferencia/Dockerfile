FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BankMore.Transferencia/BankMore.Transferencia.csproj", "src/BankMore.Transferencia/"]
RUN dotnet restore "src/BankMore.Transferencia/BankMore.Transferencia.csproj"

COPY . .
WORKDIR "/src/src/BankMore.Transferencia"
RUN dotnet build "BankMore.Transferencia.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.Transferencia.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BankMore.Transferencia.dll"]
