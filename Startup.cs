using TaskPlusPlus.API.DbContexts;
using TaskPlusPlus.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Web.Http;
using Microsoft.AspNetCore.Cors;

namespace TaskPlusPlus.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddCors(options => 
            options.AddPolicy("AllowSpecificOrigin",
            builder =>
            {
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));
            
            services.AddControllers();

            services.AddScoped<ITaskPlusPlusRepository, TaskPlusPlusRepository>();

            services.AddDbContext<TaskPlusPlusContext>(options =>
            {
                options.UseSqlServer(
                    @"Server=.;Database=TaskPlusPlusDB;User Id=taskppir;Password=@yrS5j01JtVrmoob;");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseCors("AllowSpecificOrigin");


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            /*app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // using Microsoft.AspNetCore.Http;
                    ctx.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                }
            });
            */
            app.UseRouting();


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
