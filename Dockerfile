# Базовый образ для выполнения приложения
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем только проект из подпапки TodoApi, путь относительно текущей папки (где расположен Dockerfile)
COPY ["TodoApi/TodoApi.csproj", "TodoApi/"]

# Выполняем восстановление пакетов внутри папки с проектом
RUN dotnet restore "TodoApi/TodoApi.csproj"

# Копируем весь код проекта относительно корня контекста, включая папку TodoApi
COPY . .

# Указываем рабочую директорию для сборки внутри /src/TodoApi
WORKDIR "/src/TodoApi"

# Компиляция проекта
RUN dotnet build "TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация проекта в папку /app/publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TodoApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный этап - образ для запуска приложения
FROM base AS final
WORKDIR /app

# Копируем опубликованные файлы из предыдущего шага
COPY --from=publish /app/publish .

# Запуск приложения (обратите внимание — запускаем TodoApi.dll из корня /app, без подпапки)
ENTRYPOINT ["dotnet", "TodoApi.dll"]
