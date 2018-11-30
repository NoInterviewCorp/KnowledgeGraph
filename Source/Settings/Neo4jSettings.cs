namespace KnowledgeGraph
{
    public class Neo4jSettings
    {
        private string connectionString = string.Empty;
        public string ConnectionString
        {
            get
            {
                if (IsDockerized)
                {
                    return Container;
                }
                return connectionString;
            }
            set
            {
                connectionString = value;
            }
        }
        public string Container { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public bool IsDockerized { get; set; }
    }
}
