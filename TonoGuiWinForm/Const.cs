// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// uConst �̊T�v�̐����ł��B
    /// </summary>
    public static class Const
    {
        /// <summary>�����[���̔z��</summary>
        public static readonly ICollection ZeroCollection = Array.Empty<object>();

        /// <summary>
        /// �����[���̃��X�g
        /// </summary>
        public static readonly IList ZeroList = new ArrayList(0);

        /// <summary>
        /// �ԓ����a[mm]
        /// </summary>
        public static readonly double EarthRadiusX = 6378137000;

        /// <summary>
        /// �ɔ��a[mm]
        /// </summary>
        public static readonly double EarthRadiusY = 6356752000;

        /// <summary>
        /// ������̃t�H�[�}�b�g
        /// </summary>
        public static class Formatter
        {
            public static string Size(long size)
            {
                if (size < 1000)
                {
                    return size.ToString();
                }
                if (size < 1024 * 1024)
                {
                    return (size / 1024).ToString() + "k";
                }
                if (size < 1024 * 1024 * 1024)
                {
                    return (size / 1024.0 / 1024.0).ToString("0.0") + "M";
                }
                return (size / 1024.0 / 1024.0 / 1024.0).ToString("0.00") + "G";
            }
        }

        /// <summary>
        /// ���C���[
        /// </summary>
        public static class Layer
        {
            /// <summary>�f�o�C�X�v���C���[���g�p����t���[���C���[</summary>
            public const int DevicePlayer = 60001;

            /// <summary>�c�[���`�b�v</summary>
            public const int Tooltip = 79003;

            /// <summary>
            /// ����ȃp�[�c�iClear�������Ȃ����́j��p�̃��C���[
            /// </summary>
            public static class StaticLayers
            {
                // ���O�\���p�p�l���`��
                public const int LogPanel = 79008;

                // �I��̈�(FeaturePartsSelectOnRect)
                public const int MaskRect = 79007;

                // �X�N���[���o�[(FeatureScrollBarHorz/FeatureScrollBarVert)
                public const int ScrollBarH = 79001;
                public const int ScrollBarV = 79002;

                /// <summary>
				/// ����ȃp�[�c�iClear�������Ȃ����́j���ǂ����H
				/// </summary>
				/// <param name="layer">�����Ώ�</param>
				/// <returns></returns>
				public static bool IsStaticLayers(int layer)
                {
                    switch (layer)
                    {
                        case LogPanel:
                        case MaskRect:
                            return true;
                        default:
                            return false;
                    }
                }
            }
        }

        /// <summary>
        /// ������True�������Ă��邩�ǂ����𒲂ׂ�
        /// </summary>
        /// <param name="s">������</param>
        /// <returns>����</returns>
        /// <remarks>!IsTrue() != IsFalse()�ł��邱�Ƃɒ��ӂ���</remarks>
        public static bool IsTrue(string s)
        {
            var ss = s.ToLower();
            if (ss == "1" || ss == "true" || ss == "on" || ss == "��" || ss == "�^" || ss == "ok" || ss == "yes" || ss == "y")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ��������������S�� true�𔻒f����
        /// </summary>
        /// <param name="val">�������� / 0 �ɋ߂���� false�A�����łȂ����true</param>
        /// <returns>true / false</returns>
        public static bool IsTrue(double val)
        {
            if (Math.Abs(val) < 0.01)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ������False�������Ă��邩�ǂ����𒲂ׂ�
        /// </summary>
        /// <param name="s">������</param>
        /// <returns>����</returns>
        /// <remarks>!IsFalse() != IsTrue()�ł��邱�Ƃɒ��ӂ���</remarks>
        public static bool IsFalse(string s)
        {
            var ss = s.ToLower();
            if (ss == "0" || ss == "false" || ss == "off" || ss == "��" || ss == "�U" || ss == "ng" || ss == "no" || ss == "n")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ��������������S�� true�𔻒f����
        /// </summary>
        /// <param name="val">�������� / 0 �ɋ߂���� false�A�����łȂ����true</param>
        /// <returns>true / false</returns>
        public static bool IsFalse(double val)
        {
            return !IsTrue(val);
        }

        /// <summary>���s�R�[�h</summary>
        public const string CR = "\r\n";

        /// <summary>
        /// �w��R���N�V��������ЂƂ擾����
        /// </summary>
        /// <param name="col">�R���N�V����</param>
        /// <returns>�R���N�V�������̂ЂƂ�</returns>
        public static object GetOne(ICollection col)
        {
            var e = col.GetEnumerator();
            if (e.MoveNext())
            {
                return e.Current;
            }
            return null;
        }
    }
}
