using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class StudentGroupDTO
    {
        #region attributes
        [Required]
        public string enrollmentDate { get; set; }
        #endregion

        #region relationships 
        // One-to-Many relationship with Student
        public Guid? studentDTOID { get; set; }
        public StudentDTO studentDTOFK { get; set; }

        // One-to-Many relationship with Group
        public Guid? GroupDTOID { get; set; }
        public GroupDTO groupDTOFK { get; set; }
        #endregion
    }
}
