using EducaWebApi.Domain.Constants;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Service.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace EducaWebApi.Tests.Services
{
    public class TurmaServiceTests
    {
        private readonly Mock<ITurmaRepository> _turmaRepositoryMock = new Mock<ITurmaRepository>();
        private readonly Mock<ICacheService> _cacheServiceMock = new Mock<ICacheService>();
        private readonly TurmaService _service;

        public TurmaServiceTests()
        {
            _service = new TurmaService(_turmaRepositoryMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Listar_ComCacheHit_NaoConsultaRepositorioERetornaDadosDoCache()
        {
            var turmasEmCache = new List<TurmaResponseDto>
            {
                new TurmaResponseDto 
                { 
                    Id = 1, 
                    Nome = "Turma A", 
                    VagasTotal = 30, 
                    VagasDisponiveis = 10 
                }
            };

            _cacheServiceMock
                .Setup(c => c.ObterAsync<List<TurmaResponseDto>>(CacheKeys.ListaTurmas))
                .ReturnsAsync(turmasEmCache);

            var resultado = await _service.Listar();

            Assert.False(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.OK, resultado.StatusCode);
            Assert.Equal(turmasEmCache, resultado.Dados);
            _turmaRepositoryMock.Verify(r => r.Listar(), Times.Never);
            _cacheServiceMock.Verify(c => c.DefinirAsync(It.IsAny<string>(), It.IsAny<List<TurmaResponseDto>>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task Listar_ComCacheMiss_ConsultaRepositorioEGravaResultadoNoCache()
        {
            var turmasDoBanco = new List<TurmaResponseDto>
            {
                new TurmaResponseDto 
                { 
                    Id = 2, 
                    Nome = "Turma B", 
                    VagasTotal = 20, 
                    VagasDisponiveis = 5 
                }
            };

            _cacheServiceMock
                .Setup(c => c.ObterAsync<List<TurmaResponseDto>>(CacheKeys.ListaTurmas))
                .ReturnsAsync((List<TurmaResponseDto>)null);

            _turmaRepositoryMock
                .Setup(r => r.Listar())
                .ReturnsAsync(turmasDoBanco);

            var resultado = await _service.Listar();

            Assert.False(resultado.ContemErros);
            Assert.Equal(turmasDoBanco, resultado.Dados);
            _turmaRepositoryMock.Verify(r => r.Listar(), Times.Once);
            _cacheServiceMock.Verify(c => c.DefinirAsync(CacheKeys.ListaTurmas, turmasDoBanco, It.IsAny<TimeSpan>()), Times.Once);
        }
    }
}
