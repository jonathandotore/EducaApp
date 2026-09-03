using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Domain.Responses;
using System;
using System.Net;
using System.Threading.Tasks;

namespace EducaWebApi.Service.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly IMatriculaRepository _matriculaRepository;

        public MatriculaService(IMatriculaRepository matriculaRepository)
        {
            _matriculaRepository = matriculaRepository;
        }

        public async Task<MainResponse<MatriculaResponseDto>> Matricular(MatriculaRequestDto matricula)
        {
            var erroDados = ValidarDados(matricula);
            if (erroDados != null)
                return erroDados;

            return await ExecutarAsync(async () =>
            {
                var turmaComVaga = await _matriculaRepository.TurmaTemVagaDisponivel(matricula.TurmaId);
                if (!turmaComVaga)
                    throw new ConflictException("A turma não possui vagas disponíveis.");

                var alunoAtivo = await _matriculaRepository.AlunoEstaAtivo(matricula.AlunoId);
                if (!alunoAtivo)
                    throw new ConflictException("O aluno está inativo e não pode ser matriculado.");

                var jaMatriculado = await _matriculaRepository.AlunoJaPossuiVinculoComTurma(matricula.AlunoId, matricula.TurmaId);
                if (jaMatriculado)
                    throw new ConflictException("O aluno já está matriculado nesta turma.");

                return await _matriculaRepository.Matricular(matricula);
            }, HttpStatusCode.Created);
        }

        #region Métodos privados

        private static async Task<MainResponse<T>> ExecutarAsync<T>(Func<Task<T>> operacao, HttpStatusCode statusCodeSucesso = HttpStatusCode.OK)
        {
            try
            {
                var resultado = await operacao();
                return MainResponse<T>.Sucesso(resultado, statusCodeSucesso);
            }
            catch (NotFoundException ex)
            {
                return MainResponse<T>.Erro(ex.Message, HttpStatusCode.NotFound);
            }
            catch (ConflictException ex)
            {
                return MainResponse<T>.Erro(ex.Message, HttpStatusCode.Conflict);
            }
        }

        private static MainResponse<MatriculaResponseDto> ValidarDados(MatriculaRequestDto matricula)
        {
            if (matricula == null)
                return MainResponse<MatriculaResponseDto>.Erro("Os dados da matrícula são obrigatórios.", HttpStatusCode.BadRequest);

            if (matricula.AlunoId <= 0)
                return MainResponse<MatriculaResponseDto>.Erro("O id do aluno deve ser maior que zero.", HttpStatusCode.BadRequest);

            if (matricula.TurmaId <= 0)
                return MainResponse<MatriculaResponseDto>.Erro("O id da turma deve ser maior que zero.", HttpStatusCode.BadRequest);

            return null;
        }

        #endregion
    }
}
