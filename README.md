# EducaApp

API REST em ASP.NET Web API (.NET Framework 4.8.1) para gestão escolar, atualmente com o cadastro de **Alunos** como recurso principal. Projeto em desenvolvimento, organizado em camadas (Domain, Data, Service, Web API).

## Arquitetura

O projeto é dividido em 4 camadas (projetos da solução `EducaWebApi.slnx`):

| Projeto | Responsabilidade |
|---|---|
| `EducaWebApi` | Camada web: controllers, filtros, configuração do Web API e do Swagger |
| `EducaWebApi.Service` | Regras de negócio |
| `EducaWebApi.Data` | Acesso a dados via Dapper (SQL Server) |
| `EducaWebApi.Domain` | Entidades, DTOs, interfaces e exceções de domínio (sem dependências externas) |

Fluxo de dependência: `EducaWebApi` → `Service`/`Data`/`Domain`; `Service` → `Data` (via interface) → `Domain`.

## Tecnologias

- .NET Framework 4.8.1
- ASP.NET Web API 2 (`Microsoft.AspNet.WebApi` 5.2.9)
- [Dapper](https://github.com/DapperLib/Dapper) 2.1.79 (micro-ORM sobre `System.Data.SqlClient`)
- SQL Server
- Swashbuckle 5.6.0 (Swagger / Swagger UI)
- Newtonsoft.Json 13.0.3

## Funcionalidades

### Recurso: Alunos (`/api/alunos`)

| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/alunos` | Listagem paginada de alunos, com filtro opcional por nome |

Parâmetros de `GET /api/alunos`: `nome` (filtro opcional), `pagina` (padrão 1), `tamanhoPagina` (padrão 10, máximo 100).

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

Os campos de paginação só são preenchidos em respostas paginadas (ex.: listagem de alunos).

## Tratamento de erros

Um filtro global (`TratamentoDeExcecoesFilter`) converte exceções em respostas padronizadas:

| Exceção | Status HTTP |
|---|---|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `ValidationException` | 400 |
| `DatabaseException` | 500 (detalhes registrados via `Trace`, mensagem genérica ao cliente) |
| Qualquer outra | 500 |

## Configuração e execução

1. Abra `EducaWebApi/EducaWebApi.slnx` no Visual Studio (2022+) e restaure os pacotes NuGet.
2. Ajuste a connection string `EducaWebApiConnection` em `EducaWebApi/Web.config` para o seu SQL Server local (atualmente aponta para `Server=localhost\SQLEXPRESS01;Database=TesteEscola`).
3. Crie o banco e a tabela `Aluno` com as colunas: `Id, Nome, Email, DataNascimento, Ativo, DataCadastro`.
4. Rode o projeto `EducaWebApi` (F5, via IIS Express).
5. Acesse a documentação Swagger em `/swagger`.

## Status do projeto

Projeto em desenvolvimento ativo. Este README reflete o que já está implementado e será atualizado conforme novas funcionalidades forem concluídas.
