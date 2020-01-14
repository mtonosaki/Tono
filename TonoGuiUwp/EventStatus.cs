// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tono.Gui.Uwp
{
    /// <summary>
    /// EventToken status (to filter event)
    /// </summary>
    public class EventStatus
    {
        public const string True = "True";
        public const string False = "False";

        /// <summary>
        /// Status name
        /// </summary>
        public string Name { get; set; }

        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        private string _value = null;
        private Func<string> _valueGenerator = null;

        /// <summary>
        /// lazy get value (if null, use Value directly)
        /// </summary>
        public Func<string> ValueGenerator
        {
            set => _valueGenerator = value;
        }

        /// <summary>
        /// add values
        /// </summary>
        /// <param name="values"></param>
        public void AddValues(IEnumerable<string> values)
        {
            foreach (var val in values)
            {
                _values[val] = null;
            }
        }

        /// <summary>
        /// add a value as boolean
        /// </summary>
        /// <param name="defaultValue"></param>
        public void AddBooleanValues(bool defaultValue = false)
        {
            AddValues(new[] { False, True });

            var defval = defaultValue ? True : False;
            _value = defval;    // for no loggin
            Value = defval;
        }

        /// <summary>
        /// add values and objects that link to value
        /// </summary>
        /// <param name="values"></param>
        public void AddValues(IEnumerable<(string Name, object Obj)> values)
        {
            foreach (var val in values)
            {
                _values[val.Name] = val.Obj;
            }
        }


        /// <summary>
        /// status value
        /// </summary>
        /// <remarks>
        /// NOTE: null is not "no value". null means not ready
        /// 注意：値なしを null で代用しない事。値なしもステータスという考えのもと、Valuesで管理すること。 null は、ステータス準備未完了という意味とする。
        /// </remarks>
        public string Value
        {
            get => _valueGenerator?.Invoke() ?? _value;

            set
            {
                if (_values.ContainsKey(value) == false)
                {
                    throw new ArgumentOutOfRangeException();
                }
                if (_value != value)
                {
                    _value = value;
                    Debug.WriteLine($"{DateTime.Now.ToString(TimeUtil.FormatYMDHMSms)} ttStatus.{Name} = {value}");
                }
            }
        }

        /// <summary>
        /// value as boolean
        /// </summary>
        public bool ValueB
        {
            get
            {
                if (False.Equals(Value))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            set => Value = value ? True : False;
        }

        /// <summary>
        /// check the status is on specified one.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsOn(string value)
        {
            return Value?.Equals(value) ?? false;
        }

        /// <summary>
        /// NOT IsOn
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsNotOn(string value)
        {
            return !IsOn(value);
        }

        /// <summary>
        /// get object linked to value
        /// </summary>
        public (string Name, object Obj) ValueObject
        {
            get => (_value, _values[_value]);
            set
            {
                if (_values.ContainsKey(value.Name) == false)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _value = value.Name;
                _values[_value] = value.Obj;
            }
        }

        /// <summary>
        /// execute action when value is equal to specified one.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="action"></param>
        public void Select(string value, Action action)
        {
            if (_value?.Equals(value) ?? false)
            {
                action?.Invoke();
            }
        }
    }
}
