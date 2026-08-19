# Industrial ERP

Sistema web desenvolvido como desafio técnico, simulando funcionalidades de um ERP industrial para gerenciamento de produtos, controle de estoque e emissão de notas fiscais.

O projeto utiliza uma arquitetura baseada em serviços independentes, com **Angular** no frontend, **ASP.NET Core** no backend e **PostgreSQL** para persistência dos dados.

## Sobre o projeto

O **Industrial ERP** foi desenvolvido com o objetivo de demonstrar a implementação de um fluxo integrado entre cadastro de produtos, controle de estoque e emissão de notas fiscais.

A solução foi dividida em dois backends independentes:

- **Produtos e Estoque** — responsável pelo cadastro de produtos, consulta de saldo e movimentações de estoque.
- **Notas Fiscais** — responsável pela criação e gerenciamento das notas fiscais, clientes, empresas e itens da nota.

O frontend desenvolvido em Angular consome as APIs REST disponibilizadas pelos dois serviços.

## Funcionalidades

### Produtos

- Listagem de produtos
- Cadastro de novos produtos
- Edição de produtos
- Exclusão lógica (soft delete)
- Controle de produtos ativos

### Estoque

- Consulta de saldo por produto
- Baixa de estoque
- Estorno de estoque
- Registro das movimentações
- Validação para impedir saldo negativo
- Prevenção de movimentações duplicadas

### Notas Fiscais

- Listagem de notas fiscais
- Criação de notas fiscais
- Numeração sequencial
- Seleção de cliente e empresa
- Inclusão de produtos na nota
- Consulta de saldo durante a inclusão dos produtos
- Processamento da nota com baixa de estoque
- Cancelamento de notas
- Estorno de estoque ao cancelar uma nota processada

## Regras de negócio

As notas fiscais podem possuir os seguintes status:

- **Aberta**
- **Processada**
- **Cancelada**

Uma nova nota fiscal é criada com o status **Aberta**.

Enquanto estiver aberta, a nota pode ser alterada e seus itens podem ser gerenciados.

Ao processar uma nota:

1. O saldo dos produtos é validado.
2. As movimentações de estoque são realizadas.
3. O estoque dos produtos é atualizado.
4. A nota passa para o status **Processada**.

Não é permitido realizar uma baixa que resulte em saldo negativo.

O cancelamento depende do estado atual da nota:

- **Nota Aberta:** é apenas alterada para `Cancelada`, sem movimentação de estoque.
- **Nota Processada:** as movimentações realizadas anteriormente são estornadas e a nota passa para `Cancelada`.

Notas canceladas ou processadas não podem ser editadas.

As movimentações de estoque possuem uma referência que permite identificar a operação de origem e evitar movimentações duplicadas.

## Arquitetura

A aplicação foi dividida em três projetos principais:

```text
Angular Frontend
       |
       | HTTP / REST
       |
       +-----------------------+
       |                       |
       v                       v
Backend Produtos        Backend Notas
ASP.NET Core            ASP.NET Core
       |                       |
       v                       v
PostgreSQL              PostgreSQL
Produtos/Estoque        Notas Fiscais
```

O backend de produtos é responsável exclusivamente pelo domínio de produtos e estoque.

O backend de notas fiscais mantém apenas o `ProdutoId` nos itens da nota, evitando relacionamento direto de banco de dados entre serviços.

A comunicação entre os domínios ocorre através das APIs.

## Tecnologias utilizadas

### Frontend

- Angular
- TypeScript
- RxJS
- HTML
- CSS

### Backend

- C#
- ASP.NET Core
- Entity Framework Core
- REST APIs
- OpenAPI

### Banco de dados

- PostgreSQL
- Entity Framework Core Migrations

### Ferramentas

- Visual Studio Code
- Git
- GitHub
- Postman
- pgAdmin 4

## Estrutura do projeto

```text
industrial-erp/
│
├── backend-produtos/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Migrations/
│   └── Program.cs
│
├── backend-notas/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Models/
│   ├── Migrations/
│   └── Program.cs
│
├── frontend/
│   └── src/
│       └── app/
│           ├── components/
│           ├── models/
│           └── services/
│
└── README.md
```

## Executando o projeto

### Pré-requisitos

Para executar o projeto localmente é necessário possuir:

- .NET SDK
- Node.js
- Angular CLI
- PostgreSQL

### Banco de dados

O projeto utiliza dois bancos PostgreSQL independentes:

```text
Produtos / Estoque
Notas Fiscais
```

Configure as respectivas connection strings nos arquivos de configuração dos backends.

