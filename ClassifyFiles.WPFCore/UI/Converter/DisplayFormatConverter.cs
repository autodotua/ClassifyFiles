using ClassifyFiles.Data;
using ClassifyFiles.UI.Model;
using ClassifyFiles.Util;
using CSScriptLib;
using FzLib.Basic;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using static ClassifyFiles.Util.FileUtility;

namespace ClassifyFiles.UI.Converter
{
    public class DisplayFormatConverter : IMultiValueConverter
    {
        static DisplayFormatConverter()
        {
            FieldInfo[] fieldInfos = typeof(ExifDirectoryBase).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            exifCode = fieldInfos
                .Where(p => p.IsLiteral && !p.IsInitOnly)
                .ToDictionary(p => p.Name.Substring(3), p => (int)p.GetRawConstantValue());
        }

        private static ConcurrentDictionary<string, MethodDelegate> csMthods = new ConcurrentDictionary<string, MethodDelegate>();

        /// <summary>
        /// 预定义的Exif文字表示对应编号
        /// </summary>
        private static Dictionary<string, int> exifCode;

        /// <summary>
        /// 文件对应的Exif信息
        /// </summary>
        private static ConcurrentDictionary<string, ExifSubIfdDirectory> fileExifSubIfdDirectory = new ConcurrentDictionary<string, ExifSubIfdDirectory>();

        /// <summary>
        /// 以ID号定义的Exif变量
        /// </summary>
        private static Regex rPsExifType = new Regex(@"\$Exif_(?<Type>[0-9]+)", RegexOptions.Compiled);

        private static Regex rCsExifType = new Regex(@"Exif\[(?<Type>[0-9]+)\]", RegexOptions.Compiled);

        /// <summary>
        /// 获取一个文件的Exif信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 获取Exif值
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="name"></param>
        /// <returns></returns>
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

        private string GetExifValue(FileInfo fileInfo, int code)
        {
            var exif = GetExif(fileInfo.FullName);
            if (exif == null)
            {
                return null;
            }
            var tag = exif.Tags.FirstOrDefault(p => p.Type == code);
            return tag?.Description;
        }

        /// <summary>
        /// 获取某个属性值
        /// </summary>
        /// <param name="file"></param>
        /// <param name="key"></param>
        /// <returns></returns>
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
            if (key.StartsWith("Exif-") && file.IsImage())
            {
                string exifKey = key.Substring(5);
                if (int.TryParse(exifKey, out int code))
                {
                    //数字代码
                    return GetExifValue(file, code);
                }
                else
                {
                    return GetExifValue(file, exifKey);
                }
            }
            return null;
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
            if (display.File.IsFolder || string.IsNullOrEmpty(format))
            {
                return (int)parameter == 0 ? display.DefaultDisplayName : "";
            }

            string result;
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                if (format.StartsWith("cs:"))
                {
                    result = ConvertByCs(format, display.FileInfo, c);
                }
                else
                {
                    result = Convert(format, display.FileInfo, c);
                }
            }
            catch (Exception ex)
            {
                result = "";
            }
            sw.Stop();
            Debug.WriteLineIf(DebugSwitch.DisplayValueConvertTime, "convert use " + sw.ElapsedMilliseconds);
            return result;
        }

        public static void InitializeCsMethods(IEnumerable<string> formats)
        {
            foreach (var format in formats)
            {
                string f = format.Substring(3).Trim();
                if (!f.EndsWith(";"))
                {
                    f += ";";
                }
                var csMethod = CSScript.Evaluator.CreateDelegate($@"
string GetValue(System.IO.FileInfo file, System.Collections.Generic.Dictionary<object,string> exifs){{
      var Name =System.IO.Path.GetFileNameWithoutExtension(file.Name);
            var Extension=file.Extension.Replace(""."","""");
            var CreationTime=file.CreationTime;
            var LastWriteTime=file.LastWriteTime;
            var DirectoryName=file.Directory.Name;
var Exif=exifs;
{f}
        }}
");
                csMthods.TryAdd(format, csMethod);
            }
        }

        /// <summary>
        /// 通过C#语法进行值的转换
        /// </summary>
        /// <param name="format"></param>
        /// <param name="file"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public string ConvertByCs(string format, FileInfo file, Class c)
        {
            MethodDelegate csMethod = null;
            if (csMthods.ContainsKey(format))
            {
                csMethod = csMthods[format];
            }
            else
            {
                throw new Exception("发现没有被初始化的C#格式化！这不应该发生");
            }

            Dictionary<object, string> exifs = new Dictionary<object, string>();
            if (file.IsImage() && file.Exists)
            {
                var exif = GetExif(file.FullName);

                if (exif != null)
                {
                    foreach (var code in exifCode)
                    {
                        if (exif.ContainsTag(code.Value))
                        {
                            exifs.Add(code.Key, exif.GetDescription(code.Value));
                        }
                    }

                    //以数字定义的值
                    foreach (Match match in rCsExifType.Matches(format))
                    {
                        int code = int.Parse(match.Groups["Type"].Value);
                        Tag tag = exif.Tags.FirstOrDefault(p => p.Type == code);
                        if (tag != null)
                        {
                            exifs.Add(code, tag.Description);
                        }
                    }
                }
            }

            try
            {
                return csMethod(file, exifs) as string;
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        /// <summary>
        /// 通过简单语法进行值的转换
        /// </summary>
        /// <param name="format"></param>
        /// <param name="file"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private string Convert(string format, FileInfo file, Class c)
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
                char ch = format[i];//当前字符
                if (!hasLeft && ch == '{' && !ignoreNext)
                {
                    //如果没有左大括号，并且当前为左大括号，没并且不忽略下一个括号组
                    if (i > 0)
                    {
                        //不是第一个的话，就要把前面的纯文本加进去
                        result.Append(format[lastIndex..i]);
                        lastIndex = i;
                    }
                    hasLeft = true;
                }
                else if (ch == '}' && ignoreNext)
                {
                    //如果是右大括号并且忽略括号组，那么忽略结束，纯文本开始
                    ignoreNext = false;
                    lastIndex = i + 1;
                    continue;
                }
                else if (hasLeft && ch == '}')
                {
                    //如果具有左大括号并且当前为右大括号，那么变量结束
                    hasLeft = false;
                    string value = GetValue(file, format[(lastIndex + 1)..i]);
                    if (value == null)
                    {
                        //没有获取到值，查看是否后面跟着??{，如果有的话那么就不显示，显示后面的
                        if (i + 3 < format.Length && format[i + 1] == '?' && format[i + 2] == '?' && format[i + 3] == '{')
                        {
                            i += 2;
                        }
                        else
                        {
                            //否则显示原文
                            result.Append(format[lastIndex..(i + 1)]);
                        }
                        lastIndex = i + 1;
                    }
                    else
                    {
                        result.Append(value);
                        //如果后面跟着??{，那么后面的直接忽略
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

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}