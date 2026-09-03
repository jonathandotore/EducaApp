using System;

namespace EducaWebApi.Domain.Dtos
{
    public class AlunoRequestDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DataNascimento { get; set; }
    }
}
