using Dapper;
using EducaWebApi.Data.Connection;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Entities;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EducaWebApi.Data.Repositories
{
    public class MatriculaRepository : IMatriculaRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public MatriculaRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<bool> TurmaTemVagaDisponivel(int turmaId)
        {
            return ExecutarAsync(async () =>
            {
                const string sql = "SELECT VagasDisponiveis FROM Turma WHERE Id = @TurmaId;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var vagasDisponiveis = await conexao.QuerySingleOrDefaultAsync<int?>(sql, new { TurmaId = turmaId });
                    if (vagasDisponiveis == null)
                        throw new NotFoundException("Turma não encontrada.");

                    return vagasDisponiveis.Value > 0;
                }
            }, "Erro na tentativa de validar vagas da turma.");
        }

        public Task<bool> AlunoEstaAtivo(int alunoId)
        {
            return ExecutarAsync(async () =>
            {
                const string sql = "SELECT Ativo FROM Aluno WHERE Id = @AlunoId;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var ativo = await conexao.QuerySingleOrDefaultAsync<bool?>(sql, new { AlunoId = alunoId });
                    if (ativo == null)
                        throw new NotFoundException("Aluno não encontrado.");

                    return ativo.Value;
                }
            }, "Erro na tentativa de validar situação do aluno.");
        }

        public Task<bool> AlunoJaPossuiVinculoComTurma(int alunoId, int turmaId)
        {
            return ExecutarAsync(async () =>
            {
                const string sql = @"
                    SELECT COUNT(1)
                    FROM Matricula
                    WHERE AlunoId = @AlunoId AND TurmaId = @TurmaId;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var quantidade = await conexao.ExecuteScalarAsync<int>(sql, new { AlunoId = alunoId, TurmaId = turmaId });
                    return quantidade > 0;
                }
            }, "Erro na tentativa de validar vínculo entre aluno e turma.");
        }

        public Task<MatriculaResponseDto> Matricular(MatriculaRequestDto matricula)
        {
            return ExecutarAsync(async () =>
            {
                using (var conexao = _connectionFactory.CreateConnection())
                {
                    conexao.Open();

                    // ==== Lógica da transação de matrícula ====
                    // Esta operação precisa gravar DUAS coisas de forma atômica:a linha Matricula e o decremento de VagasDisponiveis em Turma.
                    // Se qualquer uma das duas falhar, a outra NÃO pode ficar gravada sozinha, senão a turma
                    // fica com contagem de vagas divergente do número real de matrículas.
                    // Por isso as duas escritas usam a MESMA conexão e a MESMA transação
                    // (conexao.BeginTransaction()): só chamamos transacao.Commit() depois que
                    // ambos os comandos rodaram com sucesso e qualquer exceção no meio do caminho
                    // cai no catch, que desfaz tudo com transacao.Rollback() e relança o erro
                    // (nada fica persistido).
                    //
                    // Por que o UPDATE repete "AND VagasDisponiveis > 0" mesmo já tendo validado
                    // isso antes (TurmaTemVagaDisponivel), na camada de serviço? Porque entre aquela
                    // leitura e esta escrita outra requisição concorrente pode ter matriculado o
                    // último aluno na mesma turma (clássica condição de corrida "check-then-act").
                    // Repetir a condição no WHERE faz o próprio banco garantir, de forma atômica,
                    // que só decrementamos se a vaga realmente ainda existir no exato instante da
                    // escrita. Se 0 linhas forem afetadas, é sinal de que a vaga acabou de ser
                    // tomada por outra transação — nesse caso desfazemos o INSERT já feito (rollback)
                    // e retornamos o mesmo erro de conflito (409) que o usuário receberia se a
                    // validação inicial já tivesse pego a turma sem vaga.
                    using (var transacao = conexao.BeginTransaction())
                    {
                        try
                        {
                            var entidade = new Matricula
                            {
                                AlunoId = matricula.AlunoId,
                                TurmaId = matricula.TurmaId,
                                DataMatricula = DateTime.Now
                            };

                            const string sqlInserir = @"
                                INSERT INTO Matricula (AlunoId, TurmaId, DataMatricula)
                                OUTPUT INSERTED.Id
                                VALUES (@AlunoId, @TurmaId, @DataMatricula);";

                            entidade.Id = await conexao.ExecuteScalarAsync<int>(sqlInserir, entidade, transacao);

                            const string sqlDecrementarVaga = @"
                                UPDATE Turma
                                SET VagasDisponiveis = VagasDisponiveis - 1
                                WHERE Id = @TurmaId AND VagasDisponiveis > 0;";

                            var linhasAfetadas = await conexao.ExecuteAsync(sqlDecrementarVaga, new { entidade.TurmaId }, transacao);

                            if (linhasAfetadas == 0)
                                throw new ConflictException("A turma não possui vagas disponíveis.");

                            transacao.Commit();

                            return MapearParaResponseDto(entidade);
                        }
                        catch
                        {
                            transacao.Rollback();
                            throw;
                        }
                    }
                }
            }, "Erro na tentativa de matricular aluno.");
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

        private static MatriculaResponseDto MapearParaResponseDto(Matricula matricula)
        {
            return new MatriculaResponseDto
            {
                Id = matricula.Id,
                AlunoId = matricula.AlunoId,
                TurmaId = matricula.TurmaId,
                DataMatricula = matricula.DataMatricula
            };
        }

        #endregion
    }
}
