namespace EducaWebApi.Domain.Entities
{
    public class Turma
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int VagasTotais { get; set; }
        public int VagasDisponiveis { get; set; }
    }
}