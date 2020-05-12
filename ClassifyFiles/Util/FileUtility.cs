using ClassifyFiles.Data;
using System;
using System.Collections.Generic;
using Dir = System.IO.Directory;
using FI = System.IO.FileInfo;
using DI = System.IO.DirectoryInfo;
using SO = System.IO.SearchOption;
using System.Text;
using System.Linq;

namespace ClassifyFiles.Util
{
    public static class FileUtility
    {
        public static Dictionary<Class,List<File>> GetFiles(DI dir,IEnumerable<Class> classes)
        {
            Dictionary<Class, List<File>> classFiles = new Dictionary<Class, List<File>>();
            foreach (var c in classes)
            {
                classFiles.Add(c, new List<File>());
            }
            var files = dir.EnumerateFiles("*", SO.AllDirectories);
            foreach (var file in files)
            {
                foreach (var c in classes)
                {
                    if (IsMatched(file, c))
                    {
                        classFiles[c].Add(new File(file,dir,c));
                    }
                }
            }
            return classFiles;
        }

        private static bool IsMatched(FI file,Class c)
        {
            List<List<MatchCondition>> orGroup = new List<List<MatchCondition>>();
            for(int i=0;i<c.MatchConditions.Count;i++)
            {
                var mc = c.MatchConditions[i];
                if(i==0)//首项直接加入列表
                {
                    orGroup.Add(new List<MatchCondition>() { mc });
                    continue;
                }

                //第二项开始，如果是“与”，则加入列表的最后一项列表中；如果是或，则新建一个列表项
                switch (mc.ConnectionLogic)
                {
                    case Logic.And:
                        orGroup.Last().Add(mc);
                        break;
                    case Logic.Or:
                        orGroup.Add(new List<MatchCondition>() { mc });
                        break;
                }
            }
            //此时，分类已经完毕。相邻的“与”为一组，每一组之间为“或”的关系。

            foreach (var andGroup in orGroup)
            {
                bool andResult=true;
                foreach (var and in andGroup)
                {
                    if(!IsMatched(file,and))
                    {
                        andResult = false;
                        break;
                    }
                }
                if(andResult)//如果and组通过了，那么直接通过
                {
                    return true;
                }
            }
            return false;//如果每一组or都不通过，那么只好不通过
        }
        private static bool IsMatched(FI file,MatchCondition mc)
        {

            string value = mc.Value;
            switch (mc.Type)
            {
                case MatchType.InFileName:
                    return file.Name.Contains(value);
                case MatchType.InDirName:
                    return file.DirectoryName.Contains(value);
                case MatchType.WithExtension:
                    return file.Extension.Contains(value);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
