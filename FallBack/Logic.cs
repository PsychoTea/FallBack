namespace FallBack
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    class Logic
    {
        private static Regex TimestampRegex = new Regex(@"\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}(_\d+)?");

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
                while (Directory.Exists((newDirName = $"{backupDir}_{dirInc}")))
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

            Logger.Log($"Backing up {validBackupSchemas.Count} objects.");

            validBackupSchemas.ForEach(x => x.PerformBackup());

            Logger.Log($"Backup complete.");
        }

        public static void CleanSchema(Schema.SchemaModel model)
        {
            if (!Logic.VerifyValidity(model))
            {
                return;
            }

            if (model.CleanKeepCount <= 0)
            {
                Logger.Log($"Clean count for model {model.Name} is set to {model.CleanKeepCount}. Skipping...");
                return;
            }

            List<string> dirs = Directory.GetDirectories(model.BackupDirectory, "*", SearchOption.TopDirectoryOnly)
                                    .Where(x => TimestampRegex.IsMatch(Path.GetFileName(x)))
                                    .ToList();
            
            List<string> dirsToRemove = dirs.OrderByDescending(x => x)
                                            .Skip(model.CleanKeepCount)
                                            .ToList();

            foreach (var dir in dirsToRemove)
            {
                Logger.Log($"--> {dir}");

                Directory.Delete(dir, true);
            }

            Logger.Log("Clean complete.");
        }
    }
}
