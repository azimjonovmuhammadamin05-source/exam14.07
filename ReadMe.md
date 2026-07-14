# TaskHub — Платформа управления задачами

Безопасная система управления задачами для команд с аутентификацией, авторизацией и ролевой системой.

## Запуск проекта

### 1. Требования
- .NET 10.0 SDK или выше
- Git

### 2. Клонирование репозитория
```bash
git clone https://github.com/username/taskhub.git
cd TaskHub
```

### 3. Конфигурация окружения

#### Вариант A: Локальное разработка (appsettings.Development.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=taskhub.db"
  },
  "JwtSettings": {
    "Secret": "your-actual-secret-key-here-minimum-256-bits-long",
    "Issuer": "TaskHubIssuer",
    "Audience": "TaskHubAudience"
  },
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  }
}
```

#### Вариант B: Переменные окружения (Production)
```bash
export JwtSettings__Secret="your-secret-key"
export JwtSettings__Issuer="TaskHubIssuer"
export JwtSettings__Audience="TaskHubAudience"
export ConnectionStrings__DefaultConnection="Server=your-sql-server;Database=TaskHub;..."
export Authentication__Google__ClientId="your-google-client-id"
export Authentication__Google__ClientSecret="your-google-client-secret"
```

### 4. Применение миграций и запуск
```bash
# Миграции применяются автоматически при первом запуске
dotnet run
```

Приложение будет доступно по адресу: `http://localhost:5000`
- API: `http://localhost:5000/api`
- Swagger: `http://localhost:5000/swagger`

---

## Архитектура

### Слои приложения
1. **Controllers** — REST API endpoints для аутентификации, авторизации и управления задачами
2. **Services** — бизнес-логика (Email Service)
3. **Data** — Entity Framework Core DbContext и модели данных
4. **Models** — ApplicationUser, TaskItem с необходимыми свойствами

### Безопасность
- **ASP.NET Core Identity** — управление пользователями и хэшированием паролей
- **JWT Bearer** — токен-базированная аутентификация для API
- **Role-based Authorization** — роли (User, Manager, Admin)
- **Claims-based Authorization** — гранулярный контроль доступа
- **Password Hashing** — использует PBKDF2 с солью (по умолчанию в Identity)

---

## API Endpoints

### Аутентификация

| Метод | Endpoint | Описание |
|---|---|---|
| POST | `/api/auth/register` | Регистрация нового пользователя |
| POST | `/api/auth/login` | Вход (возвращает JWT токен) |
| POST | `/api/auth/logout` | Выход (требует [Authorize]) |
| POST | `/api/auth/forgot-password` | Запрос reset-токена |
| POST | `/api/auth/reset-password` | Сброс пароля по токену |
| GET | `/api/auth/external-login` | Вход через Google OAuth |

### Управление задачами

| Метод | Endpoint | Описание |
|---|---|---|
| GET | `/api/tasks` | Получить все задачи пользователя |
| POST | `/api/tasks` | Создать новую задачу |
| PUT | `/api/tasks/{id}` | Обновить задачу (только владелец) |
| DELETE | `/api/tasks/{id}` | Удалить задачу (только владелец) |

### Администрирование

| Метод | Endpoint | Описание | Требуемая роль |
|---|---|---|---|
| POST | `/api/admin/assign-role` | Назначить роль пользователю | Admin |

---

## Примеры использования

### Регистрация
```bash
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "fullName": "John Doe"
}
```

### Вход и получение JWT
```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Использование JWT в защищённых endpoints
```bash
GET /api/tasks
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Политика паролей

- **Минимальная длина:** 8 символов
- **Требуемые символы:** минимум 1 цифра, 1 спецсимвол, 1 заглавная буква, 1 строчная буква
- **Примеры валидных паролей:** `Secure@Pass123`, `MyP@ssw0rd`

---

## Хэширование паролей

ASP.NET Core Identity использует **PBKDF2** (Password-Based Key Derivation Function 2) с:
- Солью (256 бит)
- Итерациями: 10,000 по умолчанию
- Алгоритм: HMAC-SHA256

Пароли **никогда** не хранятся в открытом виде, только их хэши.

---

## Email Сервис

### Текущая реализация (Development)
ConsoleEmailSender логирует письма в консоль:
```
To: user@example.com
Subject: Reset Password
Message: Token: ABC123...
```

