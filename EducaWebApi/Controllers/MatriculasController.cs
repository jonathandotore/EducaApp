using EducaWebApi.Data.Cache;
using EducaWebApi.Data.Connection;
using EducaWebApi.Data.Repositories;
using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Interfaces;
using EducaWebApi.Service.Services;
using System.Threading.Tasks;
using System.Web.Http;

namespace EducaWebApi.Controllers
{
    [RoutePrefix("api/matriculas")]
    public class MatriculasController : ApiController
    {
        private readonly IMatriculaService _matriculaService;

        public MatriculasController(IMatriculaService matriculaService)
        {
            _matriculaService = matriculaService;
        }

        public MatriculasController() : this(new MatriculaService(new MatriculaRepository(new SqlConnectionFactory()), new RedisCacheService())) { }

        [HttpPost, Route("")]
        public async Task<IHttpActionResult> Matricular([FromBody] MatriculaRequestDto matricula)
        {
            var resposta = await _matriculaService.Matricular(matricula);
            return Content(resposta.StatusCode, resposta);
        }
    }
}
