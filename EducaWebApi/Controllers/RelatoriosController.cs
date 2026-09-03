using EducaWebApi.Data.Connection;
using EducaWebApi.Data.Repositories;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Service.Services;
using System.Threading.Tasks;
using System.Web.Http;

namespace EducaWebApi.Controllers
{
    [RoutePrefix("api/relatorios")]
    public class RelatoriosController : ApiController
    {
        private readonly IRelatorioService _relatorioService;

        public RelatoriosController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        public RelatoriosController() : this(new RelatorioService(new RelatorioRepository(new SqlConnectionFactory()))) { }

        [HttpGet, Route("alunos-por-turma")]
        public async Task<IHttpActionResult> AlunosPorTurma()
        {
            var resposta = await _relatorioService.ObterAlunosPorTurma();
            return Content(resposta.StatusCode, resposta);
        }
    }
}
