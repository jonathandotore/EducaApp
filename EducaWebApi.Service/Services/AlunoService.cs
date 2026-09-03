using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Exceptions;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Domain.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EducaWebApi.Service.Services
{
    public class AlunoService : IAlunoService
    {
        private const int TamanhoPaginaMaximo = 100;

        private readonly IAlunoRepository _alunoRepository;

        public AlunoService(IAlunoRepository alunoRepository)
        {
            _alunoRepository = alunoRepository;
        }

        public async Task<MainResponse<List<AlunoResponseDto>>> ListarPaginado(string nomeFiltro, int pagina, int tamanhoPagina)
        {
            if (pagina < 1)
                return MainResponse<List<AlunoResponseDto>>.Erro("A página deve ser maior ou igual a 1.", HttpStatusCode.BadRequest);

            if (tamanhoPagina < 1 || tamanhoPagina > TamanhoPaginaMaximo)
                return MainResponse<List<AlunoResponseDto>>.Erro($"O tamanho da página deve estar entre 1 e {TamanhoPaginaMaximo}.", HttpStatusCode.BadRequest);

            var resultado = await _alunoRepository.ListarPaginado(nomeFiltro, pagina, tamanhoPagina);

            return MainResponse<List<AlunoResponseDto>>
                .Sucesso(resultado.Itens)
                .ComPaginacao(resultado.Pagina, resultado.TamanhoPagina, resultado.TotalRegistros);
        }

        public async Task<MainResponse<AlunoResponseDto>> ObterPorId(int id)
        {
            var erroId = ValidarId(id);
            if (erroId != null)
                return erroId;

            return await ExecutarAsync(() => _alunoRepository.ObterPorId(id));
        }

        public async Task<MainResponse<AlunoResponseDto>> Criar(AlunoRequestDto aluno)
        {
            var erroDados = ValidarDadosAluno(aluno);
            if (erroDados != null)
                return erroDados;

            return await ExecutarAsync(() => _alunoRepository.Criar(aluno), HttpStatusCode.Created);
        }

        public async Task<MainResponse<AlunoResponseDto>> Atualizar(int id, AlunoRequestDto aluno)
        {
            var erroId = ValidarId(id);
            if (erroId != null)
                return erroId;

            var erroDados = ValidarDadosAluno(aluno);
            if (erroDados != null)
                return erroDados;

            return await ExecutarAsync(() => _alunoRepository.Atualizar(id, aluno));
        }

        public async Task<MainResponse<AlunoResponseDto>> Inativar(int id)
        {
            var erroId = ValidarId(id);
            if (erroId != null)
                return erroId;

            return await ExecutarAsync(() => _alunoRepository.Inativar(id), HttpStatusCode.OK, "Cadastro inativado com sucesso.");
        }

        #region Métodos privados

        /// <summary>
        /// Executa uma chamada ao repositório e traduz NotFoundException/ConflictException
        /// (id inexistente ou conflito de dados no banco) para o MainResponse correspondente.
        /// </summary>
        private static async Task<MainResponse<T>> ExecutarAsync<T>(Func<Task<T>> operacao, HttpStatusCode statusCodeSucesso = HttpStatusCode.OK, string mensagemSucesso = null)
        {
            try
            {
                var resultado = await operacao();
                return MainResponse<T>.Sucesso(resultado, statusCodeSucesso, mensagemSucesso);
            }
            catch (NotFoundException ex)
            {
                return MainResponse<T>.Erro(ex.Message, HttpStatusCode.NotFound);
            }
            catch (ConflictException ex)
            {
                return MainResponse<T>.Erro(ex.Message, HttpStatusCode.Conflict);
            }
        }

        private static MainResponse<AlunoResponseDto> ValidarId(int id)
        {
            if (id <= 0)
                return MainResponse<AlunoResponseDto>.Erro("O id do aluno deve ser maior que zero.", HttpStatusCode.BadRequest);

            return null;
        }

        private static MainResponse<AlunoResponseDto> ValidarDadosAluno(AlunoRequestDto aluno)
        {
            if (aluno == null)
                return MainResponse<AlunoResponseDto>.Erro("Os dados do aluno são obrigatórios.", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(aluno.Nome))
                return MainResponse<AlunoResponseDto>.Erro("O nome do aluno é obrigatório.", HttpStatusCode.BadRequest);

            if (string.IsNullOrWhiteSpace(aluno.Email) || !EmailValido(aluno.Email))
                return MainResponse<AlunoResponseDto>.Erro("O e-mail informado é inválido.", HttpStatusCode.BadRequest);

            if (aluno.DataNascimento == default(DateTime) || aluno.DataNascimento.Date > DateTime.Now.Date)
                return MainResponse<AlunoResponseDto>.Erro("A data de nascimento informada é inválida.", HttpStatusCode.BadRequest);

            return null;
        }

        private static bool EmailValido(string email)
        {
            try
            {
                return new MailAddress(email).Address == email;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        #endregion
    }
}
