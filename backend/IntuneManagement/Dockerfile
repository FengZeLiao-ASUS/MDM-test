FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IntuneManagement.csproj", "./"]
RUN dotnet restore "IntuneManagement.csproj"
COPY . .
RUN dotnet build "IntuneManagement.csproj" -c Release -o /app/build
RUN dotnet publish "IntuneManagement.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IntuneManagement.dll"]
