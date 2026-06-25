# ===============================
# BUILD
# ===============================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar todo el proyecto
COPY . .

# Restaurar dependencias
RUN dotnet restore "BancaNet.slnx"

# Publicar la API
WORKDIR "/src/BancaNet.Api"
RUN dotnet publish "BancaNet.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===============================
# RUNTIME
# ===============================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Librerías necesarias (PostgreSQL / seguridad)
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*

# Copiar la app publicada
COPY --from=build /app/publish .

# Render usa el puerto dinámico
ENV ASPNETCORE_URLS=http://+:${PORT}

# Exponer puerto (referencial)
EXPOSE 8080

# Ejecutar la API
ENTRYPOINT ["dotnet", "BancaNet.Api.dll"]