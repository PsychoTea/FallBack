namespace FallBack.Schema
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    public static class Manager
    {
        private const string DirName = "Schemas";

        private const string FileExtension = ".json";

        private const string SchemaNamePattern = "*.json";

        private static string SchemaDirectory => Path.Combine(Directory.GetCurrentDirectory(), DirName);

        public static List<SchemaModel> Schemas { get; private set; } = new List<SchemaModel>();

        public static int Initialize()
        {
            if (!Directory.Exists(SchemaDirectory))
            {
                Directory.CreateDirectory(SchemaDirectory);
            }

            foreach (var file in Directory.GetFiles(SchemaDirectory, $"*{FileExtension}", SearchOption.TopDirectoryOnly))
            {
                string fileData = File.ReadAllText(file);

                SchemaModel newModel = null;
                try
                {
                    newModel = JsonConvert.DeserializeObject<SchemaModel>(fileData);
                }
                catch (Exception ex)
                {
                    Logger.LogException($"An exception occured whilst deserailizing a schema. Is the schema valid? Path: {file}", ex);
                    continue;
                }

                Schemas.Add(newModel);
            }

            return Schemas.Count;
        }

        public static void Generate(string name)
        {
            var newSchema = new SchemaModel()
            {
                Name = name
            };

            string text = JsonConvert.SerializeObject(newSchema, Formatting.Indented);

            // Piece together filename
            string schemaPath = Path.Combine(SchemaDirectory, name + FileExtension);

            File.WriteAllText(schemaPath, text);
        }

        public static SchemaModel Find(string name) => Schemas.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
    }
}
