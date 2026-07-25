using AppAsistencia.Models;
using Microsoft.EntityFrameworkCore;

namespace AppAsistencia.Data.DBSET
{
    public class DataContextAsistencia : DbContext
    {
        public DataContextAsistencia(DbContextOptions<DataContextAsistencia> options)
          : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("USERMANAGEMENT");

            modelBuilder.Entity<User>()
    .HasOne(u => u.roleFK)
    .WithMany(u => u.usersFK)
    .HasForeignKey(u => u.idRol);

   //         modelBuilder.Entity<User>()
   //.HasOne(u => u.studentFK)
   //.WithMany()
   //.HasForeignKey(u => u.idUser);

   //         modelBuilder.Entity<User>()
   //.HasOne(u => u.administratorFK)
   //.WithMany()
   //.HasForeignKey(u => u.idUser);

   //         modelBuilder.Entity<User>()
   //.HasOne(u => u.professorFK)
   //.WithMany()
   //.HasForeignKey(u => u.idUser);

            modelBuilder.Entity<Administrator>().ToTable("ADMINMANAGEMENT");

            modelBuilder.Entity<Administrator>()
    .HasOne(a => a.User)
    .WithOne(u => u.administratorFK)
    .HasForeignKey<Administrator>(a => a.idAdmin);

 
                //.HasOne(u => u.ParameterAdmins)
                //.WithMany()
                //.HasForeignKey(u => u.idAdmin);

            modelBuilder.Entity<AdminParameter>().ToTable("ADMINPARAMETERMANAGEMENT");

            modelBuilder.Entity<AdminParameter>()
    .HasKey(ap => new { ap.adminID, ap.parameterID });

            modelBuilder.Entity<AdminParameter>()
             .HasOne(u => u.administratorFK)
             .WithMany(u => u.ParameterAdmins)
             .HasForeignKey(u => u.adminID);

            modelBuilder.Entity<AdminParameter>()
             .HasOne(u => u.parametersManagementFK)
             .WithMany(p => p.ParameterAdmins)
             .HasForeignKey(u => u.parameterID);

            modelBuilder.Entity<AttendanceRecord>().ToTable("ATTENDANCERECORDMANAGEMENT");

            modelBuilder.Entity<AttendanceRecord>()
            .HasOne(u => u.studentFK)
            .WithMany(s  => s.attendanceRecordsFK)
            .HasForeignKey(u => u.studentID);

            modelBuilder.Entity<AttendanceRecord>()
            .HasOne(u => u.deviceFK)
            .WithMany(d => d.attendanceRecords)
            .HasForeignKey(u => u.deviceID);

            modelBuilder.Entity<AttendanceRecord>()
            .HasOne(u => u.classSessionFK)
            .WithMany(A => A.attendanceRecords)
            .HasForeignKey(u => u.classSessionID);

            modelBuilder.Entity<ClassSession>().ToTable("CLASSSESSIONMANAGEMENT");

            //modelBuilder.Entity<ClassSession>()
            //.HasOne(u => u.attendanceRecords)
            //.WithMany()
            //.HasPrincipalKey

            modelBuilder.Entity<ClassSession>()
            .HasOne(u => u.groupFK)
            .WithMany(g => g.classSessionsFK)
            .HasForeignKey(u => u.groupID);

            modelBuilder.Entity<Device>().ToTable("DEVICEMANAGEMENT");

            //modelBuilder.Entity<Device>()
            //.HasOne(u => u.attendanceRecords)
            //.WithMany()
            //.HasForeignKey(u => u.idDevice);


            modelBuilder.Entity<Group>().ToTable("GROUPMANAGEMENT");

           // modelBuilder.Entity<Group>()
           //.HasOne(u => u.studentGroupsFK)
           //.WithMany()
           //.HasForeignKey(u => u.idGroup);

           // modelBuilder.Entity<Group>()
           //.HasOne(u => u.classSessionsFK)
           //.WithMany()
           //.HasForeignKey(u => u.idGroup);

