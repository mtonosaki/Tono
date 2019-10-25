// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// ��������T�|�[�g���镶����
    /// </summary>
    /// <remarks>
    /// StrUtilingw = utility class, String, Worldwide
    /// </remarks>
    [Serializable]
    public class StringWorld : IDisposable, ICloneable, ISerializable
    {
        private Dictionary<string, string> _dat = new Dictionary<string, string>();

        #region ICloneable �����o

        public object Clone()
        {
            var ret = new StringWorld
            {
                _dat = new Dictionary<string, string>(_dat)
            };
            return ret;
        }

        #endregion

        public override string ToString()
        {
            var cd = Mes.Current.GetCode();
            if (_dat.ContainsKey(cd))
            {
                return _dat[cd];
            }
            else
            {
                if (_dat.Count > 0)
                {
                    foreach (var s in _dat.Values)
                    {
                        return s;
                    }
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        /// <summary>
        /// �R���X�g���N�^�͂Ȃ��B�����񂩂�̃L���X�g�̃I�[�o�[���C�h���g���Ă�������
        /// </summary>
        private StringWorld()
        {
            _init();
        }

        private void _init()
        {
            Mes.Current.CodeChanged += new Mes.CodeChangedEventHandler(Current_CodeChanged);
        }

        #region ISerializable �����o

        protected StringWorld(SerializationInfo info, StreamingContext context)
        {
            _init();
            var sd = (List<KeyValuePair<string, string>>)info.GetValue("_dat_KV", typeof(List<KeyValuePair<string, string>>));
            foreach (var kv in sd)
            {
                _dat[kv.Key] = kv.Value;
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            var sd = new List<KeyValuePair<string, string>>(_dat);
            info.AddValue("_dat_KV", sd);
        }

        #endregion

        /// <summary>
        /// ���݂̌���Ƀe�L�X�g���Z�b�g����B���̌���͏�������
        /// </summary>
        /// <param name="s"></param>
        public void SetToAllLanguage(string s)
        {
            _dat.Clear();
            this[Mes.Current.GetCode()] = s;
        }

        /// <summary>
        /// ���ꂪ�ς�����Ƃ��A�O����̒l�����̂܂܎g����悤�ɂ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_CodeChanged(object sender, Mes.CodeChangedEventArgs e)
        {
            if (_dat.ContainsKey(e.NewCode) == false)
            {
                _dat[e.NewCode] = _dat[e.OldCode];
            }
        }

        /// <summary>
        /// ����R�[�h���w�肵��������
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string key]
        {
            get
            {
                if (_dat.ContainsKey(key))
                {
                    return _dat[key];
                }
                if (_dat.ContainsKey("en"))
                {
                    return _dat["en"];
                }
                if (_dat.ContainsKey("jp"))
                {
                    return _dat["jp"];
                }
                foreach (var val in _dat.Values)
                {
                    return val;
                }
                return "??";
            }
            set => _dat[key] = value;
        }

        /// <summary>
        /// �W���������p���ĕԂ�
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public static implicit operator string(StringWorld tar)
        {
            return tar[Mes.Current.GetCode()];
        }

        /// <summary>
        /// �����񂩂�C���X�^���X�����
        /// </summary>
        /// <param name="tar"></param>
        /// <returns></returns>
        public static implicit operator StringWorld(string tar)
        {
            var ret = new StringWorld();
            ret[Mes.Current.GetCode()] = tar;
            return ret;
        }

        #region IDisposable �����o

        public void Dispose()
        {
            if (_dat != null)
            {
                Mes.Current.CodeChanged -= new Mes.CodeChangedEventHandler(Current_CodeChanged);
                _dat = null;
            }
        }

        #endregion
    }
}
