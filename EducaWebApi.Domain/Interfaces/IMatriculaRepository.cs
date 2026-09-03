using EducaWebApi.Domain.Dtos;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IMatriculaRepository
    {
        Task<bool> TurmaTemVagaDisponivel(int turmaId);
        Task<bool> AlunoEstaAtivo(int alunoId);
        Task<bool> AlunoJaPossuiVinculoComTurma(int alunoId, int turmaId);
        Task<MatriculaResponseDto> Matricular(MatriculaRequestDto matricula);
    }
}
