
using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class GroupDTO
    {
        #region attributes

        [Key]
        [Required]
        public Guid idGroup { get; set; }

        [Required]
        public string groupName { get; set; } = null!;

       
        [Required]
        public string classroom { get; set; } = null!; 

        [Required]
        public string semester { get; set; } = null!;
        
        [Required]
        public string GroupCode { get; set; } = null!;

        [Required]
        public bool isActive { get; set; } = true;
        #endregion

        #region relationships
        // One-to-Many relationship with student
        public List<StudentGroupDTO> studentGroupsDTOFK { get; set; } = [];
        public ICollection<GroupScheduleDTO> scheduleDaysFK { get; set; } = new List<GroupScheduleDTO>();

        // One-to-Many relationship with classSession
        public ICollection<ClassSessionDTO> classSessionsDTOFK { get; set; } = new List<ClassSessionDTO>();

        // One-to-Many relationship with subject
        public Guid? subjectDTOID { get; set; }
        public SubjectDTO subjectDTOFK { get; set; } = null!;

        // One-to-Many relationship with professor
        public Guid? professorDTOID { get; set; }
        public ProfessorDTO professorDTOFK { get; set; } = null!;
        #endregion
    }
}
