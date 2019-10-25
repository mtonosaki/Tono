// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeaturePaneSync の概要の説明です。
    /// </summary>
    public class FeaturePaneSync : FeatureBase, IZoomListener, IScrollListener, IDisposable, IAutoRemovable
    {
        private IRichPane[] _tarPanes;
        private int _pid;
        private bool isReading = false;
        private string _fname;

        public override void OnInitInstance()
        {
            base.OnInitInstance();

            _tarPanes = new IRichPane[] { Pane };
            _pid = System.Diagnostics.Process.GetCurrentProcess().Id;
            _fname = getFileName();
            Timer.AddTrigger(1200, new GuiTimer.Proc0(timerProc));
        }

        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                if (base.Enabled == false && value == true)
                {
                    Timer.AddTrigger(200, new GuiTimer.Proc0(timerProc));
                }
                base.Enabled = value;
            }
        }


        /// <summary>
        /// 記録用ファイル名を作成する
        /// </summary>
        protected virtual string getFileName()
        {
            var t = Environment.GetEnvironmentVariable("Temp");
            var proc = System.Diagnostics.Process.GetCurrentProcess();

            if (string.IsNullOrEmpty(t))
            {
                var ass = System.Reflection.Assembly.GetEntryAssembly();
                t = ass.CodeBase;
                var id = t.LastIndexOf('/');
                t = t.Substring(0, id);
            }
            var fn = new Uri(t + "/FeaturePaneSync.dat");
            return fn.LocalPath;
        }

        /// <summary>
        /// タイマーでスクロール適用を判断して、同期する
        /// </summary>
        private void timerProc()
        {
            Stream st = null;
            try
            {
                if (File.Exists(_fname) && Enabled)
                {
                    st = new FileStream(_fname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    isReading = true;
                    st.Seek(0, SeekOrigin.Begin);
                    SerializerEx.ReceiveDirect(st, out int pid);
                    if (pid != _pid)
                    {
                        SerializerEx.ReceiveDirect(st, out int sx);
                        SerializerEx.ReceiveDirect(st, out int sy);
                        SerializerEx.ReceiveDirect(st, out int zx);
                        SerializerEx.ReceiveDirect(st, out int zy);
                        var sc = ScreenPos.FromInt(sx, sy);
                        var isChanged = false;
                        if (Pane.Scroll.Equals(sc) == false)
                        {
                            Pane.Scroll = sc;
                            isChanged = true;
                        }
                        var zo = ScreenPos.FromInt(zx, zy);
                        if (Pane.Zoom.Equals(zo) == false)
                        {
                            Pane.Zoom = zo;
                            isChanged = true;
                        }
                        if (isChanged)
                        {
                            Pane.Invalidate(null);
                        }
                    }
                }
            }
            catch (IOException)
            {
            }
            finally
            {
                if (st != null)
                {
                    st.Close();
                }
            }
            isReading = false;
            if (Enabled)
            {
                Timer.AddTrigger(420, new GuiTimer.Proc0(timerProc));
            }
        }

        public void ZoomChanged(IRichPane rp)
        {
            if (isReading)
            {
                return;
            }
            Stream st = null;
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    st = new FileStream(_fname, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                    st.Seek(0, SeekOrigin.Begin);
                    SerializerEx.SendDirect(st, _pid);
                    SerializerEx.SendDirect(st, Pane.Scroll.X);
                    SerializerEx.SendDirect(st, Pane.Scroll.Y);
                    SerializerEx.SendDirect(st, Pane.Zoom.X);
                    SerializerEx.SendDirect(st, Pane.Zoom.Y);
                    break;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(10);
                }
            }
            if (st != null)
            {
                st.Close();
            }
        }

        #region IZoomListener メンバ

        public IRichPane[] ZoomEventTargets => _tarPanes;

        #endregion

        #region IScrollListener メンバ

        public IRichPane[] ScrollEventTargets => _tarPanes;

        public void ScrollChanged(IRichPane rp)
        {
            ZoomChanged(rp);
        }

        #endregion

        #region IDisposable メンバ

        public override void Dispose()
        {
            try
            {
                File.Delete(_fname);
            }
            catch (Exception)
            {
            }
        }

        #endregion
    }
}
