namespace EducaWebApi.Domain.Dtos
{
    public class TurmaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int VagasTotal { get; set; }
        public int VagasDisponiveis { get; set; }
    }
}
