using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Epa.Camd.Easey.RulesApi.Models
{
    public class NpgSqlContext : DbContext
    {
        public IConfiguration Configuration { get; }

        public NpgSqlContext(IConfiguration configuration, DbContextOptions<NpgSqlContext> options) : base(options)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            int port = 5432;
            int.TryParse(Configuration["EASEY_DB_PORT"], out port);

            string host = Configuration["EASEY_DB_HOST"]?? "database";
            string user = Configuration["EASEY_DB_USER"]??  "postgres";
            string password = Configuration["EASEY_DB_PWD"]?? "password";
            string db = Configuration["EASEY_DB_NAME"]?? "postgres";
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
            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<CheckQueue> Submissions { get; set; }

        //public DbSet<MonitorPlan> MonitorPlans { get; set; }        
    }
}
