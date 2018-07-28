namespace FallBack
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    class Program
    {
        private static void PrintUsage()
        {
            Logger.Log("Usage:");

            Logger.Log("  help            -- Shows this help page");
            Logger.Log("  template [name] -- Create a schema template with [name]");
            Logger.Log("  list            -- List all schemas");
            Logger.Log("  execute [name]  -- Execute the given schema");
            Logger.Log("  executeall      -- Execute all schemas");
            Logger.Log("  clean           -- Clean the given schema");
            Logger.Log("  cleanall        -- Clean all schemas");

            Logger.BlankLine();
        }

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var name = new AssemblyName(e.Name).Name;
                string assemblyName = $"FallBack.Dependencies.{name}.dll";
               
                var assembly = Assembly.GetExecutingAssembly();

                if (!assembly.GetManifestResourceNames().Any(x => x.Contains(assemblyName)))
                {
                    Logger.Log($"Tried to resolve assembly but was not found: {name}.");
                    return null;
                }

                using (var stream = assembly.GetManifestResourceStream(assemblyName))
                {
                    byte[] assmData = new BinaryReader(stream).ReadBytes((int)stream.Length);

                    return Assembly.Load(assmData);
                }

            };

            Run(args);
        }

        private static void Run(string[] args)
        {
            Logger.BlankLine();
            Logger.Log($"FallBack. Written by PsychoTea (Ben Sparkes).");

            int loadedSchemas = Schema.Manager.Initialize();

            Logger.Log($"Loaded {loadedSchemas} schemas.");

            // help             -- Shows this help page
            // template [name]  -- Create a schema template
            // list             -- List all schemas
            // execute [name]   -- Execute the given schema
            // executeall       -- Execute all schemas
            // clean [name]     -- Clean the given schema
            // cleanall         -- Clean all schemas

            Logger.BlankLine();

            if (args.Length < 1)
            {
                PrintUsage();
                return;
            }

            string option = args[0].ToLower();

            switch (option)
            {
                case "help":
                    PrintUsage();
                    break;

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

                    string exSchemaName = string.Join(" ", args.Skip(1));

                    // Find the schema
                    Schema.SchemaModel exSchema = Schema.Manager.Find(exSchemaName);
                    if (exSchema == null)
                    {
                        Logger.Log($"A schema was not found with the name '{exSchemaName}'.");
                        return;
                    }

                    // Execute!
                    Logger.Log($"Executing schema {exSchema.Name}...");
                    Logic.ExecuteSchema(exSchema);
                    break;

                case "executeall":
                    Logger.Log($"Executing all ({loadedSchemas}) schemas...");
                    Logger.BlankLine();

                    foreach (var schema in Schema.Manager.Schemas)
                    {
                        Logger.Log($"Executing schema {schema.Name}...");
                        Logic.ExecuteSchema(schema);
                        Logger.BlankLine();
                    }
                    
                    Logger.Log($"Finished executing {loadedSchemas} schemas.");
                    break;

                case "clean":
                    if (args.Length < 2)
                    {
                        PrintUsage();
                        return;
                    }

                    string cleanSchemaName = string.Join(" ", args.Skip(1));

                    // Find the schema
                    Schema.SchemaModel cleanSchema = Schema.Manager.Find(cleanSchemaName);
                    if (cleanSchema == null)
                    {
                        Logger.Log($"A schema was not found with the name '{cleanSchemaName}'.");
                        return;
                    }

                    // Clean!
                    Logger.Log($"Cleaning schema {cleanSchema.Name}...");
                    Logic.CleanSchema(cleanSchema);
                    break;

                case "cleanall":
                    Logger.Log($"Cleaning all ({loadedSchemas}) schemas...");
                    Logger.BlankLine();

                    foreach (var schema in Schema.Manager.Schemas)
                    {
                        Logger.Log($"Cleaning schema {schema.Name}...");
                        Logic.CleanSchema(schema);
                        Logger.BlankLine();
                    }

                    Logger.Log($"Finished cleaning {loadedSchemas} schemas.");
                    break;

                default:
                    PrintUsage();
                    break;
            }
        }
    }
}