            modelBuilder.Entity<Group>()
           .HasOne(u => u.subjectFK)
           .WithMany(s => s.groupsFK)
           .HasForeignKey(u => u.subjectID);

            modelBuilder.Entity<Group>()
           .HasOne(u => u.professorFK)
           .WithMany(p => p.groupsFK)
           .HasForeignKey(u => u.professorID);

            modelBuilder.Entity<ParametersManagement>().ToTable("PARAMETERMANAGEMENT");

          //  modelBuilder.Entity<ParametersManagement>()
          //.HasOne(u => u.ParameterAdmins)
          //.WithMany()
          //.HasForeignKey(u => u.idParameter);


            modelBuilder.Entity<Role>().ToTable("ROLEMANAGEMENT");

         //   modelBuilder.Entity<Role>()
         //.HasOne(u => u.routeRoles)
         //.WithMany()
         //.HasForeignKey(u => u.idRol);

         //   modelBuilder.Entity<Role>()
         //.HasOne(u => u.usersFK)
         //.WithMany()
         //.HasForeignKey(u => u.idRol);



            modelBuilder.Entity<RouteRole>().ToTable("ROUTEROLEMANAGEMENT");

            modelBuilder.Entity<RouteRole>()
    .HasKey(rr => new { rr.routeID, rr.roleID });

            modelBuilder.Entity<RouteRole>()
         .HasOne(u => u.route)
         .WithMany(r => r.routeRoles)
         .HasForeignKey(u => u.routeID);

            modelBuilder.Entity<RouteRole>()
         .HasOne(u => u.role)
         .WithMany(r => r.routeRoles)
         .HasForeignKey(u => u.roleID);

            modelBuilder.Entity<RoutesAs>().ToTable("ROUTEMANAGEMENT");

         //   modelBuilder.Entity<RoutesAs>()
         //.HasOne(u => u.routeRoles)
         //.WithMany()
         //.HasForeignKey(u => u.idRoute);

            modelBuilder.Entity<Student>().ToTable("STUDENTMANAGEMENT");

        //    modelBuilder.Entity<Student>()
        //.HasOne(u => u.attendanceRecordsFK)
        //.WithMany()
        //.HasForeignKey(u => u.idStudent);

        //    modelBuilder.Entity<Student>()
        //.HasOne(u => u.groupsFK)
        //.WithMany()
        //.HasForeignKey(u => u.idStudent);

            modelBuilder.Entity<Student>()
    .HasOne(s => s.user)
    .WithOne(u => u.studentFK)
    .HasForeignKey<Student>(s => s.idStudent);


            modelBuilder.Entity<StudentGroup>().ToTable("STUDENTGROUPMANAGEMENT");

            modelBuilder.Entity<StudentGroup>()
    .HasKey(ap => new { ap.studentID, ap.GroupID});

            modelBuilder.Entity<StudentGroup>()
       .HasOne(u => u.studentFK)
       .WithMany(s => s.groupsFK)
       .HasForeignKey(u => u.studentID);

            modelBuilder.Entity<StudentGroup>()
       .HasOne(u => u.groupFK)
       .WithMany(s => s.studentGroupsFK)
       .HasForeignKey(u => u.GroupID);

            modelBuilder.Entity<Subject>().ToTable("SUBJECTMANAGEMENT");

    //        modelBuilder.Entity<Subject>()
    //.HasOne(u => u.groupsFK)
    //.WithMany()
    //.HasForeignKey(u => u.idSubject);

            modelBuilder.Entity<Professor>().ToTable("PROFESSORMANAGEMENT");

          //  modelBuilder.Entity<Professor>()
          //.HasOne(u => u.groupsFK)
          //.WithMany()
          //.HasForeignKey(u => u.idTeacher);

            modelBuilder.Entity<Professor>()
      .HasOne(p => p.user)
      .WithOne(u => u.professorFK)
      .HasForeignKey<Professor>(p => p.idTeacher);



        }
    }
}
