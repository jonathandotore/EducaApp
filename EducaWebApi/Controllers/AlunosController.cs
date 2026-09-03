using EducaWebApi.Data.Connection;
using EducaWebApi.Data.Repositories;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Service.Services;
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
            var resposta = await _alunoService.ListarPaginado(nome, pagina, tamanhoPagina);
            return Content(resposta.StatusCode, resposta);
        }

        [HttpGet, Route("{id:int}")]
        public async Task<IHttpActionResult> ObterPorId(int id)
        {
            var resposta = await _alunoService.ObterPorId(id);
            return Content(resposta.StatusCode, resposta);
        }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Criar([FromBody] AlunoRequestDto aluno)
        {
            var resposta = await _alunoService.Criar(aluno);
            return Content(resposta.StatusCode, resposta);
        }

        [HttpPut, Route("{id:int}")]
        public async Task<IHttpActionResult> Atualizar(int id, [FromBody] AlunoRequestDto aluno)
        {
            var resposta = await _alunoService.Atualizar(id, aluno);
            return Content(resposta.StatusCode, resposta);
        }

        [HttpDelete, Route("{id:int}")]
        public async Task<IHttpActionResult> Desativar(int id)
        {
            var resposta = await _alunoService.Inativar(id);
            return Content(resposta.StatusCode, resposta);
        }
    }
}
