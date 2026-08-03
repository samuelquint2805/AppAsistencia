namespace AppAsistencia.DTOs
{
    public class TwoFactorRequiredDTO
    {

        // false = el correo ya estaba confirmado -> se inicio sesion directo, sin 2FA
        // true  = primera vez / correo aun no confirmado -> falta ingresar el codigo
        public bool RequiresTwoFactor { get; set; } = true;
        public Guid IdUser { get; set; }
        public string? EmailEnmascarado { get; set; } 
    }
}
