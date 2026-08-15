namespace AppAsistencia.DTOs
{
    public class ImportStudentsResultDTO
    {
        public int EstudiantesCreados { get; set; }
        public int EstudiantesVinculados { get; set; }
        public int FilasConError { get; set; }
        public List<string> Errores { get; set; } = new();
    }
}
