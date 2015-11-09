using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using BBdownloader.DataSource;

namespace BBdownloader.FileSystem
{
    class Mirror
    {
        private IFileSystem source;
        private IFileSystem target;

        public Mirror(IFileSystem source, IFileSystem target)
        {
            this.source = source;
            this.target = target;
        }

        private void CreateDirectoriesOnTarget()
        {
            var sourceDirs = source.ListDirectories("");
            var targetDirs = target.ListDirectories("");
            
            var toCreate = from d in sourceDirs
                           where (!targetDirs.Contains(d))
                           orderby d ascending
                           select d;

            foreach (var d in toCreate)
                target.CreateDirectory(d);
        }

        private void DeleteDirectoriesOnTarget()
        {
            var sourceDirs = source.ListDirectories("");
            var targetDirs = target.ListDirectories("");

            var toDelete = from f in targetDirs
                           where (!sourceDirs.Contains(f))
                           orderby f ascending
                           select f;

            foreach (var d in toDelete)
                target.DeleteDirectory(d);
        }

        private void DeleteFilesOnTarget(string path)
        {
            var sourceFiles = source.ListFiles(path);
            var targetFiles = target.ListFiles(path);

            var toDelete = from f in targetFiles
                           where (!sourceFiles.Contains(f))
                           orderby f ascending
                           select f;

            foreach (var f in toDelete)
                target.DeleteFile(f);
        }

        private void CopyNewFilesToTarget(string path)
        {
            var sourceFiles = source.ListFiles(path);
            var targetFiles = target.ListFiles(path);

            var toCopy = from f in sourceFiles
                         where (!targetFiles.Contains(f))
                         orderby f ascending
                         select f;

            foreach (var file in toCopy)
	        {
                var contents = source.ReadFileRaw(Path.Combine(path, file));
                target.WriteFileRaw(Path.Combine(path, file), contents);
	        }

        }

        private void CompareFiles(string path)
        {           
            var sourceFiles = source.ListFiles(path);
            var targetFiles = target.ListFiles(path);

            foreach (var targetFile in targetFiles)
            {
                var fullPath = Path.Combine(path, targetFile);

                var sourceSize = source.FileSize(fullPath);
                var targetSize = target.FileSize(fullPath);

                if (sourceSize != targetSize)
                {
                    target.DeleteFile(Path.Combine(path, targetFile));
                    var contents = source.ReadFileRaw(fullPath);
                    target.WriteFileRaw(fullPath, contents);
                }
            }

        }
        
        private void FileOperations()
        {
            var directoryList = target.ListDirectories("");


            var targetDirs = from files in directoryList
                             orderby files ascending
                             select files;

            int counter = -1;

            foreach (var dir in targetDirs)
            {
                counter++;
                ProgressBar.DrawProgressBar(counter+1, targetDirs.Count());

                DeleteFilesOnTarget(dir);
                CompareFiles(dir);
                CopyNewFilesToTarget(dir);
            }
        }

        public void PerformOperations()
        {
            DeleteDirectoriesOnTarget();
            CreateDirectoriesOnTarget();
            FileOperations();
        }


    }
}
