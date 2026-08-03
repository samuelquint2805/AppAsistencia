namespace AppAsistencia.DTOs
{
    public class SeccionPermisoDTO
    {
        public string Slug { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public bool Ver { get; set; }
        public bool Crear { get; set; }
        public bool Editar { get; set; }
        public bool Eliminar { get; set; }
    }

    public class RolePermissionsFormModel
    {
        public string RoleName { get; set; } = string.Empty; // "docente" o "estudiante"
        public List<SeccionPermisoDTO> Secciones { get; set; } = new();
    }
}
