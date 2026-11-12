# MyDockerApp: .NET Backend & Observability

Учебный проект backend-сервиса, демонстрирующий подходы к наблюдаемости, отказоустойчивости и контейнеризации.

> **Цель проекта:** Отработка паттернов, используемых в production-среде.

## Технологический стек
*   **Backend:** C#, ASP.NET Core Web API (.NET 10)
*   **Базы данных:** PostgreSQL (EF Core, Code-First), Redis (распределенный кэш)
*   **Брокер сообщений:** Apache Kafka (Producer / Consumer как `BackgroundService`)
*   **Observability:**
    *   **Логи:** Serilog -> Graylog
    *   **Метрики:** Prometheus + Grafana
    *   **Трассировка:** Кастомный Middleware для `Correlation ID`
*   **Инфраструктура:** Docker, Docker Compose, Kubernetes
*   **Надежность:** Polly, Health Checks (`/health`)

## Ключевые фичи
1.  **Clean Architecture:** Controllers, Services, Data, DTOs, Middlewares.
2.  **Паттерн Options с валидацией:** принцип Fail-Fast.
3.  **Отказоустойчивость, явное подключение при старте:** При старте приложение явно подключается к Redis с политикой повторных попыток.
4.  **Безопасность:** Пароли передаются через переменные окружения (`.env`). В Dockerfile используется Multi-stage build и запуск от непривилегированного пользователя `app`.
5.  **Custom Middleware:** Замеры времени выполнения запроса (`RequestTimingMiddleware`) и генерация `X-Correlation-Id`.
6.  **Rate Limiting:** Защита API от спама (Token Bucket).
7.  **Интеграции:** Named и Typed HTTP-клиенты (`IHttpClientFactory`) для работы с внешними API.

## Docker Compose

Для запуска вам понадобится установленный Docker Desktop.

1. Клонируйте репозиторий.

2. Создайте файл **`.env`** в корне проекта и добавьте туда переменные для базы данных и сервисов мониторинга:
```env
DB_PASSWORD=your_secure_password
GRAYLOG_PASSWORD_SECRET=somepasswordpepper
GRAYLOG_ROOT_PASSWORD_SHA2=8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918
GRAFANA_ADMIN_PASSWORD=admin
DATANODE_PASSWORD_SECRET=somepasswordpepper
```

3. Выполните запуск проекта:
```bash
docker compose up -d --build
```

## Доступные интерфейсы после запуска
*   **API (OpenAPI спецификация): http://localhost:8080/openapi/v1.json
*   **Тестовый эндпоинт:** `http://localhost:8080/weatherforecast`
*   **Health Check:** `http://localhost:8080/health`
*   **Graylog (Логи):** `http://localhost:9000` (admin / admin)
*   **Grafana (Метрики):** `http://localhost:3000` (admin / admin)
*   **Prometheus:** `http://localhost:9090`

## Тестирование
В проекте есть пример юнит-теста (`xUnit` + `Moq`).

Запуск тестов:
```bash
dotnet test
```

## Kubernetes
В папке `k8s/` находятся манифесты для развертывания:
*   `deployment.yaml`
*   `scaledobject.yaml`