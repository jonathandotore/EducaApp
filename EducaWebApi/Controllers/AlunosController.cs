using EducaWebApi.Domain.Entities;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Models;
using System.Collections.Generic;
using System.Net;
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

        [HttpGet, Route("")]
        public IHttpActionResult Listar(string nome = null, int pagina = 1, int tamanhoPagina = 10)
        {
            var resultado = _alunoService.ListarPaginado(nome, pagina, tamanhoPagina);
            var resposta = MainResponse<List<Aluno>>
                .Sucesso(resultado.Itens)
                .ComPaginacao(resultado.Pagina, resultado.TamanhoPagina, resultado.TotalRegistros);

            return Content(resposta.StatusCode, resposta);
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult ObterPorId(int id)
        {
            var aluno = _alunoService.ObterPorId(id);
            var resposta = MainResponse<Aluno>.Sucesso(aluno);
            
            return Content(resposta.StatusCode, resposta);
        }

        [HttpPost, Route("")]
        public IHttpActionResult Criar([FromBody] Aluno aluno)
        {
            var criado = _alunoService.Criar(aluno);
            var resposta = MainResponse<Aluno>.Sucesso(criado, HttpStatusCode.Created);
            
            return Content(resposta.StatusCode, resposta);
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Atualizar(int id, [FromBody] Aluno aluno)
        {
            var atualizado = _alunoService.Atualizar(id, aluno);
            var resposta = MainResponse<Aluno>.Sucesso(atualizado);
            
            return Content(resposta.StatusCode, resposta);
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Desativar(int id)
        {
            _alunoService.Inativar(id);
            var resposta = MainResponse<object>.Sucesso(null, HttpStatusCode.OK, "Cadastro inativado com sucesso.");
            
            return Content(resposta.StatusCode, resposta);
        }
    }
}