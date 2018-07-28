namespace FallBack
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    class Logic
    {
        private static List<string> GenerateBasePaths(Schema.SchemaModel model)
        {
            List<string> basePaths = new List<string>();

            foreach (var file in model.Files)
            {
                string filePath = Path.Combine(model.BaseDirectory, file);

                basePaths.Add(filePath);
            }

            return basePaths;
        }

        public static bool VerifyValidity(Schema.SchemaModel model)
        {
            if (string.IsNullOrEmpty(model.BaseDirectory) ||
                !Directory.Exists(model.BaseDirectory))
            {
                Logger.LogError($"Base Directory is invalid or not found for schema {model.Name}.");
                return false;
            }

            if (string.IsNullOrEmpty(model.BackupDirectory) ||
                !Directory.Exists(model.BackupDirectory))
            {
                Logger.LogError($"Backup Directory is invalid or not found for schema {model.Name}.");
                return false;
            }

            var basePaths = GenerateBasePaths(model);

            foreach (var path in basePaths)
            {
                if (!File.Exists(path) &&
                    !Directory.Exists(path))
                {
                    Logger.LogError($"Backup File is invalid or not found for schema {model.Name}. File: {path}");
                    return false;
                }
            }

            return true;
        }

        public static void ExecuteSchema(Schema.SchemaModel model)
        {
            // Verify everything is valid 
            if (!Logic.VerifyValidity(model))
            {
                return;
            }

            // Get a timestamp and increment in the case of collisons
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

            string backupDir = Path.Combine(model.BackupDirectory, timestamp);

            if (Directory.Exists(backupDir))
            {
                int dirInc = 1;
                string newDirName = string.Empty;
                while (Directory.Exists((newDirName = $"{backupDir} {dirInc}")))
                {
                    dirInc++;
                }

                backupDir = newDirName;
            }

            Logger.Log($"Timestamp: {timestamp}");
            Logger.Log($"Backup Directory: {backupDir}");

            List<BackupFileModel> validBackupSchemas = new List<BackupFileModel>();

            // Generate BackupFileModel's for each file 
            foreach (var file in model.Files)
            {
                var backupFile = new BackupFileModel(
                    file, 
                    model.BaseDirectory, 
                    backupDir);

                if (!backupFile.IsValid()) continue;

                validBackupSchemas.Add(backupFile);
            }
            
            if (validBackupSchemas.Count == 0)
            {
                Logger.LogError("No files were found to back up. Bailing...");
                return;
            }

            Directory.CreateDirectory(backupDir);

            Logger.Log($"Backing up {validBackupSchemas.Count} schemas.");

            validBackupSchemas.ForEach(x => x.PerformBackup());

            Logger.BlankLine();
            Logger.Log($"Backup complete.");
        }
    }
}
