// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Feature Group Base �̊T�v�̐����ł��B
    /// fg : Feature Group
    /// �����̃t�B�[�`���[���ЂƂ̃p�b�P�[�W�Ƃ��ĊǗ����邽�߂̊�{�N���X
    /// </summary>
    public abstract class FeatureGroupBase : DataLinkManager, IMouseListener, IKeyListener, IDisposable
    {
        #region	����(�V���A���C�Y����)
        /// <summary>�q�t�B�[�`���[�O���[�v���Ǘ�����z��</summary>
        private readonly IList /*<fgBase �܂��� FeatureBase>*/ _children = new ArrayList();
        #endregion
        #region ����(�V���A���C�Y���Ȃ�)
        /// <summary>�}�E�X�̃N���b�N�ʒu��ۑ����鋤�L�ϐ�</summary>
        private MouseState _clickPos = null;
        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        protected FeatureGroupBase()
        {
        }
        #region IDisposable �����o

        public virtual void Dispose()
        {
            // ���ӁF������ffDataLinkBase��Dispose�͎��{���Ȃ��B����΁ATimerObject�Ȃǂ��g�p�ł��Ȃ��Ȃ�
            //        ����Dispose�̓��[�g�O���[�v������s����܂�

            // �q����Dispose���R�[������
            foreach (var lb in _children)
            {
                if (lb is IDisposable)
                {
                    ((IDisposable)lb).Dispose();
                }
            }
            _children.Clear();
        }

        #endregion

        /// <summary>
        /// �����ȉ��̃t�B�[�`���[��S�Ď擾����i�q�O���[�v���܂ށj
        /// </summary>
        /// <returns></returns>
        public ICollection GetChildFeatureInstance()
        {
            var ret = new ArrayList();
            _getChildFeatureInstances(ret, this);
            return ret;
        }

        /// <summary>
        /// GetChildFeatureInstance�Ŏg�p����ċA�֐�
        /// </summary>
        /// <param name="buf">���ʂ����߂�z��</param>
        /// <param name="fg">�����J�n����t�B�[�`���[�O���[�v</param>
        private void _getChildFeatureInstances(IList buf, FeatureGroupBase fg)
        {
            //lock(fg._children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var cg in fg._children)
                {
                    if (cg is FeatureGroupBase)
                    {
                        _getChildFeatureInstances(buf, (FeatureGroupBase)cg);
                    }
                    if (cg is FeatureBase)
                    {
                        buf.Add(cg);
                    }
                }
            }
        }

        /// <summary>
        /// �w��t�B�[�`���[�Ƀg�[�N�����Z�b�g����
        /// </summary>
        /// <param name="feature">�t�B�[�`���[</param>
        /// <param name="tokenID">�g�[�N��ID</param>
        protected void requestStartup(FeatureGroupBase group, Type feature, NamedId tokenID)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var cg in _children)
                {
                    if (cg is FeatureGroupBase)
                    {
                        requestStartup((FeatureGroupBase)cg, feature, tokenID);
                    }
                    if (cg is FeatureBase)
                    {
                        if (((FeatureBase)cg).Enabled == false)
                        {
                            continue;
                        }

                        if (cg.GetType().Equals(feature))
                        {
                            ((FeatureBase)cg).RequestStartup(tokenID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �q�t�B�[�`���[�O���[�v��ǉ�����
        /// </summary>
        public FeatureGroupBase AddChildGroup()
        {
            FeatureGroupBase ret = new FeatureGroupChild();
            linkDataTo(ret);
            //lock(_children.SyncRoot)	// �g�[�N�����s����AddChild�������̂ŁA�f�b�h���b�N���Ȃ��悤��Lock���Ȃ��B
            {
                _children.Add(ret);
            }
            return ret;
        }

        /// <summary>
        /// �q�t�B�[�`���[��ǉ�����i���O������j
        /// </summary>
        /// <param name="child"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public FeatureBase AddChildFeature(Type child, string name)
        {
            var ret = AddChildFeature(child);
            ret.Name = name;
            return ret;
        }

        /// <summary>
        /// �q�t�B�[�`���[�N���X��ǉ�����
        /// </summary>
        /// <param name="child">�t�B�[�`���[�N���X�̌^</param>
        public FeatureBase AddChildFeature(Type child)
        {
            System.Diagnostics.Debug.Assert(child.IsSubclassOf(typeof(FeatureBase)), "AddChildFeature�ɓo�^�ł���̂́AFeatureBase���p�������N���X�݂̂ł�");
            try
            {
                var ret = (FeatureBase)Activator.CreateInstance(child, true);
                linkDataTo(ret);
                //lock(_children.SyncRoot)	// �g�[�N�����s����AddChild�������̂ŁA�f�b�h���b�N�h�~��lock���Ȃ��B
                {
                    _children.Add(ret);
                }
                try
                {
                    ret.OnInitInstance();
                    var dummy = ret.CanStart;
                }
                catch (Exception ee)
                {
                    if (ret is IAutoRemovable)
                    {
                        Remove(ret);
                        ret = null;
                        System.Diagnostics.Debug.WriteLine("Feature " + child.ToString() + " is Auto Removed because " + ee.Message + " (" + ee.GetType().Name + ")");
                    }
                    else
                    {
                        if (ee.InnerException == null)
                        {
                            throw ee;
                        }
                        else
                        {
                            throw ee.InnerException;
                        }
                    }
                }
                return ret;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// �w�肵���t�B�[�`���[�C���X�^���X����������
        /// </summary>
        /// <param name="value">��������t�B�[�`���[</param>
        public void Remove(DataLinkManager value)
        {
            //lock(_children.SyncRoot)	// �g�[�N�����s����Remove�������̂ŁA�f�b�h���b�N�h�~��lock���Ȃ�
            {
                _children.Remove(value);
            }
            if (value is FeatureBase)
            {
                ((FeatureBase)value).Enabled = false;
            }
            if (value is IDisposable)
            {
                ((IDisposable)value).Dispose();
            }
        }

        /// <summary>
        /// �S�t�B�[�`���[���폜����
        /// </summary>
        public void RemoveAllFeatures()
        {
            foreach (var obj in _children)
            {
                if (obj is IDisposable)
                {
                    ((IDisposable)obj).Dispose();
                }
            }
            _children.Clear();
        }

        private static readonly FieldInfo _fiTokenInvokedChecker = typeof(TokenTray).GetField("TokenInvokedChecker", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// �g�[�N���������Ȃ�܂Ń��[�v���āA�e�t�B�[�`���[��Start�����s����
        /// </summary>
        /// <returns>true=�������ꂽ / false=�����͖�������</returns>
        private bool invokeStartupToken()
        {
            var root = (FeatureGroupRoot)GetRoot();
            var ret = false;


            var dupChecker = (IDictionary)_fiTokenInvokedChecker.GetValue(Token);

            dupChecker.Clear();

            var retryMax = 1000;
            while (Token.Count > 0)
            {
                System.Diagnostics.Debug.Assert(retryMax-- >= 0, "InvokeStartupToken�Ŗ������[�v�����m���܂���");
                var cnt = 0;
                root.startupLoop(dupChecker, ref cnt);
                if (cnt == 0)
                {
                    break;
                }
                else
                {
                    ret = true;
                }
            }
            Token._clear();
            dupChecker.Clear();

            // �A�v���I���v��������΃N���[�Y����
            if (root.IsApplicationQuitting)
            {
                // �t�H�[�����擾����
                System.Windows.Forms.Control c;
                for (c = root.GetFeatureRich(); c is System.Windows.Forms.Form == false; c = c.Parent)
                {
                    ;
                } ((System.Windows.Forms.Form)c).Close();
                root.IsApplicationQuitting = false;
            }
            return ret;
        }

        private static readonly MethodInfo _mi_control_bredge_event = typeof(FeatureControlBridgeBase).GetMethod("_event", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// �w��R���g���[���ɃC�x���g�𔭍s����
        /// </summary>
        /// <param name="featureID"></param>
        /// <param name="target"></param>
        /// <param name="eventName"></param>
        public void FireEvent(Id featureID, System.Windows.Forms.Control target, string eventName)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).FireEvent(featureID, target, eventName);
                    }
                    else if (c is FeatureControlBridgeBase)
                    {
                        if (((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (((FeatureBase)c).ID == featureID)
                        {
                            _mi_control_bredge_event.Invoke(c, new object[] { target, null, eventName });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �ً}�g�[�N�������ċA���[�v
        /// </summary>
        private void invokeUrgentTokenLoop(NamedId id, NamedId who, object param, FeatureGroupBase tar)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).invokeUrgentTokenLoop(id, who, param, tar);
                    }
                    else if (c is FeatureBase)
                    {
                        if (((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (TokenTray.ContainsTokenID((FeatureBase)c, id))
                        {
                            if (((FeatureBase)c).CanStart)
                            {
                                ((FeatureBase)c).Start(who);
                                ((FeatureBase)c).Start(who, param);
                            }
                        }
                    }
                }
            }
        }


        private void paramloop(FeatureGroupBase tar, List<FeatureBase> fcs)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).paramloop(tar, fcs);
                    }
                    else if (c is FeatureBase fc)
                    {
                        if (string.IsNullOrEmpty(fc.CommandParameter) == false)
                        {
                            fcs.Add(fc);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �R�}���h���C������͂��Ď��s����
        /// </summary>
        /// <param name="v"></param>
        public void ParseCommandLineParameter(string[] args)
        {
            var fcs = new List<FeatureBase>();
            paramloop(this, fcs);
            fcs.Sort((x, y) => x.CommandParameter?.Length ?? 0 - y.CommandParameter?.Length ?? 0);  // StartWith�œ��Ă�̂ŁA���������珈������

            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                for (var ii = 0; ii < fcs.Count; ii++)
                {
                    if (arg.StartsWith(fcs[ii].CommandParameter))
                    {
                        fcs[ii].OnCommandParameter(arg.Substring(fcs[ii].CommandParameter.Length).TrimStart());
                    }
                }
            }
        }

        /// <summary>
        /// �t�@�C�i���C�U�[��g�[�N�������ׂď����
        /// </summary>
        public void FlushFeatureTriggers()
        {
            for (var cnt = 0; ; cnt++)
            {
                var finalizer = Finalizers.Flush();
                var token = invokeStartupToken();
                if ((finalizer || token) == false)
                {
                    break;
                }
                System.Diagnostics.Debug.Assert(cnt < 100, "�t�@�C�i���C�U�ƃg�[�N���ŏz�Q�Ƃ��������Ă��邩������܂���");
            }
            // �p�[�c�폜�C�x���g
            checkAndFireDataChanged();

            // �S�g�[�N���I���C�x���g
            checkAndFireAllTokenCompleted();
        }

        /// <summary>
        /// ���O�Ŏq�t�B�[�`���[����������i������������j
        /// </summary>
        /// <param name="name">�������閼�O�i�啶���������̋�ʗL��j</param>
        /// <returns>���������t�B�[�`���[�ꗗ</returns>
        public IList<FeatureBase> FindChildFeatures(string name)
        {
            var ret = new List<FeatureBase>();
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        var tmp = ((FeatureGroupBase)c).FindChildFeatures(name);
                        ret.AddRange(tmp);
                    }
                    if (c is FeatureBase)
                    {
                        if (((FeatureBase)c).Name == name)
                        {
                            ret.Add((FeatureBase)c);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// �w��^�C�v���p�����Ă���t�B�[�`���[����������
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public IList<FeatureBase> FindChildFeatures(Type interfaceType)
        {
            var ret = new List<FeatureBase>();
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        var tmp = ((FeatureGroupBase)c).FindChildFeatures(interfaceType);
                        ret.AddRange(tmp);
                    }
                    if (c is FeatureBase)
                    {
                        if (interfaceType.IsInstanceOfType(c))
                        {
                            ret.Add((FeatureBase)c);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// �S�g�[�N���I���C�x���g����
        /// </summary>
        private void checkAndFireAllTokenCompleted()
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).checkAndFireAllTokenCompleted();
                    }
                    if (c is IAllTokenCompletedListener)
                    {
                        ((IAllTokenCompletedListener)c).OnAllTokenCompleted();
                    }
                }
            }
        }

        private static readonly FieldInfo _fiRemovedPartsOfData = typeof(PartsCollectionBase).GetField("_removedParts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly FieldInfo _fiAddedPartsOfData = typeof(PartsCollectionBase).GetField("_addedParts", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// �p�[�c�폜�����o������A�C�x���g���΂�
        /// </summary>
        private void checkAndFireDataChanged()
        {
            if (Parts == null)
            {
                return;
            }

            // �폜�f�[�^���擾����
            var removed = (IList)_fiRemovedPartsOfData.GetValue(Parts);
            var added = (IList)_fiAddedPartsOfData.GetValue(Parts);
            if (removed.Count > 0 || added.Count > 0)
            {
                _dataChangedEventLoop(removed, added);
                removed.Clear();
                added.Clear();
            }
        }

        /// <summary>
        /// �폜�C�x���g�̃C���^�[�t�F�[�X���������Ă���t�B�[�`���[�ɃC�x���g���΂�
        /// </summary>
        /// <param name="removed"></param>
        private void _dataChangedEventLoop(ICollection removed, ICollection added)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c)._dataChangedEventLoop(removed, added);
                    }
                    if (c is IPartsRemoveListener)
                    {
                        ((IPartsRemoveListener)c).OnPartsRemoved(removed);
                    }
                    if (c is IPartsAddListener)
                    {
                        ((IPartsAddListener)c).OnPartsAdded(added);
                    }
                }
            }
        }

        /// <summary>
        /// �ً}�g�[�N���𔭍s����i���ׂẴg�[�N������Ɏ��s����j
        /// </summary>
        /// <param name="id">TokenListener�ɓo�^����Ă���ID</param>
        /// <param name="who">FeatureBase.Start(who)�ɓ`����ID</param>
        /// <param name="param">�p�����[�^�t��Start�ɓn���p�����[�^</param>
        public void SetUrgentToken(NamedId id, NamedId who, object param)
        {
            invokeUrgentTokenLoop(id, who, param, GetRoot());
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �C�x���g�N��
        /// </summary>
        private void startupLoop(IDictionary dupChecker, ref int cnt)
        {
            //lock(_children.SyncRoot)	// �r����AddChild����̂ŁA�f�b�h���b�N�h�~�̂��߃X���b�h�Z�[�t
            {
                foreach (var c in new ArrayList(_children))  // �r����AddChild����̂ŁA_children���ύX����鎖�ɑΉ�
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).startupLoop(dupChecker, ref cnt);
                    }
                    else if (c is FeatureBase)
                    {
                        if (((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (dupChecker.Contains(c) == false)
                        {
#if DEBUG
                            TimeKeeper.SetStart(TimeKeeper.RecordType.Start, ((FeatureBase)c).ID);
#endif
                            ((FeatureBase)c).invokeToken();
                            cnt++;
                            dupChecker[c] = this;
#if DEBUG
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.Start, ((FeatureBase)c).ID);
#endif
                        }
                    }
                }
            }
        }

        /// <summary>
        /// �Y�[���ύX�C�x���g�]��
        /// </summary>
        /// <param name="target"></param>
        public virtual void ZoomChanged(IRichPane target)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IZoomListener)
                    {
                        foreach (var t in ((IZoomListener)c).ZoomEventTargets)
                        {
                            if (t == target)
                            {
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetStart(TimeKeeper.RecordType.ZoomChanged, ((FeatureBase)c).ID);
                                }
#endif
#if DEBUG == false
                                try
#endif
                                {
                                    ((IZoomListener)c).ZoomChanged(target);
                                }
#if DEBUG == false
                                catch (Exception ex)
                                {
                                    LOG.WriteLineException(ex);
                                }
#endif
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetEnd(TimeKeeper.RecordType.ZoomChanged, ((FeatureBase)c).ID);
                                }
#endif
                            }
                        }
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �X�N���[���ύX�C�x���g�]��
        /// </summary>
        /// <param name="target"></param>
        public virtual void ScrollChanged(IRichPane target)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IScrollListener)
                    {
                        foreach (var t in ((IScrollListener)c).ScrollEventTargets)
                        {
                            if (target == null || t == target)
                            {
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetStart(TimeKeeper.RecordType.ScrollChanged, ((FeatureBase)c).ID);
                                }
#endif
#if DEBUG == false
                                try
#endif
                                {
                                    ((IScrollListener)c).ScrollChanged(target);
                                }
#if DEBUG == false
                                catch (Exception ex)
                                {
                                    LOG.WriteLineException(ex);
                                }
#endif
#if DEBUG
                                if (c is FeatureBase)
                                {
                                    TimeKeeper.SetEnd(TimeKeeper.RecordType.ScrollChanged, ((FeatureBase)c).ID);
                                }
#endif
                            }
                        }
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }


        /// <summary>
        /// �}�E�X�ړ��C�x���g�]��
        /// </summary>
        public virtual void OnMouseMove(Tono.GuiWinForm.MouseState e)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseMove, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseMove(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseMove, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }


        /// <summary>
        /// �e�L�X�g�R�}���h���e�t�B�[�`���[�ɓ]������
        /// </summary>
        /// <param name="tc"></param>
        public void PlayTextCommand(CommandBase tc)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureGroupBase)
                    {
                        ((FeatureGroupBase)c).PlayTextCommand(tc);
                    }
                    else
                    {
                        if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                        {
                            continue;
                        }

                        if (c is ITextCommand)
                        {
                            var targets = ((ITextCommand)c).tcTarget();
                            for (var i = 0; i < targets.Length; i++)
                            {
                                if (tc.Command == targets[i])
                                {
                                    ((ITextCommand)c).tcPlay(tc);
                                }
                            }
                        }
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �}�E�X�_�E���C�x���g�]��
        /// </summary>
        public virtual void OnMouseDown(Tono.GuiWinForm.MouseState e)
        {
            if (Parts == null)
            {
                return;
            }

            if (_clickPos == null)
            {
                _clickPos = (MouseState)Share.Get("ClickPosition", typeof(MouseState));   // �ړ����̃p�[�c�ꗗ
            }
            SerializerEx.CopyObject(_clickPos, e);

            // �p�[�c���擾����
            ClickParts = Parts.GetPartsAt(e.Pos, true, out var clickPane);
            ClickPane = clickPane;

            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseDown, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseDown(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseDown, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �}�E�X�A�b�v�C�x���g�]��
        /// </summary>
        public virtual void OnMouseUp(Tono.GuiWinForm.MouseState e)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseUp, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseUp(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseUp, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �}�E�X�z�C�[���C�x���g�̓]��
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnMouseWheel(MouseState e)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IMouseListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.MouseWheel, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IMouseListener)c).OnMouseWheel(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.MouseWheel, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �A�C�e���̃h���b�v�C�x���g�̓]��
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnDragDrop(Tono.GuiWinForm.DragState e)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IDragDropListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.DragDrop, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IDragDropListener)c).OnDragDrop(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.DragDrop, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �L�[�_�E���C�x���g�]��
        /// </summary>
        public virtual void OnKeyDown(KeyState e)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IKeyListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.KeyDown, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IKeyListener)c).OnKeyDown(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.KeyDown, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }

        /// <summary>
        /// �L�[�A�b�v�C�x���g�]��
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnKeyUp(KeyState e)
        {
            //lock(_children.SyncRoot)	// �r����AddChild���Ȃ��̂ŁA�X���b�h�Z�[�t�B�r����AddChild���Ă��܂��Ɖ��̃��[�v��Assert�����
            {
                foreach (var c in _children)
                {
                    if (c is FeatureBase && ((FeatureBase)c).Enabled == false)
                    {
                        continue;
                    }

                    if (c is IKeyListener)
                    {
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetStart(TimeKeeper.RecordType.KeyUp, ((FeatureBase)c).ID);
                        }
#endif
#if DEBUG == false
                        try
#endif
                        {
                            ((IKeyListener)c).OnKeyUp(e);
                        }
#if DEBUG == false
                        catch (Exception ex)
                        {
                            LOG.WriteLineException(ex);
                        }
#endif
#if DEBUG
                        if (c is FeatureBase)
                        {
                            TimeKeeper.SetEnd(TimeKeeper.RecordType.KeyUp, ((FeatureBase)c).ID);
                        }
#endif
                    }
                }
            }
            checkAndFireDataChanged();  // �p�[�c�폜�C�x���g
        }
    }
}
