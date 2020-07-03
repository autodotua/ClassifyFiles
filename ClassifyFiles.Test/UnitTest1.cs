using ClassifyFiles.Data;
using ClassifyFiles.Util;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DFile = ClassifyFiles.Data.File;
using IOFile = System.IO.File;

namespace ClassifyFiles.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            CreateFiles();
            Class c = new Class()
            {
                MatchConditions = new List<MatchCondition>()
                {
                    new MatchCondition(){Type=Data.MatchType.InFileName,Value="º½ÅÄ"},
                    new MatchCondition(){ConnectionLogic=Logic.Or, Type=Data.MatchType.InDirName,Value="º½ÅÄ"}
                }
            };
            var classes = new List<Class>() { c };

            var result = FileUtility.GetFilesOfClassesAsync(new DirectoryInfo("test"), classes,false);
            Assert.Pass();
        }

        [Test]
        public void FileSizeConvert()
        {
            Assert.AreEqual(FileUtility.GetFileSize("132MB"), 132L * 1024 * 1024);
            Assert.AreEqual(FileUtility.GetFileSize("115.3 KB"), Convert.ToInt64(115.3 * 1024));
            Assert.AreEqual(FileUtility.GetFileSize("1.03      GB"), Convert.ToInt64(1.03 * 1024 * 1024 * 1024));
        }
        private void CreateFiles()
        {
            if (Directory.Exists("test"))
            {
                Directory.Delete("test", true);
            }
            var root = Directory.CreateDirectory("test");
            var dir1 = root.CreateSubdirectory("dir1");
            IOFile.WriteAllText(Path.Combine(dir1.FullName, "º½ÅÄ-1.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir1.FullName, "º½ÅÄ-2.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir1.FullName, "Æû³µ-1.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir1.FullName, "Æû³µ-2.mp4"), "");


            var dir2 = dir1.CreateSubdirectory("dir2");
            IOFile.WriteAllText(Path.Combine(dir2.FullName, "·É»ú-1.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir2.FullName, "Æû³µ-2.txt"), "");

            var dir3 = root.CreateSubdirectory("º½ÅÄ");
            IOFile.WriteAllText(Path.Combine(dir3.FullName, "1.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir3.FullName, "2.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir3.FullName, "1.jpg"), "");
            IOFile.WriteAllText(Path.Combine(dir3.FullName, "2.jpg"), "");

            var dir4 = root.CreateSubdirectory("Æû³µ");
            IOFile.WriteAllText(Path.Combine(dir4.FullName, "1.mp4"), "");
            IOFile.WriteAllText(Path.Combine(dir4.FullName, "2.jpg"), "");
            IOFile.WriteAllText(Path.Combine(dir4.FullName, "3.jpg"), "");
            IOFile.WriteAllText(Path.Combine(dir4.FullName, "4.mp4"), "");
        }
    }
}