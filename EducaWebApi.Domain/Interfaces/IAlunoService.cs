using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Entities;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IAlunoService
    {
        ResultadosPaginados<Aluno> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina);
        Aluno ObterPorId(int id);
        Aluno Criar(Aluno aluno);
        Aluno Atualizar(int id, Aluno aluno);
        void Inativar(int id);
    }
}
