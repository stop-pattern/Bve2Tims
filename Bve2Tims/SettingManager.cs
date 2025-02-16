using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bve2Tims
{
    /// <summary>
    /// 設定ファイルのシリアライズ用クラス
    /// </summary>
    [XmlRoot]
    public class Settings
    {
        /// <summary>
        /// ユニットのインデックス
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Index")]
        public int[] UnitIndexes { get; set; }

        /// <summary>
        /// ドアのインデックス
        /// </summary>
        [XmlArray]
        [XmlArrayItem("Index")]
        public int[] DoorIndexes { get; set; }

        /// <summary>
        /// UDP通信の設定
        /// </summary>
        [XmlArray]
        [XmlArrayItem("UDPSetting")]
        public Udp[] Udps { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public Settings()
        {
            UnitIndexes = new int[0];
            DoorIndexes = new int[0];
            Udps = new Udp[0];
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="door"></param>
        /// <param name="udp"></param>
        public Settings(int[] unit, int[] door, Collection<Udp> udp)
        {
            UnitIndexes = unit;
            DoorIndexes = door;
            Udps = udp.ToArray();
        }

        /// <summary>
        /// <see cref="Model"/> から変換
        /// </summary>
        /// <param name="model">変換元の <see cref="Model"/> インスタンス</param>
        /// <returns></returns>
        public static Settings FromModel(Model model)
        {
            return new Settings(Model.UnitIndexes.ToArray(), Model.DoorIndexes.ToArray(), model.Destinations);
        }

        /// <summary>
        /// <see cref="Model"/> に変換
        /// </summary>
        /// <returns>変換元の <see cref="Model"/> インスタンス</returns>
        public static Model ToModel(Settings settings)
        {
            Model.UnitIndexes = settings.UnitIndexes;
            Model.DoorIndexes = settings.DoorIndexes;
            return new Model(settings.Udps);
        }
    }

    class SettingManager
    {
        #region Static Methods

        /// <summary>
        /// 保存先のファイルパスを動的に取得
        /// このdllのファイルパス - ".dll" + ".Settings.xml"
        /// </summary>
        /// <returns>保存先のファイルパス</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static string GetSettingsFilePath()
        {
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("アセンブリのディレクトリを取得できません。");
            string fileName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, $"{fileName}.Settings.xml");
        }

        /// <summary>
        /// <see cref="Model"/> のプロパティをXMLファイルに保存
        /// </summary>
        /// <param name="model">保存する <see cref="Model"/> インスタンス</param>
        public static void Save(Model model)
        {
            try
            {
                string filePath = GetSettingsFilePath();

                using (var writer = new StreamWriter(filePath))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    var temp = Settings.FromModel(model);
                    serializer.Serialize(writer, temp);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error saving settings: {ex.Message}");
            }
        }

        /// <summary>
        /// XMLファイルから <see cref="Model"/> のプロパティを読み込み
        /// </summary>
        /// <returns>読み込まれた <see cref="Model"/> インスタンス</returns>
        public static Model Load()
        {
            try
            {
                string filePath = GetSettingsFilePath();

                if (!File.Exists(filePath)) return new Model();

                using (var reader = new StreamReader(filePath))
                {
                    var serializer = new XmlSerializer(typeof(Settings));
                    var serializableObject = (Settings)serializer.Deserialize(reader);
                    return Settings.ToModel(serializableObject);
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error loading settings: {ex.Message}");
                return new Model();
            }
        }

        #endregion
    }
}
