// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �J�[�\����҂��ɐݒ肷��
    /// �X���b�h�Z�[�t�݌v
    /// </summary>
    /// <example>
    /// using(new WaitCursor(this))
    /// {
    ///		�Ȃ�������
    /// }
    /// </example>
    public class WaitCursor : IDisposable
    {
        private Control _target = null;
        private readonly Cursor _bak = null;

        public WaitCursor(Control tar)
        {
            _target = tar;
            _bak = tar.Cursor;
            if (tar.InvokeRequired)
            {
                var th = new ThreadUtil();
                th.SetCursorControl(tar, Cursors.WaitCursor);
            }
            else
            {
                tar.Cursor = Cursors.WaitCursor;
            }
        }

        ~WaitCursor()
        {
            Dispose();
        }

        #region IDisposable �����o

        public void Dispose()
        {
            if (_target != null)
            {
                if (_target.InvokeRequired)
                {
                    var th = new ThreadUtil();
                    th.SetCursorControl(_target, _bak);
                }
                else
                {
                    _target.Cursor = _bak;
                }
                _target = null;
            }
        }

        #endregion
    }
}
