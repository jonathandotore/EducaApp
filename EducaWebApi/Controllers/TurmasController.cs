using EducaWebApi.Data.Connection;
using EducaWebApi.Data.Repositories;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Service.Services;
using System.Threading.Tasks;
using System.Web.Http;

namespace EducaWebApi.Controllers
{
    [RoutePrefix("api/turmas")]
    public class TurmasController : ApiController
    {
        private readonly ITurmaService _turmaService;

        public TurmasController(ITurmaService turmaService)
        {
            _turmaService = turmaService;
        }

        public TurmasController() : this(new TurmaService(new TurmaRepository(new SqlConnectionFactory()))) { }

        [HttpGet, Route("")]
        public async Task<IHttpActionResult> Listar()
        {
            var resposta = await _turmaService.Listar();
            return Content(resposta.StatusCode, resposta);
        }
    }
}
