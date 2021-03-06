using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using SilkierQuartz;
using Newtonsoft.Json;

using ECMPS.Checks.CheckEngine;
using Epa.Camd.Easey.RulesApi.Models;

namespace Epa.Camd.Easey.JobScheduler
{
    public class Startup
    {
        private string connectionString;
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            int port = 5432;
            int.TryParse(Configuration["EASEY_DB_PORT"], out port);

            string host = Configuration["EASEY_DB_HOST"] ?? "database";
            string user = Configuration["EASEY_DB_USER"] ?? "postgres";
            string password = Configuration["EASEY_DB_PWD"] ?? "password";
            string db = Configuration["EASEY_DB_NAME"] ?? "postgres";
            string vcapServices = Configuration["VCAP_SERVICES"];

            if (!string.IsNullOrWhiteSpace(vcapServices))
            {
                dynamic vcapSvc = JsonConvert.DeserializeObject(vcapServices);
                dynamic vcapSvcCreds = vcapSvc["aws-rds"][0].credentials;

                host = vcapSvcCreds.host;
                port = vcapSvcCreds.port;
                user = vcapSvcCreds.username;
                password = vcapSvcCreds.password;
                db = vcapSvcCreds.name;
            }

            connectionString = $"server={host};port={port};user id={user};password={password};database={db};pooling=true";
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddCors(options => {
            //     options.AddPolicy("CorsPolicy", builder => {
            //         builder.WithOrigins(
            //             "http://localhost:3000",
            //             "https://localhost:3000",
            //             "https://easey-dev.app.cloud.gov",
            //             "https://easey-tst.app.cloud.gov"
            //         )
            //         .AllowAnyMethod()
            //         .AllowAnyHeader();
            //     });
            // });

            services.AddDbContext<NpgSqlContext>(options =>
                options.UseNpgsql(connectionString)
            );

            services.AddSwaggerGen(c => {
                c.SwaggerDoc(
                    "v1",
                    new OpenApiInfo {
                        Title = "Quartz Job Management OpenAPI Specification",
                        Version = "v1",
                    }
                );
            });            

            services.AddRazorPages();
            services.AddControllers();

            services.AddSilkierQuartz(q => {
                var quartzConfig = Configuration.GetSection("Quartz").GetChildren().GetEnumerator();

                while (quartzConfig.MoveNext())
                {
                    q.Add(quartzConfig.Current.Key, quartzConfig.Current.Value);
                }
                q.Add("quartz.dataSource.default.connectionString", connectionString);
            }, () => {
                var list = new List<Assembly>();

                var type = typeof(cCheckEngine);
                var assembly = Assembly.GetAssembly(typeof(cCheckEngine));
                list.Add(assembly);

                // type = typeof(MainJob);
                // assembly = Assembly.GetAssembly(typeof(MainJob));
                // list.Add(assembly);

                return list;
            });

            services.AddQuartzJob<cCheckEngine>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseSilkierQuartzAuthentication() ;
            //app.UseCors("CorsPolicy");
            app.UseAuthorization();

            app.UseSilkierQuartz(
                new SilkierQuartzOptions()
                {
                    //Logo = "data:wwwroot/EPALogo.svg",
                    //ProductName = "EPA CAMD Quartz Scheduler",
                    //Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result,
                    VirtualPathRoot = "/quartz",
                    UseLocalTime = true,
                    DefaultDateFormat = "yyyy-MM-dd",
                    DefaultTimeFormat = "HH:mm:ss",
                    CronExpressionOptions = new CronExpressionDescriptor.Options()
                    {
                        DayOfWeekStartIndexZero = false //Quartz uses 1-7 as the range
                    }
                    #if ENABLE_AUTH
                        ,
                        AccountName = "admin",
                        AccountPassword = "password",
                        IsAuthenticationPersist = false
                    #endif
                }
            );

            app.UseSwagger(c => {
                c.RouteTemplate = "quartz/api/swagger/{documentname}/swagger.json";
            });
            app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/quartz/api/swagger/v1/swagger.json", "v1");
                c.RoutePrefix = "quartz/api/swagger";
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}



