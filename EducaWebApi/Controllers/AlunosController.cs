using EducaWebApi.Data.Connection;
using EducaWebApi.Data.Repositories;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Entities;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Models;
using EducaWebApi.Service.Services;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace EducaWebApi.Controllers
{
    [RoutePrefix("api/alunos")]
    public class AlunosController : ApiController
    {
        private readonly IAlunoService _alunoService;

        public AlunosController(IAlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        public AlunosController() : this(new AlunoService(new AlunoRepository(new SqlConnectionFactory()))) { }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Listar(string nome = null, int pagina = 1, int tamanhoPagina = 10)
        {
            var resultado = await _alunoService.ListarPaginado(nome, pagina, tamanhoPagina);
            var resposta = MainResponse<List<AlunoResponseDto>>
                .Sucesso(resultado.Itens)
                .ComPaginacao(resultado.Pagina, resultado.TamanhoPagina, resultado.TotalRegistros);

            return Content(resposta.StatusCode, resposta);
        }

        [HttpGet, Route("{id:int}")]
        public async Task<IHttpActionResult> ObterPorId(int id)
        {
            var aluno = await _alunoService.ObterPorId(id);
            var resposta = MainResponse<AlunoResponseDto>.Sucesso(aluno);
            
            return Content(resposta.StatusCode, resposta);
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Criar([FromBody] Aluno aluno)
        {
            var criado = await _alunoService.Criar(aluno);
            var resposta = MainResponse<AlunoResponseDto>.Sucesso(criado, HttpStatusCode.Created);
            
            return Content(resposta.StatusCode, resposta);
        }

        [HttpPut, Route("{id:int}")]
        public async Task<IHttpActionResult> Atualizar(int id, [FromBody] Aluno aluno)
        {
            var atualizado = await _alunoService.Atualizar(id, aluno);
            var resposta = MainResponse<AlunoResponseDto>.Sucesso(atualizado);
            
            return Content(resposta.StatusCode, resposta);
        }

        [HttpDelete, Route("{id:int}")]
        public async Task<IHttpActionResult> Desativar(int id)
        {
            var inativo = await _alunoService.Inativar(id);
            var resposta = MainResponse<object>.Sucesso(null, HttpStatusCode.OK, "Cadastro inativado com sucesso.");
            
            return Content(resposta.StatusCode, resposta);
        }
    }
}