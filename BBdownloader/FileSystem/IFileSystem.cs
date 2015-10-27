using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BBdownloader.FileSystem
{
    public interface IFileSystem
    {
        void SetPath(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);

        string[] ReadFile(string path);
        bool WriteFile(string path, string contents);

        bool DeleteFile(string path);
        string[] ListFiles(string path);

    }
}
