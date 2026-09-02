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

        public async Task<AlunoResponseDto> ObterPorId(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Aluno> Atualizar(int id, Aluno aluno)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Aluno> Criar(Aluno aluno)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Aluno> Inativar(int id)
        {
            throw new System.NotImplementedException();
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

        #endregion
    }
}
