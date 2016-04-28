using System;
using Args;

namespace DbScripter
{
    class Program
    {
        static void Main(string[] args)
        {
            var parameters = Configuration.Configure<CommandLineParam>().CreateAndBind(args);
            if (!parameters.IsValid())
            {
                Console.Error.WriteLine("Usage: DbScripter /s DATABASE_SERVER /d DATABASE /o c:\\temp");
                return;
            }

            new Scripter(parameters).Execute();
        }
    }
}
