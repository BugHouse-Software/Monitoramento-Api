using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Monitoramento.Api
{
    public class Startup
    {

        private readonly IWebHostEnvironment Environment;
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }



        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAllCors();
            services.AddVersioning();
            services.AddRazorPages();
            services.AddMapeamentos();
            services.AddMemoryCache();
            services.AddServicesTools(() => new Tools.Translates.TranslatationTools.ConfigureTranslate
            {
                DefaultRequestCulture= "pt-BR",
                TranslationsPath = "./Properties/Translations"
            });
            services.AddHttpContextAccessor();
            services.AddControllers(true, true);
            services.AddServices(Configuration);
            services.AddServices(typeof(AutoMapperSetup).Assembly);
            services.AddHandlers(typeof(AutoMapperSetup).Assembly);
            services.AddAllTokens();
            services.AddControllers(o => { o.Filters.Add(typeof(ValidateModelStateAttribute)); });
            services.AddSwagger(hasJwt: true, hasDomain: true, sufixTitle: $" ({Environment.EnvironmentName})");

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.AutomaticAuthentication = true;
            });

            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
                options.AllowResponseHeaderCompression = true;
            });
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.SetMigrateDatabase();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
            app.UseMiddleware<EFExceptionHandlerMiddleware>();
            app.UseExceptionHandler(err => err.UseCustomErrors(true));

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger(provider);
            app.UseAllCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("/index.html");
                endpoints.MapRazorPages();
            });


        }

    }
}
