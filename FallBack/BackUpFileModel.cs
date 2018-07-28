namespace FallBack
{
    using System.IO;

    class BackupFileModel
    {
        public string FileName { get; private set; }

        public string OriginalPath { get; private set; }

        public string TargetPath { get; private set; }

        public BackupFileModel(string fileName, string baseDir, string targetDir)
        {
            this.FileName = fileName;

            this.OriginalPath = Path.Combine(baseDir, fileName);
            this.TargetPath = Path.Combine(targetDir, fileName);
        }

        public bool IsValid()
        {
            if (string.IsNullOrEmpty(OriginalPath) ||
                (!File.Exists(OriginalPath) &&
                 !Directory.Exists(OriginalPath)))
            {
                Logger.LogError($"Backup file is invalid or not found ({this.FileName}). Path: {OriginalPath}");
                return false;
            }

            return true;
        }

        public void PerformBackup()
        {
            Logger.Log($"--> {OriginalPath}");

            // Source is a directory
            if (Directory.Exists(OriginalPath))
            {
                // Create the given directory
                Directory.CreateDirectory(TargetPath);

                foreach (string dirPath in Directory.GetDirectories(OriginalPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(OriginalPath, TargetPath));
                }

                foreach (string newPath in Directory.GetFiles(OriginalPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(newPath, newPath.Replace(OriginalPath, TargetPath));
                }

                return;
            }

            string parent = Directory.GetParent(TargetPath).FullName;
            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }
            
            File.Copy(OriginalPath, TargetPath);
        }
    }
}
