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
        public string institutionalCode { get; set; }

        [Required]
        public string userName { get; set; }

        [Required]
        public string firstname { get; set; }

        [Required]
        public string lastName { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string passwordHash { get; set; }

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
        public Guid? RoleID { get; set; }
        public Role roleFK { get; set; } = null!;

        #endregion

    }
}
