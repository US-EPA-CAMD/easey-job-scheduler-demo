using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SilkierQuartz;
using Newtonsoft.Json;

using Epa.Camd.Easey.RulesApi.Models;
using Epa.Camd.Easey.JobScheduler.Jobs;

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
            services.AddDbContext<NpgSqlContext>(options =>
                options.UseNpgsql(connectionString)
            );

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
                var type = typeof(HelloJob);
                var assembly = Assembly.GetAssembly(typeof(HelloJob));
                var list = new List<Assembly>();
                list.Add(assembly);
                return list;
            });

            // services.AddQuartzJob<HelloJob>()
            //         .AddQuartzJob<HelloJobAuto>()
            //         .AddQuartzJob<HelloJobSingle>();

            //services.AddTransient<MonitorPlanEvaluation>();
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

            app.UseEndpoints(endpoints =>
            {
              endpoints.MapControllers();
            });
        }
    }
}



