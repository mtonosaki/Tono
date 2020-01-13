// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FTP操作
    /// </summary>
    public class FtpClient
    {
        private Uri _uri = null;
        private string _userid = "";
        private string _password = "";
        private string _path = "";

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public FtpClient()
        {
        }

        /// <summary>
        /// Ftpオープンしながらインスタンスを構築
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        public FtpClient(Uri uri, string userid, string password)
        {
            Open(uri, userid, password);
        }

        /// <summary>
        /// FTPクライアントをオープンする
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        public void Open(Uri uri, string userid, string password)
        {
            _uri = uri;
            _userid = userid;
            _password = password;
            GetHostWorkingDirectory();
        }

        private string makeuri(Uri uri, string path, string filename = "")
        {
            var sc = _uri.Scheme + "://";
            var ret = $"{_uri}/{_path}/{filename}";
            ret = ret.Replace(sc, "");
            while (true)
            {
                var ret2 = ret.Replace("//", "/");
                if (ret2 == ret)
                {
                    break;
                }
                else
                {
                    ret = ret2;
                }
            }
            if (ret.EndsWith("/"))
            {
                ret = StrUtil.Left(ret, ret.Length - 1);
            }
            return sc + ret;
        }

        /// <summary>
        /// ホスト側のワーキングディレクトリ名を返す（FTP URIのディレクトリとは異なる）
        /// </summary>
        /// <returns>ディレクトリ</returns>
        public string GetHostWorkingDirectory()
        {
            var req = getFtpRequest(WebRequestMethods.Ftp.PrintWorkingDirectory);
            var res = req.GetResponse() as FtpWebResponse;
            var dir = res.StatusDescription;
            dir = dir.Substring(dir.LastIndexOf(' ') + 2);
            dir = dir.Substring(0, dir.LastIndexOf('\"'));
            return dir;
        }

        private FtpWebRequest getFtpRequest(string method)
        {
            var req = (FtpWebRequest)WebRequest.Create(makeuri(_uri, _path));
            req.Credentials = new NetworkCredential(_userid, _password);
            req.Method = method;
            return req;
        }

        private FtpWebRequest getFtpRequest(string method, string filename)
        {
            var req = (FtpWebRequest)WebRequest.Create(makeuri(_uri, _path, filename));
            req.Credentials = new NetworkCredential(_userid, _password);
            req.Method = method;
            return req;
        }


        /// <summary>
        /// ディレクトリ内のファイルを一覧する
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ListDirectory()
        {
            var req = getFtpRequest(WebRequestMethods.Ftp.ListDirectory);
            var res = req.GetResponse() as FtpWebResponse;

            var parnedir = string.IsNullOrEmpty(_path) ? "" : _path.Substring(_path.LastIndexOf('/') + 1) + "/";

            using (var sr = new StreamReader(res.GetResponseStream()))
            {
                var list = sr.ReadToEnd();
                var ret = from t in list.Split('\n')
                          let fn = t.Replace("\r", "").Trim()
                          where string.IsNullOrEmpty(fn) == false
                          select string.IsNullOrEmpty(parnedir) == false ? fn.Replace(parnedir, "") : fn;
                return ret;
            }
        }

        /// <summary>
        /// 指定ディレクトリ
        /// </summary>
        public bool IsExist(string dirname)
        {
            var list = ListDirectory();
            return list.Contains(dirname);
        }

        /// <summary>
        /// カレントディレクトリに、新規フォルダを作成する
        /// </summary>
        /// <param name="dirname">ディレクトリ名  例：  data  （セパレーター等は付けない）</param>
        public void MakeAndChangeDirectory(string dirname)
        {
            if (IsExist(dirname))
            {
                _path += "/" + dirname;
                return;
            }
            _path += "/" + dirname;
            var req = (FtpWebRequest)WebRequest.Create(makeuri(_uri, _path));
            req.Credentials = new NetworkCredential(_userid, _password);
            req.Method = WebRequestMethods.Ftp.MakeDirectory;
            var res = req.GetResponse() as FtpWebResponse;
        }

        /// <summary>
        /// ファイルをアップロード
        /// </summary>
        /// <param name="localfilename">ローカルファイルフルパス</param>
        public void Upload(string localfilename)
        {
            Upload(localfilename, Path.GetFileName(localfilename));
        }

        /// <summary>
        /// ファイルをアップロード
        /// </summary>
        /// <param name="localfilename">ローカルファイルフルパス</param>
        /// <param name="newname">アップロード後のファイル名（パスは除く）</param>
        public void Upload(string localfilename, string newname)
        {
            if (string.IsNullOrEmpty(localfilename))
            {
                return;
            }
            if (File.Exists(localfilename) == false)
            {
                return;
            }
            var req = getFtpRequest(WebRequestMethods.Ftp.UploadFile, newname);
            req.UseBinary = true;
            var buf = File.ReadAllBytes(localfilename);
            var sw = new BinaryWriter(req.GetRequestStream());
            sw.Write(buf, 0, buf.Length);
            sw.Close();
            var res = req.GetResponse() as FtpWebResponse;
        }
    }
}
