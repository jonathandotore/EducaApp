using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Entities;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IAlunoRepository
    {
        Task<ResultadosPaginados<AlunoResponseDto>> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina);
        Task<AlunoResponseDto> ObterPorId(int id);
        Task<Aluno> Criar(Aluno aluno);
        Task<Aluno> Atualizar(int id, Aluno aluno);
        Task<Aluno> Inativar(int id);
    }
}
