# ChargePay Platform - Backend

Uma plataforma financeira moderna para gerenciamento de recargas para veículos elétricos, desenvolvida com ASP.NET Core 10, arquitetura de microsserviços, Apache Kafka e PostgreSQL.

## 🏗️ Arquitetura

```
ChargePay.Api
    ├── Controllers
    ├── Middleware
    └── DTOs

ChargePay.Application
    ├── UseCases
    └── DTOs

ChargePay.Infrastructure
    ├── Data (EntityFrameworkCore)
    ├── Repositories
    └── Kafka

ChargePay.Domain
    ├── Entities
    ├── Events
    ├── ValueObjects
    ├── Repositories (Interfaces)
    └── Enums
```

## 🚀 Começando

### Pré-requisitos

- .NET 10 SDK
- Docker e Docker Compose
- Git

### Instalação Local

#### 1. Clonar o repositório

```bash
git clone <repositorio-url>
cd ChargePayPlatform
```

#### 2. Iniciar infraestrutura com Docker

```bash
docker-compose up -d
```

Isso iniciará:
- **PostgreSQL 15**: Database principal (porta 5432)
- **PgAdmin 4**: Interface gráfica para PostgreSQL (porta 5050)
- **Zookeeper**: Coordenador Kafka (porta 2181)
- **Apache Kafka**: Message broker (porta 9092)
- **Kafka UI**: Interface gráfica para Kafka (porta 8080)

#### 3. Restaurar pacotes NuGet

```bash
dotnet restore
```

#### 4. Executar migrações

```bash
# A migração é executada automaticamente ao iniciar a API
# Mas se quiser executar manualmente:
cd ChargePay.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 5. Executar a API

```bash
cd ChargePay.Api
dotnet run
```

A API estará disponível em: `https://localhost:5001`

**Swagger/OpenAPI**: `https://localhost:5001/swagger`

## 📚 Documentação

### Estrutura de Dados

#### Entidades Principais

- **User**: Usuário (PF ou PJ) com autenticação JWT
- **Wallet**: Carteira digital com saldo
- **ChargingStation**: Estação de recarga
- **ChargingSession**: Sessão ativa de recarga
- **Receipt**: Comprovante de recarga
- **AuditLog**: Registro de auditoria
- **Notification**: Notificações do sistema

### Configuração

#### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=chargepay_db;Username=chargepay_user;Password=chargepay_password"
  },
  "Jwt": {
    "Secret": "sua-chave-super-secreta",
    "Issuer": "ChargePay",
    "Audience": "ChargePayApi",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "chargepay-consumer-group"
  }
}
```

## 📡 Eventos (Kafka)

Os eventos publicados no Kafka:

1. **usuario-criado**: Quando um novo usuário é registrado
2. **usuario-autenticado**: Quando um usuário faz login
3. **carteira-criada**: Quando a carteira é criada automaticamente
4. **saldo-creditado**: Quando créditos são adicionados
5. **saldo-debitado**: Quando há débito na carteira
6. **estacao-criada**: Quando uma estação é registrada
7. **sessao-criada**: Quando uma nova sessão é inicializada
8. **sessao-autorizada**: Quando a sessão é autorizada
9. **telemetria-recebida**: Quando dados de telemetria chegam
10. **consumo-calculado**: Quando o consumo é processado
11. **cobranca-gerada**: Quando uma cobrança é criada
12. **sessao-finalizada**: Quando a sessão termina
13. **recibo-gerado**: Quando o comprovante é gerado

## 🔐 Autenticação

A autenticação é feita via **JWT (JSON Web Tokens)**:

### Login

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@example.com",
  "password": "senha123"
}
```

**Resposta:**

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 3600
}
```

### Usando o Token

Adicionar header em todas as requisições autenticadas:

```
Authorization: Bearer <seu_access_token>
```

### Renovar Token

```bash
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "seu_refresh_token"
}
```

## 🛠️ Desenvolvimento

### Estrutura de Pastas

```
ChargePay.Domain/
  ├── Entities/         # Entidades de domínio
  ├── ValueObjects/     # Value Objects (Money, Email, Document)
  ├── Events/           # Eventos de domínio
  ├── Repositories/     # Interfaces de repositório
  ├── Exceptions/       # Exceções customizadas
  └── Enums/            # Enumerações

ChargePay.Application/
  ├── UseCases/         # Casos de uso / Services
  ├── DTOs/             # Data Transfer Objects
  └── Interfaces/       # Interfaces de aplicação

ChargePay.Infrastructure/
  ├── Data/             # EF Core DbContext
  ├── Repositories/     # Implementações de repositório
  └── Kafka/            # Produtor/Consumidor Kafka

ChargePay.Api/
  ├── Controllers/      # Endpoints REST
  ├── Middleware/       # Middlewares customizados
  └── Program.cs        # Configuração principal
```

### Padrões de Código

#### Value Objects

```csharp
var emailResult = Email.Create("usuario@example.com");
if (!emailResult.IsSuccess)
{
    // Tratare rro
}

var email = emailResult.Data;
```

#### Entidades de Domínio

```csharp
var userResult = User.Create(
    name: "João Silva",
    email: email,
    document: document,
    type: UserType.Individual,
    passwordHash: hashedPassword
);
```

#### Repositories

```csharp
var user = await userRepository.GetByEmailAsync("usuario@example.com");
var transactions = await walletRepository.GetTransactionsAsync(walletId);
```

## 🧪 Testes

```bash
# Executar todos os testes
dotnet test

# Com cobertura
dotnet test /p:CollectCoverage=true
```

## 📊 Banco de Dados

### Acessar PostgreSQL via PgAdmin

- **URL**: `http://localhost:5050`
- **Email**: `admin@chargepay.local`
- **Senha**: `admin`

### Acessar Kafka via Kafka UI

- **URL**: `http://localhost:8080`

## 🚨 Troubleshooting

### Erro: "Cannot connect to database"

1. Verificar se o Docker está rodando: `docker ps`
2. Reiniciar os containers: `docker-compose restart postgres`
3. Verificar a connection string em `appsettings.json`

### Erro: "Kafka connection failed"

1. Verificar se o Zookeeper está saudável: `docker-compose ps`
2. Aguardar alguns segundos para o Kafka inicializar
3. Testar conexão: `telnet localhost 9092`

### Erro: "Migration pending"

Execute as migrações:

```bash
cd ChargePay.Api
dotnet ef database update
```

## 📝 Logs

Os logs são armazenados em:

```
logs/chargepay-YYYY-MM-DD.txt
```

Também aparecem no console durante desenvolvimento.

## 🔄 CI/CD

Integração contínua será configurada com GitHub Actions para:

- Build automático
- Testes unitários
- Testes de integração
- Deploy em staging
- Deploy em produção

## 📋 Roadmap

- [ ] Sistema de notificações real-time (SignalR)
- [ ] Dashboard de análises
- [ ] Integração com bancos (Open Banking)
- [ ] Mobile App (Flutter)
- [ ] Relatórios financeiros
- [ ] Sistema de recomendações com IA

## 👨‍💼 Suporte

Para dúvidas ou problemas, abra uma issue no repositório.

## 📄 Licença

Este projeto está licenciado sob a MIT License - veja o arquivo LICENSE para detalhes.

## ✨ Contribuindo

Contribuições são bem-vindas! Por favor:

1. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
2. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
3. Push para a branch (`git push origin feature/AmazingFeature`)
4. Abra um Pull Request

---

**Desenvolvido com ❤️ para uma mobilidade sustentável**
