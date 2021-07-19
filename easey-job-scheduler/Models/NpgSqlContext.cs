using System.Data;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;

namespace Epa.Camd.Easey.RulesApi.Models
{
    public class NpgSqlContext : DbContext
    {
        public IConfiguration Configuration { get; }
        public DbSet<CheckQueue> Submissions { get; set; }

        public NpgSqlContext(IConfiguration configuration, DbContextOptions<NpgSqlContext> options) : base(options)
        {
            Configuration = configuration;
        }
        
        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     int port = 5432;
        //     int.TryParse(Configuration["EASEY_DB_PORT"], out port);

        //     string host = Configuration["EASEY_DB_HOST"]?? "database";
        //     string user = Configuration["EASEY_DB_USER"]??  "postgres";
        //     string password = Configuration["EASEY_DB_PWD"]?? "password";
        //     string db = Configuration["EASEY_DB_NAME"]?? "postgres";
        //     string vcapServices = Configuration["VCAP_SERVICES"];

        //     if (!string.IsNullOrWhiteSpace(vcapServices))
        //     {
        //         dynamic vcapSvc = JsonConvert.DeserializeObject(vcapServices);
        //         dynamic vcapSvcCreds = vcapSvc["aws-rds"][0].credentials;
                
        //         host = vcapSvcCreds.host;
        //         port = vcapSvcCreds.port;
        //         user = vcapSvcCreds.username;
        //         password = vcapSvcCreds.password;
        //         db = vcapSvcCreds.name;
        //     }

        //     string connectionString = $"server={host};port={port};user id={user};password={password};database={db};pooling=true";
        //     optionsBuilder.UseNpgsql(connectionString);
        // }

        public void ExecuteSql(string tableName, string commandText, DataSet ds)
        {
            var connectionString = this.Database.GetConnectionString();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                    connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = commandText;

                DataTable dt = new DataTable(tableName);
                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                da.Fill(dt);
                ds.Tables.Add(dt);

                connection.Close();
            }
        }

        public NpgsqlCommand ExecuteProcedure(string name, List<NpgsqlParameter> parameters)
        {
            var connectionString = this.Database.GetConnectionString();
            var connection = new NpgsqlConnection(connectionString);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = name;
            command.CommandType = CommandType.StoredProcedure;

            foreach(var param in parameters) {
                command.Parameters.Add(param);
            }

            command.ExecuteNonQuery();
            connection.Close();

            return command;
        }

        public NpgsqlParameter CreateParameter(string name, string value, NpgsqlTypes.NpgsqlDbType type, System.Data.ParameterDirection direction)
        {
            object oValue = value;
            if (string.IsNullOrEmpty(value)) {
                oValue = System.DBNull.Value;
            }

            NpgsqlParameter param = new NpgsqlParameter();
            param.Value = oValue;
            param.ParameterName = name;
            param.NpgsqlDbType = type;
            param.Direction = direction;
            return param;
        }
    }
}
