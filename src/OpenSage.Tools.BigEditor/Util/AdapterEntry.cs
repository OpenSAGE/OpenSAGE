using System;

namespace OpenSage.Tools.BigEditor.Util
{
    public class AdapterEntry
    {


        public readonly string Name;
        public readonly string FullName;
        public readonly long Length;
        public readonly DateTime CreationTime;
        public readonly bool Exists;
        public readonly bool IsFile;
        public readonly string Extension;

        public AdapterEntry(string name, string fullName, string extension, DateTime creationTime, bool exists, bool isFile, long length)
        {
            Name = name;
            FullName = fullName;
            Extension = extension;
            CreationTime = creationTime;
            Exists = exists;
            IsFile = isFile;
            Length = length;
        }
    }
}