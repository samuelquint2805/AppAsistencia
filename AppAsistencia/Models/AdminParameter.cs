using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class AdminParameter
    {
        #region Attributes

        [Required]
        public DateTime modifiedDate { get; set; }

        #endregion

        #region relationships
        // Foreign key to Administrator and ParametersManagement
        public Guid? adminID { get; set; }
        public Administrator administratorFK { get; set; } = null!;

        public Guid? parameterID { get; set; }
        public ParametersManagement parametersManagementFK { get; set; } = null!;
        #endregion
    }
}
