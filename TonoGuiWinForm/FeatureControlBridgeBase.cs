// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// .NET Form �̃R���g���[���ƃC�x���g�E�v���p�e�B��ʐM�����{�N���X
    /// </summary>
    public abstract class FeatureControlBridgeBase : FeatureBase, IDisposable
    {
        #region �����i�V���A���C�Y����j

        /// <summary>�g���K����ۑ�����z��</summary>
        private Dictionary<string, IList<EventUnit>> _addEvents = null;

        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        private readonly ThreadUtil _ts = new ThreadUtil();
        #endregion

        internal class EventUnit
        {
            public EventInfo ei;
            public Component control;
            public object function;

            public EventUnit(EventInfo e, Component c, object f)
            {
                ei = e;
                control = c;
                function = f;
            }
        }

        /// <summary>
        /// �X���b�h�Z�[�t�Ǘ�
        /// </summary>
        protected ThreadUtil ThreadSafe => _ts;

        /// <summary>
        /// �R���g���[������������
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private Control _getProc(string name, Control value)
        {
            if (value.ContextMenuStrip != null)
            {
                if (value.ContextMenuStrip.Name == name)
                {
                    return value.ContextMenuStrip;
                }
            }
            foreach (Control c in value.Controls)
            {
                if (c.Name == name)
                {
                    return c;
                }
                if (c.ContextMenuStrip != null)
                {
                    if (c.ContextMenuStrip.Name == name)
                    {
                        return c.ContextMenuStrip;
                    }
                }
                var cc = _getProc(name, c);
                if (cc != null)
                {
                    return cc;
                }
            }
            return null;
        }

        /// <summary>
        /// �S���t�B�[�`���[���b�`��ɂ���R���g���[����ǉ�����
        /// ���O�Ō�������ƒx���B������������� AddControl(Control )���g�p���邱��
        /// </summary>
        /// <param name="name">�R���g���[���� Control.Name</param>
        /// <returns>�ǉ����ꂽ�R���g���[�� / null = �G���[</returns>
        public Control GetControl(string name)
        {
            Control root;
            for (root = Pane.Control; root.Parent != null; root = root.Parent)
            {
                ;
            }

            return _getProc(name, root);
        }

        /// <summary>
        /// �e�t�H�[�����擾����
        /// </summary>
        /// <returns></returns>
        public Form GetParentForm()
        {
            Control root;
            for (root = Pane.Control; root.Parent != null; root = root.Parent)
            {
                ;
            }

            return root.FindForm();
        }

        private ToolStripItem _findTargetDropDownItem(string name, ToolStripDropDownItem value)
        {
            if (value.Name == name)
            {
                return value;
            }
            foreach (ToolStripItem tsi in value.DropDownItems)
            {
                if (tsi.Name == name)
                {
                    return tsi;
                }
                if (tsi is ToolStripDropDownItem)
                {
                    var res = _findTargetDropDownItem(name, (ToolStripDropDownItem)tsi);
                    if (res != null)
                    {
                        return res;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// �R���g���[������������
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private ToolStripItem _findTargetToolStripItemProc(string name, Control value)
        {
            var ctrls = new List<Control>();
            foreach (Control c in value.Controls)
            {
                ctrls.Add(c);
            }
            if (value.ContextMenuStrip != null)
            {
                ctrls.Add(value.ContextMenuStrip);
            }
            foreach (var c in ctrls)
            {
                if (c is ToolStrip ts)
                {
                    foreach (ToolStripItem tsi in ts.Items)
                    {
                        if (tsi.Name == name)
                        {
                            return tsi;
                        }
                        if (tsi is ToolStripDropDownItem)
                        {
                            var tsi2 = _findTargetDropDownItem(name, (ToolStripDropDownItem)tsi);
                            if (tsi2 != null)
                            {
                                return tsi2;
                            }
                        }
                    }
                }
                var cc = _findTargetToolStripItemProc(name, c);
                if (cc != null)
                {
                    return cc;
                }
            }
            return null;
        }

        /// <summary>
        /// �S���t�B�[�`���[���b�`��ɂ���
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ToolStripItem GetToolStripItem(string name)
        {
            Control root;
            for (root = Pane.Control; root.Parent != null; root = root.Parent)
            {
                ;
            }

            var ret = _findTargetToolStripItemProc(name, root);
            return ret;
        }

        private static readonly FieldInfo _fi_parentGroup = typeof(FeatureBase).GetField("_parentGroup", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _mi_setAutoRemoveFeature = typeof(FeatureGroupBase).GetMethod("_setAutoRemoveFeature", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// �C�x���g�]��
        /// </summary>
        private void _event(object sender, EventArgs e, string eventName)
        {
            if (Enabled == false)
            {
                return;
            }
            IList<EventUnit> vals = null;
            foreach (var einame in _addEvents.Keys)
            {
                if (einame == eventName)
                {
                    vals = _addEvents[einame];
                    break;
                }
            }
            if (vals == null)
            {
                System.Diagnostics.Debug.WriteLine("���o�^�C�x���g = " + eventName);
                return;
            }
            foreach (var cp in vals)
            {
                var control = cp.control;
                if (control == sender)
                {
                    var t = GetType();
                    var eventTid = NamedId.FromName("event@@" + t.AssemblyQualifiedName + "@@" + ID.ToString());
                    GetRoot().SetUrgentToken(startToken, eventTid, new object[] { sender, e, eventName, ID });

                    try
                    {
                        if (cp.function is ColumnClickEventHandler)
                        {
                            ((ColumnClickEventHandler)cp.function)(sender, (ColumnClickEventArgs)e);
                        }
                        else
                        {
                            ((EventHandler)cp.function)(sender, e);
                        }
                    }
                    catch (Exception ex)
                    {
                        var gr = (FeatureGroupBase)_fi_parentGroup.GetValue(this);
                        _mi_setAutoRemoveFeature.Invoke(gr, new object[] { this, ex, "_event" });

                    }
                    break;
                }
            }
            // �Ō�Ƀg�[�N���E�t�@�C�i���C�U���[�v�����s����
            GetRoot().FlushFeatureTriggers();
        }

        private void _SelectedIndexChanged(object sender, System.EventArgs e)
        {
            _event(sender, e, "SelectedIndexChanged");
        }

        private void _ButtonClick(object sender, System.EventArgs e)
        {
            _event(sender, e, "ButtonClick");
        }

        private void _Click(object sender, System.EventArgs e)
        {
            _event(sender, e, "Click");
        }

        private void _DoubleClick(object sender, System.EventArgs e)
        {
            _event(sender, e, "DoubleClick");
        }
        private void _ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _event(sender, e, "ColumnClick");
        }

        /// <summary>
        /// �C�x���g�n���h���^
        /// </summary>
        public class Handle
        {
            private readonly EventInfo _eventInfo;
            private readonly object _function;
            private readonly Delegate _eventHandler;
            private readonly Component _control;

            internal Handle(EventInfo eventInfo, object function, Delegate eventHandler, Component control)
            {
                _eventInfo = eventInfo;
                _function = function;
                _eventHandler = eventHandler;
                _control = control;
            }

            internal EventInfo EventInfo => _eventInfo;
            internal object Function => _function;
            internal Delegate EventHandler => _eventHandler;
            internal Component Control => _control;
        }

        /// <summary>
        /// �C�x���g��ǉ�����
        /// </summary>
        /// <param name="control">�C�x���g������</param>
        /// <param name="eventName">�C�x���g��</param>
        /// <param name="function">�C�x���g�]����</param>
        public Handle AddTrigger(Component control, string eventName, object function)
        {
            Delegate eh = null;
            if (_addEvents == null)
            {
                _addEvents = new Dictionary<string, IList<EventUnit>>();
            }
            var eis = control.GetType().GetEvents();
            foreach (var ei in eis)
            {
                if (ei.Name == eventName)
                {
                    switch (eventName)
                    {
                        case "SelectedIndexChanged": ei.AddEventHandler(control, eh = new EventHandler(_SelectedIndexChanged)); break;
                        case "Click": ei.AddEventHandler(control, eh = new EventHandler(_Click)); break;
                        case "ButtonClick": ei.AddEventHandler(control, eh = new EventHandler(_ButtonClick)); break;
                        case "DoubleClick": ei.AddEventHandler(control, eh = new EventHandler(_DoubleClick)); break;
                        case "ColumnClick": ei.AddEventHandler(control, eh = new ColumnClickEventHandler(_ColumnClick)); break;
                        default:
                            System.Diagnostics.Debug.Assert(false, "�������̃C�x���g�� " + GetType().Name + " �Ńv���O���~���O����Ă��܂��B= " + eventName);
                            break;
                    }
                    //ei.AddEventHandler(control, function);

                    // �o�^�����C�x���g���o���Ă���
                    if (_addEvents.TryGetValue(ei.Name, out var li) == false)
                    {
                        _addEvents.Add(ei.Name, li = new List<EventUnit>());
                    }
                    li.Add(new EventUnit(ei, control, function));
                    return new Handle(ei, function, eh, control);
                }
            }
            System.Diagnostics.Debug.Assert(false, control.Site.Name + "�R���g���[��(" + control.GetType().ToString() + ")�ɂ�'" + eventName + "'�Ƃ����C�x���g�����݂��܂���");
            return null;
        }

        /// <summary>
        /// �C�x���g��ǉ�����
        /// </summary>
        public void AddTrigger(Handle h)
        {
            h.EventInfo.AddEventHandler(h.Control, h.EventHandler);
            // �o�^�����C�x���g���o���Ă���
            var li = _addEvents[h.EventInfo.Name];
            if (li == null)
            {
                _addEvents.Add(h.EventInfo.Name, li = new List<EventUnit>());
            }
            li.Add(new EventUnit(h.EventInfo, h.Control, h.Function));
        }

        /// <summary>
        /// �C�x���g���폜����
        /// </summary>
        /// <param name="h">AddTrigger�Ŏ擾�����n���h��</param>
        public void RemoveTrigger(Handle h)
        {
            var li = (IList)_addEvents[h.EventInfo.Name];
            IList dels = new ArrayList();
            if (li != null)
            {
                foreach (EventUnit co in li)
                {
                    if (co.function == h.Function)
                    {
                        dels.Add(co);
                    }
                }
                foreach (EventUnit co in dels)
                {
                    h.EventInfo.RemoveEventHandler(co.control, h.EventHandler);
                    li.Remove(co);
                }
            }
        }

        /// <summary>
        /// �����񂩂�R���g���[�����������āA����ɃC�x���g��ǉ�����i�S�R���g���[�����X�L��������̂Œᑬ�j
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="eventName"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        public void AddTrigger(string controlName, string eventName, EventHandler function)
        {
            var c = GetControl(controlName);
            if (c != null)
            {
                AddTrigger(c, eventName, function);
            }
            else
            {
                var ts = GetToolStripItem(controlName);
                AddTrigger(ts, eventName, function);
            }
        }

        #region IDisposable �����o

        public override void Dispose()
        {
            if (_addEvents != null)
            {
                // �C�x���g�������ɍ폜����
                foreach (var de in _addEvents)
                {
                    foreach (var os in de.Value)
                    {
                        os.ei.RemoveEventHandler(os.control, (Delegate)os.function);
                    }
                }
                _addEvents.Clear();
            }
            base.Dispose();
        }

        #endregion
    }
}
