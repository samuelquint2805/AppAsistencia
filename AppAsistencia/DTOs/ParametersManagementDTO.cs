using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class ParametersManagementDTO
    {
        #region attributes


        [Key]
        [Required]
        public Guid idParameter { get; set; }

        [Required]
        public string parameterKey { get; set; }

        [Required]
        public string parameterValue { get; set; }

        [Required]
        public string description { get; set; }

        [Required]
        public int version { get; set; }
        [Required]
        public bool isActive { get; set; } = true;
        [Required]
        public DateTime lastModifiedDate { get; set; }
        #endregion

        #region relationships
        //this contains the foreign key from AdminParameter (1:N) to indicate the relationShip between ParametersManagement and AdminParameter (N:M)
        public List<AdminParameterDTO> ParameterAdminsDTO { get; } = [];
        #endregion

    }
}
