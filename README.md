# EducaApp

API REST em ASP.NET Web API (.NET Framework 4.8.1) para gestão escolar, cobrindo cadastro de **Alunos**, **Turmas**, **Matrículas** e um **relatório** de alunos por turma. Backend puro (sem views): o consumo é feito via HTTP/JSON, com um frontend separado ainda a ser desenvolvido.

## Arquitetura

O projeto é dividido em 5 projetos da solução `EducaWebApi/EducaWebApi.slnx`:

| Projeto | Responsabilidade |
|---|---|
| `EducaWebApi` | Camada web: controllers, filtro global de exceções, configuração do Web API e do Swagger |
| `EducaWebApi.Service` | Regras de negócio e validações |
| `EducaWebApi.Data` | Acesso a dados via Dapper (SQL Server) e cache via Redis (`StackExchange.Redis`) |
| `EducaWebApi.Domain` | Entidades, DTOs, interfaces, exceções e constantes de domínio (sem dependências externas) |
| `EducaWebApi.Tests` | Testes unitários (xUnit + Moq) dos serviços |

Fluxo de dependência: `EducaWebApi` → `Service`/`Data`/`Domain`; `Service` → `Data` (via interface) → `Domain`. `EducaWebApi.Tests` referencia apenas `EducaWebApi.Service`.

A camada `EducaWebApi` contém somente o necessário para expor a API: `Controllers/`, `Filters/`, `App_Start/` (Web API + Swagger) e os arquivos padrão do host ASP.NET (`Global.asax`, `Web.config`). Não há Views, CSHTML, CSS ou JS de boilerplate — esse projeto não serve nenhuma página HTML.

## Tecnologias

- .NET Framework 4.8.1
- ASP.NET Web API 2 (`Microsoft.AspNet.WebApi` 5.2.9)
- [Dapper](https://github.com/DapperLib/Dapper) (micro-ORM sobre `System.Data.SqlClient`)
- SQL Server
- Redis (`StackExchange.Redis`) para cache de leitura
- Swashbuckle 5.6.0 (Swagger / Swagger UI) — única documentação/UI exposta pelo projeto
- Newtonsoft.Json 13.0.3
- xUnit + Moq (testes unitários)

## Funcionalidades

### Recurso: Alunos (`/api/alunos`)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/alunos` | Listagem paginada, com filtro opcional por nome |
| GET | `/api/alunos/{id}` | Obtém um aluno pelo id |
| POST | `/api/alunos` | Cria um aluno |
| PUT | `/api/alunos/{id}` | Atualiza nome, e-mail e data de nascimento |
| DELETE | `/api/alunos/{id}` | Inativa o aluno (soft delete: `Ativo = false`) |

Parâmetros de `GET /api/alunos`: `nome` (filtro opcional), `pagina` (padrão 1), `tamanhoPagina` (padrão 10, máximo 100).

Validações de `POST`/`PUT`: nome obrigatório, e-mail obrigatório e válido, data de nascimento obrigatória e não pode ser futura.

### Recurso: Turmas (`/api/turmas`)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/turmas` | Lista todas as turmas |

A listagem usa cache-aside no Redis (chave `turmas:listar`, TTL de 1 minuto): a primeira leitura consulta o banco e popula o cache; leituras seguintes vêm do Redis até expirar ou até uma matrícula invalidar a chave.

### Recurso: Matrículas (`/api/matriculas`)

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/matriculas` | Matricula um aluno em uma turma |

Regras aplicadas antes de gravar: a turma precisa ter vaga disponível, o aluno precisa estar ativo e não pode já estar matriculado na mesma turma. A gravação (inserir matrícula + decrementar `VagasDisponiveis`) roda em uma única transação no banco, com nova checagem de vaga no `UPDATE` para evitar condição de corrida entre requisições concorrentes. Ao concluir, a chave de cache `turmas:listar` é invalidada.

### Recurso: Relatórios (`/api/relatorios`)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/relatorios/alunos-por-turma` | Para cada turma: nome, quantidade de alunos matriculados, vagas restantes e a lista de nomes dos alunos |

## Padrão de resposta

Todas as respostas da API seguem o envelope `MainResponse<T>`:

```json
{
  "dados": { },
  "contemErros": false,
  "mensagem": null,
  "statusCode": 200,
  "paginaAtual": 1,
  "tamanhoPagina": 10,
  "totalRegistros": 42,
  "totalPaginas": 5
}
```

Os campos de paginação só são preenchidos em respostas paginadas (listagem de alunos).

## Tratamento de erros

Um filtro global (`TratamentoDeExcecoesFilter`) converte exceções em respostas padronizadas:

| Exceção | Status HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `ValidationException` | 400 |
| `DatabaseException` | 500 (detalhes registrados via `Trace`, mensagem genérica ao cliente) |
| Qualquer outra | 500 |

## Banco de dados

Tabelas usadas pelas queries Dapper (não há script de criação versionado no repositório):

- **Aluno**: `Id, Nome, Email, DataNascimento, Ativo, DataCadastro`
- **Turma**: `Id, Nome, VagasTotal, VagasDisponiveis`
- **Matricula**: `Id, AlunoId, TurmaId, DataMatricula`

## Configuração e execução

1. Abra `EducaWebApi/EducaWebApi.slnx` no Visual Studio (2022+) e restaure os pacotes NuGet.
2. Ajuste a connection string `EducaWebApiConnection` em `EducaWebApi/Web.config` para o seu SQL Server local (atualmente aponta para `Server=localhost\SQLEXPRESS01;Database=TesteEscola`) e crie as tabelas listadas acima.
3. Tenha um Redis acessível para o cache de turmas; a chave `RedisConnection` em `Web.config` aponta por padrão para `localhost:6379,abortConnect=false`. Sem Redis disponível a API continua funcionando (falhas de cache são só logadas via `Trace` e a leitura cai para o banco).
4. Rode o projeto `EducaWebApi` (F5, via IIS Express).
5. A aplicação `por padrão` irá abrir uma página WEB que disponibiliza um botão para obter a listagem das turmas.
6. Caso prefira abrir a documentação no Swagger apenas adicione `/swagger` na URL.
7. Caso prefira utilizar o Postman: o arquivo `json` da collection está no repositório, basta importá-lo.
## Testes

```
dotnet test EducaWebApi.Tests/EducaWebApi.Tests.csproj
```

Cobrem `MatriculaService` e `TurmaService`, incluindo os fluxos de cache hit e cache miss no Redis (simulado via mock de `ICacheService`).

## Status do projeto

Projeto em desenvolvimento ativo. Este README reflete o que já está implementado e será atualizado conforme novas funcionalidades forem concluídas.

