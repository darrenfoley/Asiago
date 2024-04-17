using Asiago.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Asiago.Data.Extensions
{
    public static class IConfigurationExtensions
    {
        public static string? GetPostgresConnectionString(this IConfiguration config)
        {
            string? connectionString = null;

            try
            {
                NpgsqlConnectionStringBuilder connectionStringBuilder;

                // Prioritize full connection string over pieces
                if (config.GetValue<string>("POSTGRES_CONNECTION_STRING") is string dbConnectionString)
                {
                    connectionStringBuilder = new(dbConnectionString);
                }
                else
                {
                    connectionStringBuilder = new()
                    {
                        Host = config.GetValue<string>("POSTGRES_HOST"),
                        Port = config.GetValue<int>("POSTGRES_PORT"),
                        Database = config.GetValue<string>("POSTGRES_DB") ?? "asiago",
                        Username = config.GetValue<string>("POSTGRES_USER"),
                        Password = config.GetValue<string>("POSTGRES_PASSWORD"),
                        Pooling = true,
                    };
                }

                connectionString = connectionStringBuilder.ConnectionString;
            }
            catch (ArgumentException) { }

            return connectionString;
        }

        public static string GetRequiredPostgresConnectionString(this IConfiguration config)
        {
            string? connectionString = config.GetPostgresConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ConfigurationException(
                    "Unable to construct postgres connection string. Missing/invalid POSTGRES_* environment variables?"
                    );
            }
            return connectionString;
        }
    }
}
