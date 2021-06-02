using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Quartz;

using Epa.Camd.Easey.JobScheduler.Jobs;
using Epa.Camd.Easey.RulesApi.Models;
using Npgsql;
using NpgsqlTypes;
using Quartz.Impl.AdoJobStore.Common;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Epa.Camd.Easey.JobScheduler
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

            string connectionString = $"server={host};port={port};user id={user};password={password};database={db};pooling=true";
            //services.AddScoped<NpgSqlContext>();

            


            services.AddDbContext<NpgSqlContext>(options =>
                options.UseNpgsql(connectionString)
            );


            //DbconfigManager
        

            // base configuration from appsettings.json
            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));

            services.AddQuartz(q =>
            {


                DbProvider.RegisterDbMetadata("Npgsql-30", new DbMetadata
                {
                    AssemblyName = typeof(NpgsqlConnection).Assembly.FullName,
                    BindByName = true,
                    ConnectionType = typeof(NpgsqlConnection),
                    CommandType = typeof(NpgsqlCommand),
                    ParameterType = typeof(NpgsqlParameter),
                    ParameterDbType = typeof(NpgsqlDbType),
                    ParameterDbTypePropertyName = "NpgsqlDbType",
                    ParameterNamePrefix = ":",
                    ExceptionType = typeof(NpgsqlException),
                    UseParameterNamePrefixInParameterCollection = true
                });
                // base quartz scheduler, job and trigger configuration

                // handy when part of cluster or you want to otherwise identify multiple schedulers
                q.SchedulerId = "AUTO";


          
                // or 
                q.UseMicrosoftDependencyInjectionScopedJobFactory();

                // these are the defaults
              
               var QuartzConfig=  Configuration.GetSection("Quartz").GetChildren().GetEnumerator();

                while (QuartzConfig.MoveNext())
                {
                    q.SetProperty(QuartzConfig.Current.Key.ToString(), QuartzConfig.Current.Value.ToString());
                }



                q.UseDefaultThreadPool(tp => {
                    tp.MaxConcurrency = 25;
                });

           

                q.ScheduleJob<CheckEngine>(trigger => trigger
                    .WithIdentity("Check Engine")
                    .WithDailyTimeIntervalSchedule(x => x.WithInterval(1, IntervalUnit.Minute))
                    .WithDescription("Check Engine will poll submission queue every 60 seconds and schedule submission processes.")
                );
            });

            services.AddTransient<MonitorPlanEvaluation>();

            // ASP.NET Core hosting
            services.AddQuartzServer(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            // }

            // app.UseHttpsRedirection();

            // app.UseRouting();

            // app.UseAuthorization();

            // app.UseEndpoints(endpoints =>
            // {
            //     endpoints.MapControllers();
            // });
        }
    }
}



