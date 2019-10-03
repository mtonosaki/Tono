using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NoTestClassAttribute : Attribute
    {
        public NoTestClassAttribute()
        {
        }
    }

    /// <summary>
    /// NoTestClass�̃A�g���r���[�g
    /// �e�X�g���s��Ȃ��ėǂ��N���X�ɂ��鑮��
    /// </summary>
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false)]
    public class NoTestAttribute : Attribute
    {
        public NoTestAttribute()
        {
        }
    }
}