### Для Production (SMTP)
Замените ConsoleEmailSender на SmtpEmailSender с конфигурацией:
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@taskhub.com",
    "SenderName": "TaskHub",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSSL": true
  }
}
```

---

## Теоретические вопросы

### 1. Authentication vs Authorization
**Authentication** — процесс проверки личности пользователя (ВЫ действительно тот, кто вы есть?). В нашем приложении: проверка email и пароля при входе, валидация JWT токена.

**Authorization** — процесс проверки прав доступа после аутентификации (что вам разрешено делать?). В нашем приложении: проверка ролей через `[Authorize(Roles="Admin")]`, проверка владельца задачи через claims.

**Пример из проекта:**
- Authentication: `POST /api/auth/login` проверяет credentials
- Authorization: `DELETE /api/tasks/{id}` проверяет, является ли пользователь владельцем

### 2. Структура JWT-токена
JWT состоит из трёх частей, разделённых точками: `Header.Payload.Signature`

1. **Header** — метаинформация о токене (алгоритм `HS256`, тип `JWT`)
2. **Payload** — данные (claims): UserId, Email, Roles, время истечения (exp), издатель (iss)
3. **Signature** — хэш Header+Payload с использованием Secret Key для верификации подлинности

### 3. Почему не хранить JWT Secret в коде?
- **Безопасность:** если код скомпрометирован (GitHub leak, скомпилированный бинарник), злоумышленник может подделывать токены
- **Ротация:** невозможно изменить секрет без переделки кода и переразвёртывания
- **Разделение ответственности:** DevOps управляет секретами, разработчики — кодом
- **Compliance:** требование стандартов (SOC2, ISO27001)

### 4. ClaimsPrincipal и различие между Claims и Roles
**ClaimsPrincipal** — контекст текущего пользователя с набором утверждений (claims) о нём. Это объект, который содержит идентификацию и авторизационные данные.

**Roles-based авторизация:** проверяет наличие роли (`[Authorize(Roles="Admin")]`)
**Claims-based авторизация:** проверяет наличие специфического claim (`context.User.HasClaim("CanEditTask", "true")`)

**Преимущество Claims:** гибче и детальнее, позволяет учитывать динамические разрешения (например, "может редактировать задачи, созданные после 2024 года").

### 5. Риски хранения паролей в открытом виде
- **Rainbow Table Attack:** злоумышленник создаёт таблицы предвычисленных хэшей и сравнивает украденные пароли
- **Массовая утечка:** если база скомпрометирована, все пароли скомпрометированы одновременно
- **Переиспользование:** пользователи используют одинаковые пароли на разных сайтах

**Как Identity решает:**
- **Хэширование:** пароль преобразуется в необратимый хэш
- **Соль:** каждый пароль получает уникальную соль, разные хэши для одинаковых паролей
- **Slow hashing:** PBKDF2 с множественными итерациями замедляет перебор (brute force)

---

## Бонус: Google OAuth (Реализовано)

### Конфигурация Google
1. Перейти на https://console.cloud.google.com
2. Создать новый проект
3. Включить Google+ API
4. Создать OAuth 2.0 credentials (Web application)
5. Добавить Redirect URI: `http://localhost:5000/api/auth/external-login-callback`

### Добавить в appsettings
```json
"Authentication": {
  "Google": {
    "ClientId": "xxx.apps.googleusercontent.com",
    "ClientSecret": "xxx"
  }
}
```

### Использование
```
GET /api/auth/external-login?provider=Google&returnUrl=/dashboard
```

---

## Миграции БД

```bash
# Создать новую миграцию
dotnet ef migrations add InitialCreate

# Применить миграции
dotnet ef database update

# Откатить последнюю миграцию
dotnet ef database update PreviousMigration
```

---

## Разработка и тестирование

### Используйте Swagger
http://localhost:5000/swagger

### Или Thunder Client / Postman
[Экспортированная коллекция находится в `/docs/TaskHub.postman_collection.json`]

---

## Лицензия
MIT

## Автор
[Ваше имя]

---

## Примечания по безопасности
⚠️ **НИКОГДА не коммитьте:**
- `appsettings.Development.json` с реальными значениями
- JWT Secret
- Google OAuth credentials
- Строки подключения к БД

Используйте `.env` файлы и User Secrets для локальной разработки!
