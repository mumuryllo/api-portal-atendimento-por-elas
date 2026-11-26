using FluentValidation;
using portal_por_elas.AutoMapping;
using portal_por_elas.Data.Repositories;
using portal_por_elas.Data.UnitOfWork;
using portal_por_elas.Interfaces;
using portal_por_elas.Interfaces.Repositories;
using portal_por_elas.Interfaces.Services;
using portal_por_elas.Middlewares;
using portal_por_elas.Security;
using portal_por_elas.Services;
using portal_por_elas.Services.portal_por_elas.Services;
using portal_por_elas.Validators;
using PortalOS.Data.Repositories;

namespace portal_por_elas.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddScopedDependencies(this IServiceCollection services)
        {
            services.AddScoped<ErrorHandlingMiddleware>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //User
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>(); 
            services.AddScoped<IUserService, UserService>();

            //Email
            services.AddScoped<IEmailService, EmailService>();

            // Dependencias do Painel
            services.AddScoped<IEventoRepository, EventoRepository>();
            services.AddScoped<IServicoRepository, ServicoRepository>();
            services.AddScoped<IFilaAtendimentoRepository, FilaAtendimentoRepository>();

            services.AddSingleton<IFilaNotificacaoService, FilaNotificacaoService>();
            services.AddScoped<IGestaoFilaService, GestaoFilaService>();

            return services;
        }

        public static IServiceCollection AddMappingDependencies(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(UserProfile));
            return services;
        }
        public static IServiceCollection AddValidatorDependencies(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
            return services;
        }
    }
}
