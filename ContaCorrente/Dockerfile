FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BankMore.ContaCorrente/BankMore.ContaCorrente.csproj", "src/BankMore.ContaCorrente/"]
RUN dotnet restore "src/BankMore.ContaCorrente/BankMore.ContaCorrente.csproj"

COPY . .
WORKDIR "/src/src/BankMore.ContaCorrente"
RUN dotnet build "BankMore.ContaCorrente.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BankMore.ContaCorrente.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "BankMore.ContaCorrente.dll"]
