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
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public string[] ReadFile(string path)
        {            
            string currentPath = Path.Combine(this._path, path);
            string[] contents;

            if (File.Exists(currentPath))
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

        public bool DeleteFile(string path)
        {
            string currentPath = Path.Combine(this._path, path);
            File.Delete(currentPath);
            return true;
        }

        public string[] ListFiles(string path)
        {
            string currentPath = Path.Combine(this._path, path);
            return Directory.GetFiles(currentPath);
        }

        public void SetPath(string path)
        {
            _path = path;
        }

        public bool CreateDirectory(string path)
        {
            string currentPath = Path.Combine(this._path, path);
            Directory.CreateDirectory(currentPath);
            return true;
        }
    }
}
