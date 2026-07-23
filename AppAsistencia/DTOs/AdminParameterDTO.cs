using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class AdminParameterDTO
    {
        #region Attributes

        [Required]
        public DateTime modifiedDate { get; set; }

        #endregion

        #region relationships
        // Foreign key to Administrator and ParametersManagement
        public Guid? adminDTOID { get; set; }
        public AdministratorDTO administratorDTOFK { get; set; } = null!;

        public Guid? parameterDTOID { get; set; }
        public ParametersManagementDTO parametersManagementDTOFK { get; set; } = null!;
        #endregion
    }
}