Após configurar os bancos, execute as migrations do Entity Framework Core em cada backend.

```bash
dotnet ef database update
```

> As credenciais e connection strings locais não devem ser versionadas no repositório.

### Backend de Produtos

Acesse o diretório:

```bash
cd backend-produtos
```

Execute:

```bash
dotnet restore
dotnet run
```

Por padrão, durante o desenvolvimento, a API é executada em:

```text
http://localhost:5019
```

### Backend de Notas Fiscais

Acesse o diretório:

```bash
cd backend-notas
```

Execute:

```bash
dotnet restore
dotnet run
```

Por padrão, durante o desenvolvimento, a API é executada em:

```text
http://localhost:5150
```

### Frontend Angular

Acesse o diretório do frontend:

```bash
cd frontend
```

Instale as dependências:

```bash
npm install
```

Execute a aplicação:

```bash
ng serve
```

A aplicação estará disponível em:

```text
http://localhost:4200
```

## Endpoints da API

### Produtos

```http
GET    /api/Produtos
POST   /api/Produtos
PUT    /api/Produtos/{id}
GET    /api/Produtos/{id}
DELETE /api/Produtos/{id}
```

### Estoque

```http
GET  /api/Estoque/{produtoId}
POST /api/Estoque/baixa
POST /api/Estoque/estorno
```

### Clientes

```http
GET /api/Clientes
```

### Empresas

```http
GET /api/Empresas
```

### Notas Fiscais

```http
GET    /api/Notas
GET    /api/Notas/{id}
POST   /api/Notas
POST   /api/Notas/{id}/itens
PUT    /api/Notas/{notaId}/itens/{itemId}
DELETE /api/Notas/{notaId}/itens/{itemId}
POST   /api/Notas/{id}/processar
POST   /api/Notas/{id}/cancelar
```

A documentação detalhada dos endpoints, incluindo parâmetros, request bodies e modelos de resposta, está disponível através da especificação **OpenAPI** ao executar os backends.

**Backend de Produtos:**
`http://localhost:5019/openapi/v1.json`

**Backend de Notas Fiscais:**
`http://localhost:5150/openapi/v1.json`

## Screenshots

### Produtos

![Listagem de produtos](docs/images/produtos.jpg)

### Notas Fiscais

![Listagem de produtos](docs/images/notasCadastradas.jpg)

### Criação de Nota Fiscal

![Listagem de produtos](docs/images/criarNota.jpg)

### Controle de Estoque

![Listagem de produtos](docs/images/validacaoEstoque.jpg)

## Decisões técnicas

### Separação dos backends

Produtos/estoque e notas fiscais foram separados em serviços independentes para reduzir o acoplamento entre os domínios e demonstrar uma arquitetura preparada para evolução independente.

Cada serviço possui seu próprio banco de dados.

### Referência de produtos nas notas

O serviço de notas fiscais armazena apenas o identificador (`ProdutoId`) do produto.

Não existe chave estrangeira entre os bancos dos serviços, preservando a independência entre os domínios.

### Soft delete de produtos

Produtos não são removidos fisicamente do banco de dados.

A exclusão altera o estado do produto para inativo, preservando referências históricas.

### Movimentações de estoque

As alterações de estoque são registradas como movimentações.

Essa abordagem permite manter histórico das operações e possibilita realizar estornos de forma controlada.

### Prevenção de duplicidade

As movimentações possuem uma referência da operação responsável pela alteração do estoque.

Essa referência é utilizada para impedir que uma mesma operação gere movimentações duplicadas.

### Validação de saldo com RxJS

No frontend, RxJS é utilizado para controlar a consulta de saldo durante a seleção dos produtos, reduzindo chamadas desnecessárias à API e permitindo validar a disponibilidade antes do processamento da nota.

## Melhorias futuras

Algumas melhorias que poderiam ser implementadas em uma evolução do projeto:

- Autenticação e autorização de usuários
- Controle de acesso baseado em perfis
- CRUD completo de clientes e empresas
- Histórico detalhado das notas fiscais
- Paginação e filtros nas listagens
- Testes unitários e testes de integração
- Tratamento global de exceções
- Logs estruturados
- Dockerização dos serviços
- Docker Compose para execução completa do ambiente
- Documentação completa das APIs
- CI/CD
- Monitoramento e observabilidade
- Interface responsiva
- Dashboard com indicadores de estoque e faturamento

---

Projeto desenvolvido como parte de um desafio técnico para demonstrar conhecimentos em **Angular, ASP.NET Core, PostgreSQL, APIs REST, arquitetura de software e regras de negócio aplicadas a sistemas ERP**.
