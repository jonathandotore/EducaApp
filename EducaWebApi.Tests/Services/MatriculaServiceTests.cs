using EducaWebApi.Domain.Constants;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Service.Services;
using Moq;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace EducaWebApi.Tests.Services
{
    public class MatriculaServiceTests
    {
        private readonly Mock<IMatriculaRepository> _matriculaRepositoryMock = new Mock<IMatriculaRepository>();
        private readonly Mock<ICacheService> _cacheServiceMock = new Mock<ICacheService>();
        private readonly MatriculaService _service;

        public MatriculaServiceTests()
        {
            _service = new MatriculaService(_matriculaRepositoryMock.Object, _cacheServiceMock.Object);
        }

        [Fact]
        public async Task Matricular_DadosNulos_RetornaBadRequestSemChamarRepositorio()
        {
            var resultado = await _service.Matricular(null);

            Assert.True(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            _matriculaRepositoryMock.Verify(r => r.TurmaTemVagaDisponivel(It.IsAny<int>()), Times.Never);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        public async Task Matricular_IdsInvalidos_RetornaBadRequestSemChamarRepositorio(int alunoId, int turmaId)
        {
            var resultado = await _service.Matricular(new MatriculaRequestDto { AlunoId = alunoId, TurmaId = turmaId });

            Assert.True(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.BadRequest, resultado.StatusCode);
            _matriculaRepositoryMock.Verify(r => r.TurmaTemVagaDisponivel(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task Matricular_TurmaSemVaga_RetornaConflictENaoGravaMatriculaNemInvalidaCache()
        {
            _matriculaRepositoryMock.Setup(r => r.TurmaTemVagaDisponivel(2)).ReturnsAsync(false);

            var resultado = await _service.Matricular(new MatriculaRequestDto { AlunoId = 1, TurmaId = 2 });

            Assert.True(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.Conflict, resultado.StatusCode);
            Assert.Equal("A turma não possui vagas disponíveis.", resultado.Mensagem);
            _matriculaRepositoryMock.Verify(r => r.AlunoEstaAtivo(It.IsAny<int>()), Times.Never);
            _matriculaRepositoryMock.Verify(r => r.Matricular(It.IsAny<MatriculaRequestDto>()), Times.Never);
            _cacheServiceMock.Verify(c => c.RemoverAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Matricular_AlunoInativo_RetornaConflictENaoGravaMatricula()
        {
            _matriculaRepositoryMock.Setup(r => r.TurmaTemVagaDisponivel(2)).ReturnsAsync(true);
            _matriculaRepositoryMock.Setup(r => r.AlunoEstaAtivo(1)).ReturnsAsync(false);

            var resultado = await _service.Matricular(new MatriculaRequestDto { AlunoId = 1, TurmaId = 2 });

            Assert.True(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.Conflict, resultado.StatusCode);
            Assert.Equal("O aluno está inativo e não pode ser matriculado.", resultado.Mensagem);
            _matriculaRepositoryMock.Verify(r => r.AlunoJaPossuiVinculoComTurma(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _matriculaRepositoryMock.Verify(r => r.Matricular(It.IsAny<MatriculaRequestDto>()), Times.Never);
            _cacheServiceMock.Verify(c => c.RemoverAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Matricular_AlunoJaMatriculadoNaTurma_RetornaConflictENaoGravaMatricula()
        {
            _matriculaRepositoryMock.Setup(r => r.TurmaTemVagaDisponivel(2)).ReturnsAsync(true);
            _matriculaRepositoryMock.Setup(r => r.AlunoEstaAtivo(1)).ReturnsAsync(true);
            _matriculaRepositoryMock.Setup(r => r.AlunoJaPossuiVinculoComTurma(1, 2)).ReturnsAsync(true);

            var resultado = await _service.Matricular(new MatriculaRequestDto { AlunoId = 1, TurmaId = 2 });

            Assert.True(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.Conflict, resultado.StatusCode);
            Assert.Equal("O aluno já está matriculado nesta turma.", resultado.Mensagem);
            _matriculaRepositoryMock.Verify(r => r.Matricular(It.IsAny<MatriculaRequestDto>()), Times.Never);
            _cacheServiceMock.Verify(c => c.RemoverAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Matricular_TurmaInexistente_RetornaNotFound()
        {
            _matriculaRepositoryMock
                .Setup(r => r.TurmaTemVagaDisponivel(99))
                .ThrowsAsync(new NotFoundException("Turma não encontrada."));

            var resultado = await _service.Matricular(new MatriculaRequestDto { AlunoId = 1, TurmaId = 99 });

            Assert.True(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.NotFound, resultado.StatusCode);
            Assert.Equal("Turma não encontrada.", resultado.Mensagem);
            _matriculaRepositoryMock.Verify(r => r.Matricular(It.IsAny<MatriculaRequestDto>()), Times.Never);
            _cacheServiceMock.Verify(c => c.RemoverAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Matricular_TodasRegrasOk_GravaMatriculaInvalidaCacheERetornaCreated()
        {
            var request = new MatriculaRequestDto { AlunoId = 1, TurmaId = 2 };
            var respostaEsperada = new MatriculaResponseDto { Id = 10, AlunoId = 1, TurmaId = 2, DataMatricula = DateTime.Now };

            _matriculaRepositoryMock.Setup(r => r.TurmaTemVagaDisponivel(2)).ReturnsAsync(true);
            _matriculaRepositoryMock.Setup(r => r.AlunoEstaAtivo(1)).ReturnsAsync(true);
            _matriculaRepositoryMock.Setup(r => r.AlunoJaPossuiVinculoComTurma(1, 2)).ReturnsAsync(false);
            _matriculaRepositoryMock.Setup(r => r.Matricular(request)).ReturnsAsync(respostaEsperada);

            var resultado = await _service.Matricular(request);

            Assert.False(resultado.ContemErros);
            Assert.Equal(HttpStatusCode.Created, resultado.StatusCode);
            Assert.Equal(respostaEsperada, resultado.Dados);
            _matriculaRepositoryMock.Verify(r => r.Matricular(request), Times.Once);
            _cacheServiceMock.Verify(c => c.RemoverAsync(CacheKeys.ListaTurmas), Times.Once);
        }
    }
}
