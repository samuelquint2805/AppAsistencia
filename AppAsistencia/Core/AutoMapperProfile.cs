using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AutoMapper;

namespace AppAsistencia.Core
{
    public class AutoMapperProfile : Profile
    {

        public AutoMapperProfile()
        {
            CreateMap<Student, StudentDTO>().ReverseMap();
            CreateMap<Group, GroupDTO>().ReverseMap();
            CreateMap<ClassSession, ClassSessionDTO>().ReverseMap();
            CreateMap<AttendanceRecord, AttendanceRecordDTO>().ReverseMap();
            CreateMap<StudentGroup, StudentGroupDTO>().ReverseMap();
            CreateMap<Device, DeviceDTO>().ReverseMap();
            CreateMap<Administrator, AdministratorDTO>().ReverseMap();
            CreateMap<ParametersManagement, ParametersManagementDTO>().ReverseMap();
            CreateMap<AdminParameter, AdminParameterDTO>().ReverseMap();
            CreateMap<Professor, ProfessorDTO>().ReverseMap();
            CreateMap<Role, RoleDTO>().ReverseMap();
            CreateMap<RoutesAs, RoutesASDTO>().ReverseMap();
            CreateMap<RouteRole, RouteRoleDTO>().ReverseMap();
            CreateMap<StudentGroup, StudentGroupDTO>().ReverseMap();
            CreateMap<Subject, SubjectDTO>().ReverseMap();
            CreateMap<User, UserDTO>().ReverseMap();


        }
    }
}
