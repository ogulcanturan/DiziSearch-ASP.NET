using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiziSearch.Data;
using DiziSearch.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiziSearch
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            #region FORMSSQLConnection
            //FOR MSSQL
            //services.AddDbContext<AppIdentityDbContext>(options =>
            //    options.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=DiziIdentityUsers;Trusted_Connection=True;MultipleActiveResultSets=true"));

            //FOR MSSQL
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration["Data:DiziSearch:ConnectionString"]));
            #endregion
            #region FORSQLITECONNECTION
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite("Data Source=DiziSearch.db"));

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseSqlite("Data Source=DiziIdentityUsers.db"));
            #endregion

            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();


           

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();//Explain error
                app.UseStatusCodePages();//404NotFound
            }
            app.UseStaticFiles();//Enables wwwrootfolder
            app.UseAuthentication();
            app.UseMvc(routes => 
            {
                routes.MapRoute(
                   name: "paginationHomePage",
                   template: "Ara/{searchString}/",
                   defaults: new { controller = "Home", action = "Ara" });

                routes.MapRoute(
                    name: "paginationHomePage",
                    template: "Sayfa/{page:int}",
                    defaults: new { controller = "Home", action = "Index" });

                routes.MapRoute(
                    name: "routingwithAliass",
                    template: "{dizi}/{alias}/{id?}",
                    defaults: new { controller = "Episode", action = "Display" });

                routes.MapRoute(
                    name: "routingwithAlias",
                    template: "Diziler/{category}/{page:int}/",
                    defaults: new { controller = "Dizi", action = "Index" });
                routes.MapRoute(
                   name: "routingwithAlias",
                   template: "Diziler/List/{page:int}/",
                   defaults: new { controller = "Dizi", action = "Index" });

                routes.MapRoute(
                    name:"routingwithAlias",
                    template: "Dizi/{alias}/",
                    defaults: new {controller ="Dizi", action="Display"});

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            //IdentitySeedData.EnsurePopulated(app);
        }
    }
}
