using AppAsistencia.Data.DBSET;
using AppAsistencia.Services.Abstractions;
using AppAsistencia.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace AppAsistencia
{
    public static class CustomConfiguration
    {
        public static WebApplicationBuilder AddCustomConfiguration(this WebApplicationBuilder builder)
        {
            builder.Services.AddDbContext<DataContextAsistencia>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IEmailSenderService, SmtpEmailSender>();
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            return builder;
        }

    }
    }
