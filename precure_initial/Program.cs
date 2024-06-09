using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace precure_initial
{
    class Program
    {
        /// <summary>
        /// rubicure/config のパス
        /// </summary>
        static readonly string rubicureConfigPath = "../../rubicure/config/";

        /// <summary>
        /// シリーズデータ.
        /// </summary>
        class Series
        {
            /// <summary>
            /// シリーズ名.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// シリーズに出演するプリキュアのキー.
            /// </summary>
            public List<string> PrecureKeys { get; set; }
        }

        static void Main(string[] args)
        {
            Dictionary<string, Series> seriesList = LoadSeries();
            foreach (var series in seriesList)
            {
                Console.WriteLine($"シリーズキー：{series.Key}");
                Console.WriteLine($"シリーズ名：{series.Value.Name}");
                Console.WriteLine($"プリキュアキー：[{string.Join(", ", series.Value.PrecureKeys)}]");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// rubicure/config/series.yml からシリーズキーとシリーズデータを取得する.
        /// </summary>
        /// <returns>シリーズキーをキーとする<code>Dictionary</code>.</returns>
        static Dictionary<string, Series> LoadSeries()
        {
            var filePath = rubicureConfigPath + "series.yml";
            var input = new StreamReader(filePath, Encoding.UTF8);
            var deserializer = new Deserializer();

            Dictionary<string, Dictionary<string, object>> series_tmp = deserializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(input);
            var series_directory = new Dictionary<string, Series>();
            foreach (var series in series_tmp)
            {
                if (series.Value.Count == 1)
                {
                    // エイリアスは飛ばす
                    continue;
                }
                series.Value.TryGetValue("title", out object seriesName);
                series.Value.TryGetValue("girls", out object precureList);
                List<string> precures = new List<string>();
                foreach (var precure in (List<object>)precureList)
                {
                    precures.Add((string)precure);
                }
                series_directory.Add(series.Key, new Series
                {
                    Name = (string)seriesName,
                    PrecureKeys = precures
                });
            }
            return series_directory;
        }
    }
}