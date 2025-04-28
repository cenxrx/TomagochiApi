# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Копируем ТОЛЬКО файлы проекта
COPY ["TomagochiApi/TomagochiApi.csproj", "TomagochiApi/"]
# Если есть файл решения, раскомментируйте:
COPY ["TomagochiApi.sln", "./"]

# 2. Чистим кэш и восстанавливаем зависимости
RUN dotnet nuget locals all --clear
RUN dotnet restore "TomagochiApi/TomagochiApi.csproj" --runtime linux-x64

# 3. Копируем остальные файлы (исключая ненужные через .dockerignore)
COPY . .

# 4. Собираем проект
RUN dotnet publish "TomagochiApi/TomagochiApi.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false \
    /p:RestoreUseStaticGraphEvaluation=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TomagochiApi.dll"]