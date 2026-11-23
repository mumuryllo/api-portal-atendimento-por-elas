# Portal Por Elas — Back-End API

API RESTful desenvolvida em **.NET 8** para gerenciamento de autenticação, usuários e descoberta dinâmica de recursos (HATEOAS) do ecossistema **Portal Por Elas**.

---

## Arquitetura do Projeto

O projeto segue uma **Arquitetura em Camadas com Domain-Driven Design (DDD) leve**, isolando regras de negócio, contratos de entrada/saída e persistência de dados:

```text
portal-por-elas/
├── AutoMapping/          # Perfis de mapeamento entre Entidades e DTOs (AutoMapper)
├── Contracts/            # DTOs de Request e Response da API
│   ├── Request/
│   └── Response/
├── Controllers/          # Endpoints HTTP documentados via Swagger/OpenAPI
├── Data/                 # Camada de persistência (EF Core + PostgreSQL)
│   ├── Context/          # PortalPorElasDbContext
│   ├── Mappings/         # Configurações Fluent API (EntityTypeConfiguration)
│   ├── Migrations/       # Histórico de versionamento do banco de dados
│   ├── Repositories/     # Implementação do Generic Repository e repositórios específicos
│   └── UnitOfWork/       # Controle transacional atômico (CommitAsync)
├── Domain/               # Núcleo da aplicação sem dependências externas
│   ├── Constants/        # Constantes de tabelas, colunas e autorização
│   └── Entities/         # Entidades ricas com encapsulamento e validações de domínio
├── Exceptions/           # Exceções customizadas de domínio e HTTP (Conflict, NotFound, Unauthorized)
├── Extensions/           # Métodos de extensão para DI, Banco de Dados e Swagger
├── Interfaces/           # Contratos de Repositórios, Unit of Work e Serviços
├── Middlewares/          # Tratamento global de exceções (ErrorHandlingMiddleware)
├── Security/             # Configurações de token JWT (JwtSettings)
├── Services/             # Regras de negócio (UserService, AuthService, HateoasService)
└── Validators/           # Validações de entrada com FluentValidation
Tecnologias, Pacotes e Extensões UtilizadasCategoriaPacote / ExtensãoFinalidadeFramework.NET 8.0 / ASP.NET Core Web APIBase da aplicação web e pipeline HTTPBanco de DadosNpgsql.EntityFrameworkCore.PostgreSQL (8.0.11)Provedor Entity Framework Core para PostgreSQLDesign / CLIMicrosoft.EntityFrameworkCore.Design (8.0.24)Geração e aplicação de MigrationsAutenticaçãoMicrosoft.AspNetCore.Authentication.JwtBearer (8.0.24)Validação de tokens JWT nas rotas protegidasTokensSystem.IdentityModel.Tokens.Jwt (8.15.0)Geração de Access Tokens e leitura de ClaimsSegurançaBCrypt.Net-Next (4.0.3)Hashing seguro de senhasMapeamentoAutoMapper.Extensions.Microsoft.DependencyInjection (12.0.0)Conversão automática entre Entidades e DTOsValidaçãoFluentValidation.AspNetCore (11.3.1)Validação declarativa dos DTOs de requisiçãoDocumentaçãoSwashbuckle.AspNetCore (6.6.2)Interface Swagger UI configurada com suporte a Bearer TokenTestesxUnit (2.9.2), AutoFixture.AutoMoq (3.6.2), WebApplicationFactoryTestes unitários e de integraçãoExtensões Customizadas (Extensions/)DataBaseServiceExtension: Registra o PortalPorElasDbContext conectado ao PostgreSQL via DefaultConnection.DependencyInjectionExtension: Registra IRepository<>, IUserRepository, IUnitOfWork, IUserService, IAuthService e IHateoasService no container de injeção de dependência.SwaggerExtension: Configura a documentação OpenAPI e o botão Authorize (JWT Bearer) no Swagger UI.Principais FuncionalidadesAutenticação JWT + Refresh Token: Login com emissão de AccessToken e RefreshToken rotativo armazenado em banco, além de endpoint para renovação e revogação (logout).Gestão de Usuários: Cadastro com validação de unicidade de e-mail, hash de senha com BCrypt, alteração de senha e exclusão.Descoberta Dinâmica HATEOAS (/api/navigation/links):Lê os endpoints registrados na aplicação via EndpointDataSource e retorna apenas as rotas permitidas para a Role do usuário autenticado.Adapta URLs e metadados conforme o ambiente (Development vs Production), permitindo simulação de roles (?simulateRole=...) apenas em desenvolvimento.Tratamento Global de Erros: Middleware centralizado que converte exceções de domínio (ConflictException, NotFoundException, UnauthorizedException, etc.) em respostas HTTP padronizadas.Configuração de AmbientesConfigure o banco PostgreSQL e ajuste o arquivo appsettings.Development.json (este arquivo não deve ser versionado no Git):JSON{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=portal_por_elas;Username=root;Password=root"
  },
  "JwtSettings": {
    "SecretKey": "sua-chave-secreta-com-no-minimo-32-caracteres",
    "ExpirationMinutes": 240,
    "Issuer": "PortalPorElas",
    "Audience": "PortalPorElas"
  },
  "ApiSettings": {
    "BaseUrl": "[https://api.dev.portalporelas.com.br](https://api.dev.portalporelas.com.br)"
  }
}
Em Produção, defina as variáveis de ambiente no servidor/container:ASPNETCORE_ENVIRONMENT=ProductionConnectionStrings__DefaultConnection=<string-de-conexao-producao>JwtSettings__SecretKey=<chave-secreta-producao>Comandos do Entity Framework (Migrations)Execute os comandos abaixo a partir da pasta raiz da solução (onde está o arquivo .sln) ou entre na subpasta portal-por-elas:1. Instalar a ferramenta CLI do EF Core (apenas na primeira vez)Bashdotnet tool install --global dotnet-ef
2. Criar uma nova MigrationBashdotnet ef migrations add NomeDaMigration --project portal-por-elas/portal-por-elas.csproj --output-dir Data/Migrations
3. Aplicar as Migrations e atualizar o banco de dadosBashdotnet ef database update --project portal-por-elas/portal-por-elas.csproj
4. Reverter ou remover a última Migration (caso necessário)Bash# Reverter o banco para uma migration anterior
dotnet ef database update NomeDaMigrationAnterior --project portal-por-elas/portal-por-elas.csproj

# Remover o arquivo da última migration não aplicada
dotnet ef migrations remove --project portal-por-elas/portal-por-elas.csproj
Executando a AplicaçãoBash# Ambiente de Desenvolvimento (habilita Swagger em /swagger)
dotnet run --project portal-por-elas/portal-por-elas.csproj --launch-profile "Development (http)"

# Ambiente de Produção (local)
dotnet run --project portal-por-elas/portal-por-elas.csproj --launch-profile "Production (Local Test)"