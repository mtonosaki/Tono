// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Specialized;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// �ϐ������L����
    /// </summary>
    public class DataSharingManager
    {
        /// <summary>�g�p�֎~
        /// ���L�Ɏg���镶����^
        /// </summary>
        public class String
        {
            public string Value = "";
        }

        /// <summary>
        /// ���L�Ɏg���镶����^
        /// </summary>
        public class Object
        {
            public object value;
        }

        /// <summary>
        /// ���L�Ɏg����u�[���A���^
        /// </summary>
        public class Boolean
        {
            public bool value;
        }

        /// <summary>
        /// ���L�Ɏg���鐮���^
        /// </summary>
        public class Int
        {
            public int value = 0;
#if DEBUG
            public string _ => value.ToString();
#endif
            public static Int operator ++(Int tar)
            {
                tar.value++;
                return tar;
            }
            public static Int operator --(Int tar)
            {
                tar.value--;
                return tar;
            }

            /// <summary>
            /// int�^�ւ̃L���X�g�T�|�[�g
            /// </summary>
            public static implicit operator int(Int from)
            {
                return from.value;
            }

            public override int GetHashCode()
            {
                return value;
            }

            public override bool Equals(object obj)
            {
                return int.Equals(value, obj);
            }

            public override string ToString()
            {
                return value.ToString();
            }
        }

        #region �����i�V���A���C�Y����j

        /// <summary>�C���X�^���X���\������f�[�^</summary>
        private readonly IDictionary _dat = new HybridDictionary();

        #endregion

        /// <summary>
        /// �o�^�L�[�̌���
        /// </summary>
        public int Count => _dat.Count;

        /// <summary>
        /// ���L�ϐ����擾����
        /// </summary>
        /// <param name="name">���L�ϐ���</param>
        /// <param name="valueType">���L�ϐ��̒l�̌^</param>
        /// <returns>���L�ϐ��̃C���X�^���X</returns>
        public object Get(string name, Type valueType)
        {
            var ret = _dat[name];
            if (ret == null)
            {
                System.Diagnostics.Debug.Assert(valueType.IsSubclassOf(typeof(System.ValueType)) == false, "���L�ϐ�ffShare�Ɏg�p�ł���͎̂Q�ƌ^�����ł��B" + name + " �Ƃ������O�̋��L�ϐ��Ɏg�p���Ă���^��ύX���Ă�������");
                if (valueType.Equals(typeof(string)))
                {
                    ret = "";
                }
                else
                {
                    ret = Activator.CreateInstance(valueType, true);
                }
                _dat.Add(name, ret);
            }
            return ret;
        }

        /// <summary>
        /// ���L�ϐ����擾����(���݂��Ȃ�������A�w�肵���C���X�^���X�����蓖�Ă�)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object Get(string name, object instance)
        {
            var ret = _dat[name];
            if (ret == null)
            {
                System.Diagnostics.Debug.Assert(instance.GetType().IsSubclassOf(typeof(System.ValueType)) == false, "���L�ϐ�ffShare�Ɏg�p�ł���͎̂Q�ƌ^�����ł��B" + name + " �Ƃ������O�̋��L�ϐ��Ɏg�p���Ă���^��ύX���Ă�������");
                ret = instance;
                _dat.Add(name, ret);
            }
            return ret;
        }
    }
}
