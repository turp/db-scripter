using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using Args;
using Dapper;
using System.Linq;

namespace DbScripter
{
    class Program
    {
        static void Main(string[] args)
        {
            var parameters = Configuration.Configure<CommandLineParam>().CreateAndBind(args);
            if (string.IsNullOrEmpty(parameters.Database) || string.IsNullOrEmpty(parameters.Server) || string.IsNullOrEmpty(parameters.OutputFolder))
            {
                Console.Error.WriteLine("Usage: DbScripter /s DATABASE_SERVER /d DATABASE /o c:\\temp");
                return;
            }

            ScriptDatabaseObjects(parameters);
        }

        private static SqlConnection CreateSqlConnection(CommandLineParam parameters)
        {
            var conn = string.Format("Server={0};Database={1};Trusted_Connection=yes;",
                parameters.Server,
                parameters.Database
            );

            return new SqlConnection(conn);
        }
        private static void ScriptDatabaseObjects(CommandLineParam parameters)
        {
            using (var connection = CreateSqlConnection(parameters))
            {
                connection.Open();

                var objects = GetDatabaseObjects(connection);

                if (!objects.Any())
                {
                    Console.Error.WriteLine("No objects found on database {0}: {1}", parameters.Server, parameters.Database);
                    return;
                }

                foreach (var o in objects)
                    CreateScript(parameters.OutputFolder, connection, o.name, o.Type, parameters.Force);
            }
        }

        private static List<dynamic> GetDatabaseObjects(SqlConnection connection)
        {
            var objects = connection
                .Query(@"
                    SELECT s.name + '.' + o.name as [name], CASE type WHEN 'P' THEN 'Procedure' WHEN 'V' THEN 'View' WHEN 'TR' THEN 'Trigger' ELSE 'Function' END as [Type]
                    FROM sys.objects o
	                    JOIN sys.schemas s ON o.schema_id = s.schema_id
                    WHERE type in ('P', 'V', 'FN', 'IF', 'TF', 'FS', 'FT', 'TR')
                    ORDER BY s.name, o.name
                ")
                .ToList();

            return objects;
        }

        private static void CreateScript(string outputFolder, SqlConnection connection, string name, string type, bool force)
        {
            var path = Path.Combine(outputFolder, type + "s");
            var fileName = Path.Combine(path, name + ".sql");

            if (File.Exists(fileName) && !force)
            {
                Console.WriteLine("SKIPPED: {0}", name);
                return;
            }

            CreateFolder(path);

            try
            {
                var rows = connection.Query("sp_helptext '" + name + "'");

                using (var sw = File.CreateText(fileName))
                {
                    Console.WriteLine("         {0}", name);

                    CreateFileHeader(name, type, sw);

                    foreach (var row in rows)
                    {
                        sw.Write(row.Text);
                    }

                    CreateFileFooter(name, type, sw);
                }
            }
            catch
            {
                Console.WriteLine("ERROR:    {0} - unable to execute sp_helptext", name);
            }
        }

        private static void CreateFolder(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void CreateFileFooter(string name, string type, StreamWriter sw)
        {
            sw.WriteLine();
            sw.WriteLine("GO");
            sw.WriteLine();
            sw.WriteLine("IF OBJECT_ID('dbo.[{0}]') IS NOT NULL", name);
            sw.WriteLine("\tPRINT '<<< CREATED {0} dbo.{1} >>>'", type.ToUpper(), name);
            sw.WriteLine("ELSE");
            sw.WriteLine("\tPRINT '<<< FAILED CREATING {0} dbo.{1} >>>'", type.ToUpper(), name);
            sw.WriteLine("GO");
            sw.WriteLine();
        }

        private static void CreateFileHeader(string name, string type, StreamWriter sw)
        {
            sw.WriteLine("IF OBJECT_ID('dbo.[{0}]') IS NOT NULL", name);
            sw.WriteLine("\tDROP {0} dbo.[{1}]", type.ToUpper(), name);
            sw.WriteLine("GO");
        }
    }

    internal class CommandLineParam
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string OutputFolder { get; set; }
        public bool Force { get; set; }
    }
}
