# Step 1: Build .NET App + Tailwind CSS
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl gnupg && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

COPY package*.json ./
RUN npm ci

COPY Itransition_Task4/Itransition_Task4.csproj Itransition_Task4/
RUN dotnet restore Itransition_Task4/Itransition_Task4.csproj

COPY . .

RUN npx @tailwindcss/cli -i ./Itransition_Task4/Styles/input.css -o ./Itransition_Task4/wwwroot/css/site.css

WORKDIR /src/Itransition_Task4
RUN dotnet publish Itransition_Task4.csproj -c Release -o /app/publish /p:UseAppHost=false

# Step 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Prevents Linux inotify file handle exhaustion on Render
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Itransition_Task4.dll"]