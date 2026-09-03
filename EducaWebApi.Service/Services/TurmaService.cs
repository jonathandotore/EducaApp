using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Domain.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducaWebApi.Service.Services
{
    public class TurmaService : ITurmaService
    {
        private readonly ITurmaRepository _turmaRepository;

        public TurmaService(ITurmaRepository turmaRepository)
        {
            _turmaRepository = turmaRepository;
        }

        public async Task<MainResponse<List<TurmaResponseDto>>> Listar()
        {
            var turmas = await _turmaRepository.Listar();

            return MainResponse<List<TurmaResponseDto>>.Sucesso(turmas);
        }
    }
}
