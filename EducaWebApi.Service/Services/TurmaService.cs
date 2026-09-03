using EducaWebApi.Domain.Constants;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Domain.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EducaWebApi.Service.Services
{
    public class TurmaService : ITurmaService
    {
        //Apenas um minuto para testes locais
        private static readonly TimeSpan TempoExpiracaoCache = TimeSpan.FromMinutes(1);
        private readonly ITurmaRepository _turmaRepository;
        private readonly ICacheService _cacheService;

        public TurmaService(ITurmaRepository turmaRepository, ICacheService cacheService)
        {
            _turmaRepository = turmaRepository;
            _cacheService = cacheService;
        }

        public async Task<MainResponse<List<TurmaResponseDto>>> Listar()
        {
            // Cache-aside: primeiro tenta servir a lista direto do Redis (cache hit). Só se não
            // houver nada em cache (primeira chamada, expirou ou foi invalidado por uma matrícula
            // ver MatriculaService.Matricular) é que consultamos o banco e recacheamos o
            // resultado para as próximas chamadas.
            var turmasEmCache = await _cacheService.ObterAsync<List<TurmaResponseDto>>(CacheKeys.ListaTurmas);
            if (turmasEmCache != null)
                return MainResponse<List<TurmaResponseDto>>.Sucesso(turmasEmCache);

            var turmas = await _turmaRepository.Listar();
            await _cacheService.DefinirAsync(CacheKeys.ListaTurmas, turmas, TempoExpiracaoCache);

            return MainResponse<List<TurmaResponseDto>>.Sucesso(turmas);
        }
    }
}
