using Dapper;
using EducaWebApi.Data.Connection;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace EducaWebApi.Data.Repositories
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly IConnectionFactory _connectionFactory;

        public RelatorioRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public Task<List<RelatorioTurmaAlunosResponseDto>> ObterAlunosPorTurma()
        {
            return ExecutarAsync(async () =>
            {
                // ==== Lógica por trás da montagem da query ====
                //     O relatório precisa, PARA CADA TURMA: nome, quantidade de alunos matriculados,
                //     vagas restantes e a lista de nomes dos alunos — já agrupado por turma. Em vez de
                //     trazer uma linha por matrícula para o C# e agrupar/contar/concatenar em memória
                //     (o que exigiria carregar todas as turmas, matrículas e alunos na aplicação só
                //     para somar e montar listas), a agregação inteira é feita pelo SQL Server:
                //
                //   - LEFT JOIN de Turma com Matricula e com Aluno: é LEFT (e não INNER) para que
                //     turmas sem nenhum aluno matriculado também apareçam no relatório — com
                //     quantidade 0 e lista de nomes vazia — em vez de simplesmente sumirem do
                //     resultado, que é o que um INNER JOIN faria.
                //
                //   - GROUP BY t.Id, t.Nome, t.VagasDisponiveis: gera uma única linha de resultado
                //     por turma. Nome e VagasDisponiveis entram no GROUP BY junto com o Id porque
                //     dependem funcionalmente dele (cada turma tem exatamente um valor de cada);
                //     sem isso o SQL Server não aceitaria selecioná-los fora de uma função de
                //     agregação.
                //
                //   - COUNT(m.AlunoId): conta quantas matrículas (não nulas) caíram em cada grupo.
                //     Por causa do LEFT JOIN, uma turma sem matrícula tem m.AlunoId nulo nessa
                //     linha, e COUNT(coluna) ignora nulos — resultando corretamente em 0. Usar
                //     COUNT(*) aqui seria um erro clássico: contaria 1 (a própria linha do LEFT
                //     JOIN) mesmo quando não há aluno nenhum.
                //
                //
                //     ESSA PARTE DO STRING FOI GERADA COMPLETAMENTE COM AJUDA DA IA (NÃO CONHECIA ESSA FUNCIONALIDADE DO BANCO DE DADOS)
                //     Requer SQL Server 2017+ (ou Azure SQL) por causa do STRING_AGG.
                //
                //   - STRING_AGG(a.Nome, ', ') WITHIN GROUP (ORDER BY a.Nome): concatena os nomes
                //     de todos os alunos de cada grupo em uma única string, já ordenada
                //     alfabeticamente, inteiramente dentro do banco. O C# apenas faz o Split(", ")
                //     dessa string pronta para devolver como lista no DTO de resposta — não há
                //     nenhum agrupamento, contagem ou concatenação acontecendo em memória, só a
                //     formatação de um resultado que o SQL já calculou. 

                const string sql = @"
                    SELECT
                        t.Nome AS NomeTurma,
                        COUNT(m.AlunoId) AS QuantidadeAlunos,
                        t.VagasDisponiveis AS VagasRestantes,
                        STRING_AGG(a.Nome, ', ') WITHIN GROUP (ORDER BY a.Nome) AS NomesAlunos
                    FROM Turma t
                    LEFT JOIN Matricula m ON m.TurmaId = t.Id
                    LEFT JOIN Aluno a ON a.Id = m.AlunoId
                    GROUP BY t.Id, t.Nome, t.VagasDisponiveis
                    ORDER BY t.Nome;";

                using (var conexao = _connectionFactory.CreateConnection())
                {
                    var linhas = await conexao.QueryAsync<RelatorioTurmaAlunosLinha>(sql);

                    return linhas.Select(MapearParaResponseDto).ToList();
                }
            }, "Erro na tentativa de gerar relatório de alunos por turma.");
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

        private static RelatorioTurmaAlunosResponseDto MapearParaResponseDto(RelatorioTurmaAlunosLinha linha)
        {
            return new RelatorioTurmaAlunosResponseDto
            {
                NomeTurma = linha.NomeTurma,
                QuantidadeAlunos = linha.QuantidadeAlunos,
                VagasRestantes = linha.VagasRestantes,
                NomesAlunos = string.IsNullOrEmpty(linha.NomesAlunos)
                    ? new List<string>()
                    : linha.NomesAlunos.Split(new[] { ", " }, StringSplitOptions.None).ToList()
            };
        }

        private class RelatorioTurmaAlunosLinha
        {
            public string NomeTurma { get; set; }
            public int QuantidadeAlunos { get; set; }
            public int VagasRestantes { get; set; }
            public string NomesAlunos { get; set; }
        }

        #endregion
    }
}
