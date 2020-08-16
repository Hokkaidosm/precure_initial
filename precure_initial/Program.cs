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

        /// <summary>
        /// プリキュアデータ.
        /// </summary>
        class Precure
        {
            /// <summary>
            /// プリキュア英語名.
            /// </summary>
            public string GirlName { get; set; }

            /// <summary>
            /// プリキュア日本語名.
            /// </summary>
            public string PrecureName { get; set; }

            /// <summary>
            /// イニシャル.
            /// </summary>
            public string Initial { get; set; }
        }

        static void Main(string[] args)
        {
            Dictionary<string, Series> seriesList = LoadSeries();
            Dictionary<string, Precure> precureList = LoadPrecures();
            foreach (var series in seriesList)
            {
                OutputPrecureList(series.Value.Name, series.Value.PrecureKeys, precureList);
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
                series_directory.Add(series.Key, new Series {
                    Name = (string)seriesName,
                    PrecureKeys = precures
                });
            }
            return series_directory;
        }

        /// <summary>
        /// 全てのプリキュアのデータを読み込む.
        /// </summary>
        /// <returns>プリキュアキーをキーとする<code>Dictionary</code>.</returns>
        static Dictionary<string, Precure> LoadPrecures()
        {
            Dictionary<string, Precure> precures = new Dictionary<string, Precure>();
            var rootPath = rubicureConfigPath + "girls/";
            string[] files = Directory.GetFiles(rootPath, "*.yml", SearchOption.TopDirectoryOnly);
            foreach (string file in files)
            {
                var input = new StreamReader(file, Encoding.UTF8);
                var deserializer = new Deserializer();
                Dictionary<string, Dictionary<string, object>> girls_tmp = deserializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(input);
                foreach (var girl in girls_tmp)
                {
                    if (girl.Value.Count == 1)
                    {
                        // エイリアスは飛ばす
                        continue;
                    }
                    string girlName = girl.Key, precureName = null;
                    if (girl.Value.TryGetValue("precure_name", out object precureNameObj))
                    {
                        precureName = (string)precureNameObj;
                    }
                    girlName = ToPascal(girlName);
                    string initial = girlName.Replace("Cure ", "").Substring(0, 1);
                    precures.Add(girl.Key, new Precure {
                        GirlName = girlName,
                        PrecureName = precureName,
                        Initial = initial
                    });
                }
            }
            return precures;
        }

        /// <summary>
        /// プリキュアリストを出力する.
        /// </summary>
        /// <param name="seriesName">シリーズ名</param>
        /// <param name="precures">シリーズプリキュアキーリスト</param>
        /// <param name="precureList">プリキュアリスト</param>
        static void OutputPrecureList(string seriesName, List<string> precures, Dictionary<string, Precure> precureList)
        {
            Console.WriteLine(seriesName);
            Console.WriteLine("プリキュア名,英語,頭文字");
            // イニシャルリスト
            List<string> precureInitials = new List<string>();
            // イニシャル重複チェック
            bool initialsConflict = false;
            foreach (var precureKey in precures)
            {
                if (!precureList.TryGetValue(precureKey, out Precure precure))
                {
                    // 万が一取得できなかった場合はスキップする
                    continue;
                }
                if (precureInitials.Contains(precure.Initial))
                {
                    initialsConflict = true;
                }
                precureInitials.Add(precure.Initial);
                Console.WriteLine($"{precure.PrecureName},{precure.GirlName},{precure.Initial}");
            }
            Console.WriteLine($"人数：{precureInitials.Count}");
            Console.WriteLine($"かぶり：{(initialsConflict ? "あり" : "なし")}");
            Console.WriteLine();
        }

        /// <summary>
        /// cure_black => Cure Blackのような変換
        /// ref. https://increment-i.hateblo.jp/entry/csharp/regularexpression/pascal
        /// </summary>
        /// <param name="text">変換元</param>
        /// <returns>変換結果</returns>
        private static string ToPascal(string text)
        {
            return Regex.Replace(
                text.Replace("_", " "),
                @"\b[a-z]",
                match => match.Value.ToUpper());
        }
    }
}
