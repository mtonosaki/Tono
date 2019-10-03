using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// DB�̃X�L�[�}�ł��邱�Ƃ𖾎�����A�g���r���[�g
    /// </summary>
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DBTableClassAttribute : Attribute
    {
        private readonly string _name;

        public string Name => _name;
        public DBTableClassAttribute(string name)
        {
            _name = name;
        }
    }

    /// <summary>
    /// DB�̃X�L�[�}�ł��邱�Ƃ𖾎�����A�g���r���[�g
    /// </summary>
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DBSchemaAttribute : Attribute
    {
        private readonly string _name;

        public string Name => _name;
        public DBSchemaAttribute(string name)
        {
            _name = name;
        }
    }

    /// <summary>
    /// DB�̃X�L�[�}�łȂ����Ƃ𖾎�����A�g���r���[�g
    /// </summary>
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DBNonSchemaAttribute : Attribute
    {
        public DBNonSchemaAttribute()
        {
        }
    }

    /// <summary>
    /// DB�֘A�����w�肷��A�g���r���[�g
    /// </summary>
    [NoTestClass]
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class DBRelationAttribute : Attribute
    {
        private readonly string _table;
        private readonly string _col;

        public string TableName => _table;
        public string ColumnName => _col;


        /// <summary>
        /// �������w�肷��R���X�g���N�^
        /// </summary>
        /// <param name="table">�e�[�u����</param>
        /// <param name="col">������</param>
        public DBRelationAttribute(string table, string col)
        {
            _table = table;
            _col = col;
        }
    }
}
