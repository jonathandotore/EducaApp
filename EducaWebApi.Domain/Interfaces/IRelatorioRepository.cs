using EducaWebApi.Domain.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IRelatorioRepository
    {
        Task<List<RelatorioTurmaAlunosResponseDto>> ObterAlunosPorTurma();
    }
}
