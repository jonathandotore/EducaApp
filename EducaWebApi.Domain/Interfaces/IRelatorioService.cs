using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IRelatorioService
    {
        Task<MainResponse<List<RelatorioTurmaAlunosResponseDto>>> ObterAlunosPorTurma();
    }
}
