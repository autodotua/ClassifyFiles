using System.Collections.Generic;
using System.Linq;

namespace ClassifyFiles.UI.Model
{
    public class FileType
    {
        static FileType()
        {
            FileTypes.Add(new FileType("Video", "\uE714", "mp4", "avi", "mkv", "mov", "rm", "rmvb"));
            FileTypes.Add(new FileType("Photo", "\uEB9F", "jpeg", "jpg", "tiff", "tif", "png", "bmp", "dng", "raw", "rw2", "arw", "psd"));
            FileTypes.Add(new FileType("Audio", "\uE8D6", "mp3", "acc", "ogg", "flac", "wav"));
            FileTypes.Add(new FileType("Program", "\uE756", "exe", "msi", "apk", "dll", "ini", "xml"));
            FileTypes.Add(new FileType("Document", "\uE8A5", "doc", "docx", "ppt", "pptx", "xls", "xlsx", "txt", "md"));
        }

        public string Name { get; set; }
        public IReadOnlyList<string> Extensions { get; }
        public string Glyph { get; set; }

        public FileType(string name, string glyph, params string[] exts)
        {
            Name = name;
            Extensions = exts.ToList().AsReadOnly();
            Glyph = glyph;
        }

        public static List<FileType> FileTypes { get; private set; } = new List<FileType>();
    }
}