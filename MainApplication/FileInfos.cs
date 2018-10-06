using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace MainApplication
{
    public class FileInfos : CollectionBase
    {
        public void Add(FileInfo newFileInfo)
        {
            List.Add(newFileInfo);
        }

        public void Remove(FileInfo newFileInfo)
        {
            List.Remove(newFileInfo);
        }

        public FileInfo this[int fileInfoIndex]
        {
            get
            {
                return (FileInfo)List[fileInfoIndex];
            }
        }
    }

    public struct FileInfo
    {
        public string type;
        public string name;
    }
}
