using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IAlunoService
    {
        Task<MainResponse<List<AlunoResponseDto>>> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina);
        Task<MainResponse<AlunoResponseDto>> ObterPorId(int id);
        Task<MainResponse<AlunoResponseDto>> Criar(AlunoRequestDto aluno);
        Task<MainResponse<AlunoResponseDto>> Atualizar(int id, AlunoRequestDto aluno);
        Task<MainResponse<AlunoResponseDto>> Inativar(int id);
    }
}
