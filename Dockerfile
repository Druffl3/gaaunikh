FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend
COPY src/frontend/package*.json ./
COPY src/frontend/.npmrc ./
RUN npm ci
COPY src/frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS backend-build
WORKDIR /src
COPY src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj src/backend/Gaaunikh.Api/
RUN dotnet restore src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj
COPY src/backend/Gaaunikh.Api/ src/backend/Gaaunikh.Api/
RUN dotnet publish src/backend/Gaaunikh.Api/Gaaunikh.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /src/frontend/dist ./wwwroot
ENTRYPOINT ["dotnet", "Gaaunikh.Api.dll"]
