using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FallBack
{
    class Program
    {
        private static void PrintUsage()
        {
            Logger.Log("Usage:");

            Logger.Log("  template [name] -- Create a schema template with [name]");
            Logger.Log("  list            -- List all schemas");
            Logger.Log("  execute [name]  -- Execute the given schema");
            Logger.Log("  executeall      -- Execute all schemas");

            Logger.BlankLine();
        }

        private static void Main(string[] args)
        {
            Logger.Log($"FallBack. Written by PsychoTea (Ben Sparkes).");

            int loadedSchemas = Schema.Manager.Initialize();

            Logger.Log($"Loaded {loadedSchemas} schemas.");

            // template [name] -- Create a schema template
            // list -- List all schemas
            // execute [name] -- Execute the given schema
            // executeall -- Execute all schemas

            Logger.BlankLine();

            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            string option = args[0].ToLower();

            switch (option)
            {
                case "template":
                    if (args.Length < 2)
                    {
                        PrintUsage();
                        return;
                    }

                    string newSchemaName = string.Join(" ", args.Skip(1));

                    Schema.Manager.Generate(newSchemaName);

                    Logger.Log($"Generated a new schema with the name: {newSchemaName}");
                    break;

                case "list":
                    if (loadedSchemas == 0)
                    {
                        Logger.Log("No schemas were found.");
                        return;
                    }

                    Logger.Log($"Listing {loadedSchemas} schemas:");

                    foreach (var schema in Schema.Manager.Schemas)
                    {
                        Logger.Log($"  {schema.Name} ({schema.Files.Count} files)");
                    }
                    break;

                case "execute":
                    if (args.Length < 2)
                    {
                        PrintUsage();
                        return;
                    }

                    string schemaName = string.Join(" ", args.Skip(1));

                    // Find the schema
                    Schema.SchemaModel foundSchema = Schema.Manager.Find(schemaName);
                    if (foundSchema == null)
                    {
                        Logger.Log($"A schema was not found with the name '{schemaName}'.");
                        return;
                    }

                    // Execute!
                    Logger.Log($"Executing schema {foundSchema.Name}...");
                    Logic.ExecuteSchema(foundSchema);
                    break;

                case "executeall":
                    Logger.Log($"Executing all ({loadedSchemas}) schemas...");
                    Logger.BlankLine();

                    foreach (var schema in Schema.Manager.Schemas)
                    {
                        Logger.Log($"Executing schema {schema.Name}...");
                        Logic.ExecuteSchema(schema);
                    }

                    Logger.BlankLine();
                    Logger.Log($"Finished executing {loadedSchemas} schemas.");
                    break;

                default:
                    PrintUsage();
                    break;
            }
        }
    }
}
