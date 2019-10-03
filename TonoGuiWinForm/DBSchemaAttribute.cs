using System;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// DBのスキーマであることを明示するアトリビュート
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
    /// DBのスキーマであることを明示するアトリビュート
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
    /// DBのスキーマでないことを明示するアトリビュート
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
    /// DB関連情報を指定するアトリビュート
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
        /// 属性を指定するコンストラクタ
        /// </summary>
        /// <param name="table">テーブル名</param>
        /// <param name="col">属性名</param>
        public DBRelationAttribute(string table, string col)
        {
            _table = table;
            _col = col;
        }
    }
}
