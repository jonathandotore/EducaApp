using Dapper;
using EducaWebApi.Data.Connection;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Entities;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EducaWebApi.Data.Repositories
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public AlunoRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<ResultadosPaginados<AlunoResponseDto>> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina)
        {
            return ExecutarAsync(async () =>
            {
                const string sqlItens = @"
                    SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                    FROM Aluno
                    WHERE (@NomeFiltro IS NULL OR Nome LIKE '%' + @NomeFiltro + '%')
                    ORDER BY Nome
                    OFFSET @Offset ROWS FETCH NEXT @TamanhoPagina ROWS ONLY;";

                const string sqlTotal = @"
                    SELECT COUNT(*)
                    FROM Aluno
                    WHERE (@NomeFiltro IS NULL OR Nome LIKE '%' + @NomeFiltro + '%');";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var offset = (pagina - 1) * tamanhoPagina;

                    var itens = await conexao.QueryAsync<AlunoResponseDto>(sqlItens, new
                    {
                        NomeFiltro = nomeFiltro,
                        Offset = offset,
                        TamanhoPagina = tamanhoPagina
                    });

                    var total = await conexao.ExecuteScalarAsync<int>(sqlTotal, new { NomeFiltro = nomeFiltro });

                    return new ResultadosPaginados<AlunoResponseDto>
                    {
                        Itens = new List<AlunoResponseDto>(itens),
                        TotalRegistros = total,
                        Pagina = pagina,
                        TamanhoPagina = tamanhoPagina
                    };
                }
            }, "Erro na tentativa de listar alunos.");
        }

        public Task<AlunoResponseDto> ObterPorId(int id)
        {
            return ExecutarAsync(async () =>
            {
                const string sql = @"
                    SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                    FROM Aluno
                    WHERE Id = @Id;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var aluno = await conexao.QuerySingleOrDefaultAsync<AlunoResponseDto>(sql, new { Id = id });
                    if (aluno == null)
                        throw new NotFoundException("Não foi possível identificar o aluno.");

                    return aluno;
                }
            }, "Erro na tentativa de obter aluno.");
        }

        public Task<AlunoResponseDto> Criar(AlunoRequestDto aluno)
        {
            return ExecutarAsync(async () =>
            {
                var entidade = new Aluno
                {
                    Nome = aluno.Nome,
                    Email = aluno.Email,
                    DataNascimento = aluno.DataNascimento,
                    Ativo = true,
                    DataCadastro = DateTime.Now
                };

                const string sql = @"
                    INSERT INTO Aluno (Nome, Email, DataNascimento, Ativo, DataCadastro)
                    OUTPUT INSERTED.Id
                    VALUES (@Nome, @Email, @DataNascimento, @Ativo, @DataCadastro);";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    entidade.Id = await conexao.ExecuteScalarAsync<int>(sql, entidade);
                }

                return MapearParaResponseDto(entidade);
            }, "Erro na tentativa de criar aluno.");
        }

        public Task<AlunoResponseDto> Atualizar(int id, AlunoRequestDto aluno)
        {
            return ExecutarAsync(async () =>
            {
                const string sqlObter = @"
                    SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                    FROM Aluno
                    WHERE Id = @Id;";

                const string sqlAtualizar = @"
                    UPDATE Aluno
                    SET Nome = @Nome, Email = @Email, DataNascimento = @DataNascimento
                    WHERE Id = @Id;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var entidade = await conexao.QuerySingleOrDefaultAsync<Aluno>(sqlObter, new { Id = id });
                    if (entidade == null)
                        throw new NotFoundException("Aluno não encontrado.");

                    entidade.Nome = aluno.Nome;
                    entidade.Email = aluno.Email;
                    entidade.DataNascimento = aluno.DataNascimento;

                    await conexao.ExecuteAsync(sqlAtualizar, entidade);

                    return MapearParaResponseDto(entidade);
                }
            }, "Erro na tentativa de atualizar aluno.");
        }

        public Task<AlunoResponseDto> Inativar(int id)
        {
            return ExecutarAsync(async () =>
            {
                const string sqlObter = @"
                    SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                    FROM Aluno
                    WHERE Id = @Id;";

                const string sqlInativar = @"UPDATE Aluno SET Ativo = 0 WHERE Id = @Id;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var entidade = await conexao.QuerySingleOrDefaultAsync<Aluno>(sqlObter, new { Id = id });
                    if (entidade == null)
                        throw new NotFoundException("Aluno não encontrado.");

                    if (!entidade.Ativo)
                        throw new ConflictException("Aluno já está inativo.");

                    await conexao.ExecuteAsync(sqlInativar, new { Id = id });
                    entidade.Ativo = false;

                    return MapearParaResponseDto(entidade);
                }
            }, "Erro na tentativa de inativar aluno.");
        }

        #region Métodos privados

        private async Task<T> ExecutarAsync<T>(Func<Task<T>> operacao, string mensagemErro)
        {
            try
            {
                return await operacao();
            }
            catch (SqlException ex)
            {
                throw new DatabaseException(mensagemErro, ex);
            }
        }

        private static AlunoResponseDto MapearParaResponseDto(Aluno aluno)
        {
            return new AlunoResponseDto
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Ativo = aluno.Ativo,
                DataCadastro = aluno.DataCadastro
            };
        }

        #endregion
    }
}
