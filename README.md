# RabbitaskWebAPI

API REST para o sistema de gerenciamento de tarefas Rabbitask.

## Pré-requisitos

### Para desenvolvimento local (Windows)

- **Visual Studio 2022** com a carga de trabalho "Desenvolvimento ASP.NET e web"
- **MySQL Server 8.0+** e **MySQL Workbench**
- **.NET 6.0 SDK** (instalado via Visual Studio ou [download separado](https://dotnet.microsoft.com/download/dotnet/6.0))

### Para desenvolvimento com Docker

- **Docker** e **Docker Compose**

---

## Configuração no Windows 

### 1. Clonar o Repositório

```bash
git clone https://github.com/scrwww/RabbitaskWebAPI.git
cd RabbitaskWebAPI
```

### 2. Abrir no Visual Studio 2022

1. Abra o **Visual Studio 2022**
2. Clique em **"Abrir um projeto ou solução"**
3. Navegue até a pasta clonada e selecione `RabbitaskWebAPI.sln`

### 3. Configurar o Banco de Dados MySQL

#### No MySQL Workbench:

1. Abra o **MySQL Workbench** e conecte-se ao seu servidor MySQL local
2. Execute o seguinte script SQL para criar o banco e o usuário:

```sql
-- Criar banco de dados
CREATE DATABASE IF NOT EXISTS rabbitask;

-- Criar usuário
CREATE USER IF NOT EXISTS 'rabbitask_user'@'localhost' IDENTIFIED BY 'usersecret';

-- Conceder privilégios
GRANT ALL PRIVILEGES ON rabbitask.* TO 'rabbitask_user'@'localhost';
FLUSH PRIVILEGES;
```

### 4. Configurar a Connection String

O arquivo `appsettings.Development.json` já está configurado para conexão local:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=rabbitask;User=rabbitask_user;Password=usersecret;"
  }
}
```

> Se seu MySQL estiver em uma porta diferente ou usar credenciais diferentes, atualize este arquivo.

### 5. Aplicar as Migrations

#### Via Package Manager Console (Visual Studio):

1. Abra **Ferramentas → Gerenciador de Pacotes NuGet → Console do Gerenciador de Pacotes**
2. Execute:

```powershell
Update-Database
```

#### Via Terminal:

```bash
cd RabbitaskWebAPI
dotnet ef database update
```

### 6. Configurar Variáveis de Ambiente (Opcional)

Para JWT e outras configurações, você pode adicionar os valores ao `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=3306;Database=rabbitask;User=rabbitask_user;Password=usersecret;"
  },
  "Jwt": {
    "Key": "your-256-bit-hex-key-min-64-chars-change-in-production",
    "Issuer": "Rabbitask",
    "Audience": "RabbitaskUsers"
  },
  "FrontendOrigin": "http://localhost:8100;http://localhost:4200;http://localhost:8101"
}
```

### 7. Executar o Projeto

1. No Visual Studio, pressione **F5** ou clique no botão **Executar** (verde)
2. A API iniciará em `https://localhost:7xxx` ou `http://localhost:5xxx`
3. O **Swagger UI** abrirá automaticamente para testar os endpoints

---

## Configuração com Docker

### 1. Executar com Docker Compose

```bash
docker-compose up -d
```

Isso irá:
- Criar e iniciar o container do MySQL
- Criar e iniciar o container da API
- Configurar a rede entre os containers

### 2. Verificar os Containers

```bash
docker-compose ps
```

### 3. Parar os Containers

```bash
docker-compose down
```

### 4. Parar e Remover Volumes (reset completo)

```bash
docker-compose down -v
```

---

## Estrutura do Projeto

```
RabbitaskWebAPI/
├── Controllers/       # Controladores da API
├── Data/              # Contexto do banco de dados
├── DTOs/              # Data Transfer Objects
├── Middleware/        # Middlewares personalizados
├── Migrations/        # Migrations do Entity Framework
├── Models/            # Modelos de domínio
├── Services/          # Serviços da aplicação
├── Auth/              # Handlers e Requirements de autorização
└── Properties/        # Configurações de launch
```

---

## Endpoints Principais

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/login` | Autenticação de usuário |
| POST | `/api/auth/register` | Registro de novo usuário |
| GET | `/api/tarefa` | Listar tarefas |
| POST | `/api/tarefa` | Criar tarefa |
| GET | `/api/tag` | Listar tags |
| GET | `/api/usuario` | Listar usuários |

---


## Solução de Problemas

### Erro de conexão com MySQL

1. Verifique se o MySQL está rodando
2. Confirme as credenciais no `appsettings.Development.json`
3. Teste a conexão no MySQL Workbench

### Erro nas Migrations

```bash
dotnet ef migrations remove  # Remove última migration
dotnet ef database update    # Reaplica migrations
```

### Porta já em uso

Altere a porta no arquivo `Properties/launchSettings.json`

---

## Licença

Este projeto está sob a licença MIT.
