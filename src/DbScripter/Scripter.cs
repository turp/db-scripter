using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Dapper;

namespace DbScripter
{
    public class Scripter
    {
        private readonly CommandLineParam _parameters;

        public Scripter(CommandLineParam parameters)
        {
            this._parameters = parameters;
        }

        public void Execute()
        {
            using (var connection = CreateSqlConnection(_parameters))
            {
                connection.Open();

                var objects = GetDatabaseObjects(connection);

                if (!objects.Any())
                {
                    Console.Error.WriteLine("No objects found on database {0}: {1}", _parameters.Server, _parameters.Database);
                    return;
                }

                foreach (var o in objects)
                    CreateScript(_parameters.OutputFolder, connection, o.name, o.Type, _parameters.Force);
            }
        }

        private SqlConnection CreateSqlConnection(CommandLineParam parameters)
        {
            var conn = $"Server={parameters.Server};Database={parameters.Database};Trusted_Connection=yes;";
            return new SqlConnection(conn);
        }

        private List<dynamic> GetDatabaseObjects(SqlConnection connection)
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

        private void CreateScript(string outputFolder, SqlConnection connection, string name, string type, bool force)
        {
            var path = Path.Combine(outputFolder, type + "s");
            var fileName = Path.Combine(path, name + ".sql");

            if (File.Exists(fileName) && !force)
            {
                Console.WriteLine("SKIPPED: {0}", name);
                return;
            }

            CreateDirectory(path);

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

        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private void CreateFileFooter(string name, string type, StreamWriter sw)
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

        private void CreateFileHeader(string name, string type, StreamWriter sw)
        {
            sw.WriteLine("IF OBJECT_ID('dbo.[{0}]') IS NOT NULL", name);
            sw.WriteLine("\tDROP {0} dbo.[{1}]", type.ToUpper(), name);
            sw.WriteLine("GO");
        }
    }
}