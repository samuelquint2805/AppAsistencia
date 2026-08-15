namespace AppAsistencia.DTOs
{
    public class BulkImportResultDTO
    {
        public int Creados { get; set; }
        public List<string> Duplicados { get; set; } = new();
        public List<string> Errores { get; set; } = new();
    }
}
