namespace DbScripter
{
    public class CommandLineParam
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public string OutputFolder { get; set; }
        public bool Force { get; set; }

        public bool IsValid()
        {
            return false == (string.IsNullOrEmpty(Database) || string.IsNullOrEmpty(Server) || string.IsNullOrEmpty(OutputFolder));
        }
    }
}