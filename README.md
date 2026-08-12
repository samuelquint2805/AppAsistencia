# AppAsistencia
Sistema web de gestión de asistencia estudiantil con control de acceso dinámico basado en roles (RBAC).
Permite a los docentes registrar y administrar la asistencia de sus estudiantes, mientras que los administradores configuran en tiempo real qué roles pueden acceder a cada sección del sistema y qué operaciones (ver, crear, editar, eliminar) tienen habilitadas.
## Tecnologías
| Capa | Tecnología |
|------|------------|
| Backend | ASP.NET Core 8.0 (C#) – MVC |
| Base de datos | SQL Server + Entity Framework Core |
| Autenticación | Cookie Authentication + 2FA por correo electrónico |
| Autorización | RBAC dinámico (`RequireRoutePermissionAttribute`) |
| Seguridad de contraseñas | BCrypt |
| Frontend | Razor Views + Tailwind CSS |
| Build de estilos | Node.js + Tailwind CLI |
## Requisitos previos
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js ≥ 18](https://nodejs.org/)
- [SQL Server 2019+](https://www.microsoft.com/sql-server) (o SQL Server Express / LocalDB)
## Instalación
### 1. Clonar el repositorio
```bash
git clone https://github.com/samuelquint2805/AppAsistencia.git
cd AppAsistencia
