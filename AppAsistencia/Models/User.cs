using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class User
    {
        #region atributes
        [Key]
        [Required]
        public Guid idUser { get; set; }

        

        [Required]
        public string userName { get; set; } = null!;

        [Required]
        public string firstname { get; set; } = null!;

        [Required]
        public string lastName { get; set; } = null!;

        [Required]
        public string email { get; set; } = null!;

        [Required]
        public string passwordHash { get; set; } = null!;

        [Required]
        public bool isActive { get; set; } = true;

        [Required]
        public bool isEmailConfirmed { get; set; } = false;
        [Required]
        public DateTime registerDate { get; set; }

        [Required]
        public DateTime accountRenewalDate { get; set; }
        #endregion

        #region relationships
        // One-to-Many relationship with Student
        public Student studentFK { get; set; } = null!;

        // One-to-Many relationship with Administrator
        public Administrator administratorFK { get; set; } = null!;

        // One-to-Many relationship with professor
        public Professor professorFK { get; set; } = null!;

        // One-to-Many relationship with role
        public Guid? idRol { get; set; }
        public Role roleFK { get; set; } = null!;

        #endregion

    }
}
