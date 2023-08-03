using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DoenaSoft.MediaInfoHelper.DataObjects.VideoMetaXml;
using DoenaSoft.MediaInfoHelper.Helpers;

namespace DoenaSoft.SeriesList
{
    public static class SeriesList
    {
        private static readonly XmlSerializer _serializer;

        static SeriesList()
        {
            _serializer = new XmlSerializer(typeof(Doc));
        }

        public static void Main()
        {
            //System.Diagnostics.Debugger.Launch();

            var folders = GetFolders().ToList();

            //System.Diagnostics.Debugger.Launch();

            folders.Sort(CompareTitle);

            WriteFolders(folders);

            //File.Copy(TargetFile, @"T:\CompleteSeriesList.txt", true);
        }

        private static Dictionary<string, string> GetFolders()
        {
            var roots = new[] { @"N:\Drive1\TVShows\", @"N:\Drive2\TVShows\", @"N:\Drive3\TVShows\" };

            var tasks = new List<Task<List<KeyValuePair<string, string>>>>();

            foreach (var root in roots)
            {
                tasks.Add(Task.Run(() => GetFolders(root)));
            }

            Task.WaitAll(tasks.ToArray());

            var folderInfos = tasks.SelectMany(t => t.Result).ToList();

            var folders = new Dictionary<string, string>(folderInfos.Count);

            foreach (var folderInfo in folderInfos)
            {
                folders[folderInfo.Key] = folderInfo.Value;
            }

            return folders;
        }

        private static List<KeyValuePair<string, string>> GetFolders(string root)
        {
            var folderInfos = new List<KeyValuePair<string, string>>();

            if (Directory.Exists(root))
            {
                GetFolders(root, folderInfos);
            }

            return folderInfos;
        }

        private static void GetFolders(string root, List<KeyValuePair<string, string>> folderInfos)
        {
            var folders = Directory.GetDirectories(root, "*.*", SearchOption.AllDirectories);

            foreach (var folder in folders)
            {
                if (!folder.Contains("Season ") && !folder.Contains("Staffel "))
                {
                    continue;
                }
                else if (!EndsWithNumber(folder))
                {
                    continue;
                }

                folderInfos.Add(new KeyValuePair<string, string>(folder.Replace(root, string.Empty), folder));
            }
        }

        private static void WriteFolders(List<KeyValuePair<string, string>> folders)
        {
            const string TargetFile = @"N:\Drive1\TVShows\CompleteSeriesList.txt";

            using (var streamWriter = new StreamWriter(TargetFile, false, Encoding.GetEncoding("Windows-1252")))
            {
                if (folders.Count > 0)
                {
                    WriteFolders(folders, streamWriter);
                }
            }
        }

        private static void WriteFolders(List<KeyValuePair<string, string>> folders, StreamWriter streamWriter)
        {
            var split = folders[0].Key.Split('\\');

            var previousTitle = split[0];

            streamWriter.WriteLine(previousTitle);

            foreach (var folder in folders)
            {
                WriteFolder(folder, streamWriter, ref previousTitle);
            }
        }

        private static void WriteFolder(KeyValuePair<string, string> folder, StreamWriter streamWriter, ref string previousTitle)
        {
            var split = folder.Key.Split('\\');

            if (split[0] != previousTitle)
            {
                streamWriter.WriteLine();

                previousTitle = split[0];

                streamWriter.WriteLine(previousTitle);
            }

            var season = folder.Key.Substring(previousTitle.Length + 1);

            streamWriter.Write("\t");
            streamWriter.Write(season);

            var languages = GetLanguages(folder.Value);

            streamWriter.WriteLine(languages);
        }

        private static string GetLanguages(string folder)
        {
            var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);

            var tasks = new List<Task<List<string>>>(files.Length);

            foreach (var file in files)
            {
                tasks.Add(Task.Run(() => TryAddLanguages(file)));
            }

            Task.WaitAll(tasks.ToArray());

            var languages = tasks
                .SelectMany(t => t.Result)
                .StandardizeLanguage()
                .OrderBy(LanguageExtensions.GetLanguageWeight)
                .ToList();

            var text = string.Empty;

            if (languages.Count == 1)
            {
                text = $" (Language: {languages[0]})";
            }
            else if (languages.Count > 1)
            {
                text = $" (Languages: {string.Join(", ", languages)})";
            }

            return text;
        }

        private static List<string> TryAddLanguages(string infoFile)
        {
            try
            {
                using (var fs = new FileStream(infoFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var doc = (Doc)_serializer.Deserialize(fs);

                    var languages = doc.VideoInfo?.Audio?.Select(a => a.Language) ?? Enumerable.Empty<string>();

                    return languages.ToList();
                }
            }
            catch
            {
                return new List<string>(0);
            }
        }

        private static bool EndsWithNumber(string folder)
        {
            var endsWithNumber = folder.EndsWith("0") || folder.EndsWith("1") || folder.EndsWith("2") || folder.EndsWith("3") || folder.EndsWith("4")
                || folder.EndsWith("5") || folder.EndsWith("6") || folder.EndsWith("7") || folder.EndsWith("8") || folder.EndsWith("9");

            if (endsWithNumber && (folder.EndsWith("mp4") || folder.EndsWith("mp3")))
            {
                endsWithNumber = false;
            }

            return endsWithNumber;
        }

        private static int CompareTitle(KeyValuePair<string, string> left, KeyValuePair<string, string> right)
        {
            //System.Diagnostics.Debugger.Launch();

            var leftTitle = left.Key;

            var rightTitle = right.Key;

            ProcessTitle(ref leftTitle);
            ProcessTitle(ref rightTitle);

            var compare = leftTitle.CompareTo(rightTitle);

            return compare;
        }

        private static void ProcessTitle(ref string title)
        {
            var split = title.Split(' ', '\\');

            title = string.Empty;

            var start = 0;

            if (split[0].Equals("the", StringComparison.CurrentCultureIgnoreCase))
            {
                start = 1;
            }
            else if (split[0].Equals("a", StringComparison.CurrentCultureIgnoreCase))
            {
                start = 1;
            }
            else if (split[0].Equals("der", StringComparison.CurrentCultureIgnoreCase))
            {
                start = 1;
            }

            for (var splitIndex = start; splitIndex < split.Length; splitIndex++)
            {
                if (int.TryParse(split[splitIndex], out var value))
                {
                    split[splitIndex] = value.ToString("0000");
                }

                title += split[splitIndex] + " ";
            }
        }
    }
}