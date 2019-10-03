using System;
using System.IO;
using System.Windows.Forms;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ファイル・フォルダ関係のユーティリティクラス
    /// </summary>
    public class FileUtil
    {
        /// <summary>
        /// Portforioや uMesのXML保存ディレクトリを取得する
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string MakeMesFilename(string filename)
        {
            string fname = null;
            var deploy = (string)AppDomain.CurrentDomain.GetData("DataDirectory");
            if (deploy == null || File.Exists(fname = Path.Combine(deploy, filename)) == false)
            {
                if (File.Exists(fname = Path.Combine(Application.StartupPath, filename)) == false)
                {
                    if (File.Exists(fname = Application.StartupPath + @"\..\..\" + filename) == false)
                    {
                        fname = Application.StartupPath + @"\..\..\..\" + filename;
                    }
                }
            }
            return fname;
        }

        /// <summary>
        /// 指定ディレクトリに移動。フォルダが無ければ作成する
        /// </summary>
        /// <param name="path"></param>
        public static void PrepareDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return;
            }
            var folders = path.Split(Path.DirectorySeparatorChar);
            var folder = "";
            foreach (var f in folders)
            {
                folder += f;
                if (Directory.Exists(folder) == false)
                {
                    Directory.CreateDirectory(folder);
                }
                folder += Path.DirectorySeparatorChar;
            }
        }
    }
}
