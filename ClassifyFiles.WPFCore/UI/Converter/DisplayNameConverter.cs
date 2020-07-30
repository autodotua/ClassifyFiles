using ClassifyFiles.Data;
using ClassifyFiles.Util;
using System;
using System.Linq;
using System.IO;
using System.Globalization;
using System.Windows;
using System.Drawing;
using System.Windows.Data;
using FzLib.Basic;
using System.Drawing.Imaging;
using System.Text;
using System.Text.RegularExpressions;
using ClassifyFiles.UI.Model;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Collections.Generic;
using System.Reflection;
using System.Management.Automation;
using System.Collections.Concurrent;

namespace ClassifyFiles.UI.Converter
{
    public class DisplayNameConverter : IMultiValueConverter
    {
        static DisplayNameConverter()
        {
            FieldInfo[] fieldInfos = typeof(ExifDirectoryBase).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            exifCode = fieldInfos
                .Where(p => p.IsLiteral && !p.IsInitOnly)
                .ToDictionary(p => p.Name.Substring(3), p => (int)p.GetRawConstantValue());
        }

        private static Dictionary<string, int> exifCode;
        private static ConcurrentDictionary<string, ExifSubIfdDirectory> fileExifSubIfdDirectory = new ConcurrentDictionary<string, ExifSubIfdDirectory>();
        private static Regex r = new Regex(":", RegexOptions.Compiled);
        private static Regex rExif = new Regex(@"\{Exif\-(?<Name>.+)\}", RegexOptions.Compiled);
        private static Regex rNullable = new Regex(@"(?<First>\{Exif\-(?<Name>.+)\})??(?<Second>\{Exif\-(?<Name>.+)\})", RegexOptions.Compiled);

        private ExifSubIfdDirectory GetExif(string path)
        {
            if (fileExifSubIfdDirectory.TryGetValue(path, out ExifSubIfdDirectory exif))
            {
                return exif;
            }
            try
            {
                var metadatas = ImageMetadataReader.ReadMetadata(path);
                ExifSubIfdDirectory dir = metadatas.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (dir != null)
                {
                    fileExifSubIfdDirectory.TryAdd(path, dir);
                    return dir;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string GetExifValue(FileInfo fileInfo, string name)
        {
            if (!exifCode.ContainsKey(name))
            {
                return null;
            }
            var exif = GetExif(fileInfo.FullName);
            if (exif == null)
            {
                return null;
            }
            if (!exif.ContainsTag(exifCode[name]))
            {
                return null;
            }
            return exif.GetString(exifCode[name]);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0].Equals(DependencyProperty.UnsetValue))
            {
                return "";
            }
            UIFileDisplay display = values[0] as UIFileDisplay;
            Class c = values[1] as Class;
            System.Diagnostics.Debug.Assert(c != null);
            string format = (int)parameter switch
            {
                0 => c.DisplayNameFormat,
                1 => c.DisplayProperty1,
                2 => c.DisplayProperty2,
                3 => c.DisplayProperty3
            };
            if (display.File.IsFolder ||  string.IsNullOrEmpty(format))
            {
                return (int)parameter==0? display.DefaultDisplayName:"";
            }
       
            string result;
            if (format.StartsWith("ps:"))
            {
                result = ConvertByPs(format,display.FileInfo, c);
            }
            else
            {
                result = Convert(format, display.FileInfo, c);
            }
            return result;

        }

        public string ConvertByPs(string format, FileInfo file, Class c)
        {
            PowerShell ps = PowerShell.Create();
            ps.AddScript($"$Name='{Path.GetFileNameWithoutExtension(file.Name)}'");
            ps.AddScript($"$Extension='{file.Extension.RemoveStart(".")}'");
            ps.AddScript($"$CreationTime='{file.CreationTime}'");
            ps.AddScript($"$LastWriteTime='{file.LastWriteTime}'");
            ps.AddScript($"$DirectoryName='{file.Directory.Name}'");
            var exif = GetExif(file.FullName);
            string ext = file.Extension.RemoveStart(".");
            if (exif != null && FileUtility.imgExtensions.Contains(ext.ToLower()))
            {
                foreach (var code in exifCode)
                {
                    if (exif.ContainsTag(code.Value))
                    {
                        ps.AddScript($"$Exif_{code.Key}=\"{exif.GetDescription(code.Value)}\"");
                    }
                }
            }
            foreach (var line in format.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
            {
                ps.AddScript(line.Trim().RemoveStart("ps:"));
            }

            try
            {
                var result = ps.Invoke();
                if (result.Count >= 1)
                {
                    return result[0]?.ToString();
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        private string Convert(string format,FileInfo file, Class c)
        {
            string name = file.Name;
            string path = file.FullName;
            string ext = file.Extension.RemoveStart(".");
            string str = format;

            StringBuilder result = new StringBuilder();
            int lastIndex = 0;//纯文本的开头索引
            bool hasLeft = false;//是否有左大括号
            bool ignoreNext = false;//是否忽略下一个左大括号，用于在两个问号之后
            for (int i = 0; i < format.Length; i++)
            {
                char ch = format[i];
                if (!hasLeft && ch == '{' && !ignoreNext)
                {
                    if (i > 0)
                    {
                        result.Append(format[lastIndex..i]);
                        lastIndex = i;
                    }
                    hasLeft = true;
                }
                else if (ch == '}' && ignoreNext)
                {
                    ignoreNext = false;
                    lastIndex = i + 1;
                    continue;
                }
                else if (hasLeft && ch == '}')
                {

                    hasLeft = false;
                    string value = GetValue(file, format[(lastIndex + 1)..i]);
                    if (value == null)
                    {
                        if (i + 3 < format.Length && format[i + 1] == '?' && format[i + 2] == '?' && format[i + 3] == '{')
                        {
                            i += 2;
                        }
                        else
                        {
                            result.Append(format[lastIndex..(i + 1)]);
                        }
                        lastIndex = i + 1;
                    }
                    else
                    {
                        result.Append(value);
                        if (i + 3 < format.Length && format[i + 1] == '?' && format[i + 2] == '?' && format[i + 3] == '{')
                        {
                            i += 2;
                            ignoreNext = true;
                        }
                        lastIndex = i + 1;
                    }
                }
            }
            result.Append(format[lastIndex..format.Length]);
            return result.ToString();
        }

        private string GetValue(FileInfo file, string key)
        {
            switch (key)
            {
                case "":
                    return "";
                case nameof(FileInfo.Name):
                    return Path.GetFileNameWithoutExtension(file.Name);
                case nameof(FileInfo.Extension):
                    return file.Extension.RemoveStart(".");
            }
            if (!file.Exists)
            {
                //接下来的操作都需要有文件的存在
                return null;
            }
            switch (key)
            {
                case nameof(FileInfo.CreationTime):
                    return file.CreationTime.ToString();
                case nameof(FileInfo.LastWriteTime):
                    return file.LastWriteTime.ToString();
                case nameof(FileInfo.DirectoryName):
                    return file.Directory.Name;
            }
            if (key.StartsWith("Exif-") && FileUtility.imgExtensions.Contains(file.Extension.RemoveStart(".").ToLower()))
            {
                string exifKey = key.Substring(5);
                return GetExifValue(file, exifKey);
            }
            return null;

        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}
