using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace BBdownloader.FileSystem
{
    public class LocalDisk: IFileSystem
    {
        private string _path;

        public bool DirectoryExists(string path)
        {
            string currentPath = Path.Combine(this._path, path);
            return Directory.Exists(currentPath);
        }

        public bool FileExists(string path)
        {
            string currentPath = Path.Combine(this._path, path);

            return File.Exists(currentPath);
        }

        public string[] ReadFile(string path)
        {            
            string currentPath = Path.Combine(this._path, path);
            string[] contents;

            if (this.FileExists(path))
                contents = File.ReadAllLines(currentPath);
            else
                contents = new string[0];

            return contents;
        }

        public bool WriteFile(string path, string contents)
        {
            string currentPath = Path.Combine(this._path, path);

            File.WriteAllText(currentPath, contents);

            return true;
        }

        public bool DeleteDirectory(string path)
        {
            Directory.Delete(path, true);
            return true;
        }


        public bool DeleteFile(string path)
        {
            string currentPath = Path.Combine(this._path, path);
            File.Delete(currentPath);
            return true;
        }

        public string[] ListFiles(string path)
        {
            string currentPath = Path.Combine(this._path, path);

            var dirs = (from dir in Directory.GetFiles(currentPath)
                        select Path.GetFileName(dir)).ToArray();

            return dirs;
        }

        public string[] ListDirectories(string path)
        {
            string currentPath = Path.Combine(this._path, path);

            var dirs = (from dir in Directory.GetDirectories(currentPath)
                        select Path.GetFileName(dir)).ToArray();

            return dirs;
        }

        public void SetPath(string path)
        {
            _path = path;

            if (!DirectoryExists(""))
                CreateDirectory("");
        }

        public bool CreateDirectory(string path)
        {
            string currentPath = Path.Combine(this._path, path);
            Directory.CreateDirectory(currentPath);
            return true;
        }

        public float FileSize(string path)
        {
            string currentPath = Path.Combine(this._path, path);

            if (this.FileExists(path))
            {
                var fileInfo = new FileInfo(currentPath);
                return fileInfo.Length;
            }
            else return 0;
        }



        public bool WriteFile(string path, string[] contents)
        {
            throw new NotImplementedException();
        }
    }
}
