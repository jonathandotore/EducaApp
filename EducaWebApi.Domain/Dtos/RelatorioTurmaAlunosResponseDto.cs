using System.Collections.Generic;

namespace EducaWebApi.Domain.Dtos
{
    public class RelatorioTurmaAlunosResponseDto
    {
        public string NomeTurma { get; set; } = string.Empty;
        public int QuantidadeAlunos { get; set; }
        public int VagasRestantes { get; set; }
        public List<string> NomesAlunos { get; set; } = new List<string>();
    }
}
