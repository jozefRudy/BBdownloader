using System;
using System.IO;

namespace BBdownloader.FileSystem
{
    public interface IFileSystem
    {
        string GetFullPath();
        void SetPath(string path);
        bool DirectoryExists(string path);
        bool FileExists(string path);

        bool CreateDirectory(string path);


        string[] ReadFile(string path);
        byte[] ReadFileRaw(string path);

        bool WriteFile(string path, string contents);
        bool WriteFile(string path, string[] contents);
        bool WriteFileRaw(string path, byte[] byteArray);

        bool DeleteFile(string path);
        string[] ListFiles(string path);

        string[] ListDirectories(string path);
        bool DeleteDirectory(string path);

        float FileSize(string path);
        DateTime? LastModifiedDate(string path);
    }
}
