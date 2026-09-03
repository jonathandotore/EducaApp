using EducaWebApi.Domain.Dtos;
using EducaWebApi.Domain.Responses;
using System.Threading.Tasks;

namespace EducaWebApi.Domain.Interfaces
{
    public interface IMatriculaService
    {
        Task<MainResponse<MatriculaResponseDto>> Matricular(MatriculaRequestDto matricula);
    }
}
