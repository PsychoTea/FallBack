namespace FallBack.Schema
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class SchemaModel
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Base Directory")]
        public string BaseDirectory { get; set; } = string.Empty;

        [JsonProperty("Backup Directory")]
        public string BackupDirectory { get; set; } = string.Empty;

        [JsonProperty("Files")]
        public List<string> Files { get; set; } = new List<string>();
    }
}
