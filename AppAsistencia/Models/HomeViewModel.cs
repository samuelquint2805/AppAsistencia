namespace AppAsistencia.Models
{
    public class HomeViewModel
    {
        public List<AccionPrincipal> AccionesPrincipales { get; set; } = new();
        public List<AccionSecundaria> AccionesSecundarias { get; set; } = new();
        public ResumenHoyViewModel Resumen { get; set; } = new();
        public List<ClaseProxima> ProximasClases { get; set; } = new();
    }

    public class AccionPrincipal
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;      // clase de Bootstrap Icons, ej: "bi-person-check"
        public string Clases { get; set; } = string.Empty;     // ej: "bg-blue-500 hover:bg-blue-600"
        public string Href { get; set; } = string.Empty;
    }

    public class AccionSecundaria
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Icono { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
    }

    public class ResumenHoyViewModel
    {
        public int ClasesProgramadas { get; set; }
        public int AsistenciasRegistradas { get; set; }
        public int PorcentajeAsistencia { get; set; }
    }

    public class ClaseProxima
    {
        public string NombreClase { get; set; } = string.Empty;
        public string Horario { get; set; } = string.Empty;
        public string Aula { get; set; } = string.Empty;
        public string ColorFondo { get; set; } = "bg-primary";  // alterna bg-primary / bg-secondary como en el original
    }
} 