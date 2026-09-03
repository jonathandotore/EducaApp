using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Domain.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducaWebApi.Service.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly IRelatorioRepository _relatorioRepository;

        public RelatorioService(IRelatorioRepository relatorioRepository)
        {
            _relatorioRepository = relatorioRepository;
        }

        public async Task<MainResponse<List<RelatorioTurmaAlunosResponseDto>>> ObterAlunosPorTurma()
        {
            var relatorio = await _relatorioRepository.ObterAlunosPorTurma();

            return MainResponse<List<RelatorioTurmaAlunosResponseDto>>.Sucesso(relatorio);
        }
    }
}
