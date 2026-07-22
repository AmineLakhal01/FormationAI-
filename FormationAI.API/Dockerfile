# --- Étape build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY FormationAI.API.csproj ./
RUN dotnet restore
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# --- Étape runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
# Railway injecte la variable PORT ; l'app écoute sur 0.0.0.0:$PORT
EXPOSE 8080
ENTRYPOINT ["dotnet", "FormationAI.API.dll"]
