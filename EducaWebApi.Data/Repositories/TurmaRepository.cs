using Dapper;
using EducaWebApi.Data.Connection;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EducaWebApi.Data.Repositories
{
    public class TurmaRepository : ITurmaRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public TurmaRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<List<TurmaResponseDto>> Listar()
        {
            return ExecutarAsync(async () =>
            {
                const string sql = @"
                    SELECT Id, Nome, VagasTotal, VagasDisponiveis
                    FROM Turma
                    ORDER BY Nome;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var turmas = await conexao.QueryAsync<TurmaResponseDto>(sql);
                    return new List<TurmaResponseDto>(turmas);
                }
            }, "Erro na tentativa de listar turmas.");
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
