using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Entities;
using EducaWebApi.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace EducaWebApi.Service.Services
{
    public class AlunoService : IAlunoService
    {
        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }

        public async Task<ResultadosPaginados<AlunoResponseDto>> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina)
        {
            try
            {
                if (pagina < 1)
                    pagina = 1;
                if (tamanhoPagina < 1 || tamanhoPagina > 100)
                    tamanhoPagina = 10;

                var alunos = await _alunoRepository.ListarPaginado(nomeFiltro, pagina, tamanhoPagina);
                return alunos;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            
        }
        public async Task<AlunoResponseDto> ObterPorId(int id)
        {
            try
            {
                if (id <= 0) return null;

                return await _alunoRepository.ObterPorId(id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<AlunoResponseDto> Criar(Aluno aluno) => throw new NotImplementedException();
        public async Task<AlunoResponseDto> Atualizar(int id, Aluno aluno) => throw new NotImplementedException();
        public async Task<AlunoResponseDto> Inativar(int id) => throw new NotImplementedException();
    }
}
