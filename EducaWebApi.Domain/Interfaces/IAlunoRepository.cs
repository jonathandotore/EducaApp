using EducaWebApi.Domain.Dtos;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IAlunoRepository
    {
        Task<ResultadosPaginados<AlunoResponseDto>> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina);
        Task<AlunoResponseDto> ObterPorId(int id);
        Task<AlunoResponseDto> Criar(AlunoRequestDto aluno);
        Task<AlunoResponseDto> Atualizar(int id, AlunoRequestDto aluno);
        Task<AlunoResponseDto> Inativar(int id);
    }
}
