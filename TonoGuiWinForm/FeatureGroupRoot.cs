// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Reflection;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// fgRoot �̊T�v�̐����ł��B
    /// ���[�g�̃t�B�[�`���[�O���[�v
    /// ���̂��ׂẴt�B�[�`���[�O���[�v�́A���̎q�t�B�[�`���[�O���[�v�ƂȂ�
    /// </summary>
    public sealed class FeatureGroupRoot : FeatureGroupBase, IDisposable
    {
        #region �����i�V���A���C�Y����j

        #endregion
        #region �����i�V���A���C�Y���Ȃ��j
        private FeatureLoaderBase _loader;
        private readonly TGuiView _motherPane;
        private readonly DataSharingManager.Int _isApplicationQuitting;

        #endregion

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        /// <param name="motherPane">���̃t�B�[�`���[���[�g�Ŏg�p�����Ãy�[��</param>
        public FeatureGroupRoot(TGuiView motherPane)
        {
            _motherPane = motherPane;
            setPane(motherPane);
            setShare(new DataSharingManager());
            _isApplicationQuitting = (DataSharingManager.Int)Share.Get("ApplicationQuitFlag", typeof(DataSharingManager.Int));
        }

        /// <summary>
        /// �w��t�B�[�`���[�Ƀg�[�N�����Z�b�g����
        /// </summary>
        /// <param name="feature">�t�B�[�`���[</param>
        /// <param name="tokenID">�g�[�N��ID</param>
        public void RequestStartup(Type feature, NamedId tokenID)
        {
            requestStartup(this, feature, tokenID);
        }

        /// <summary>
        /// �A�v���P�[�V�����I���v���������Ă��邩�ǂ����𒲂ׂ�
        /// </summary>
        public bool IsApplicationQuitting
        {
            get => _isApplicationQuitting.value == 0 ? false : true;
            set => _isApplicationQuitting.value = value ? 1 : 0;
        }

        /// <summary>
        /// �}�U�[�y�[����Ԃ��i��{�ƂȂ�t�B�[�`���[���b�`�R���|�[�l���g�j
        /// </summary>
        /// <returns></returns>
        public TGuiView GetFeatureRich()
        {
            return _motherPane;
        }

        /// <summary>
        /// �t�H�[����OnLoad�Ŏ��s����R�}���h
        /// </summary>
        /// <param name="featureLoader">uFeatureLoader�V���[�Y�̃^�C�v</param>
        public void Initialize(Type featureLoader)
        {
            if (featureLoader != null)
            {
                _loader = (FeatureLoaderBase)Activator.CreateInstance(featureLoader);
                _loader.Load(this, "Portfolio0.xml");
            }
        }

        /// <summary>
        /// �t�@�C�������w�肵�ăt�H�[����OnLoad�����s����R�}���h
        /// </summary>
        /// <param name="featureLoader">uFeatureLoader�V���[�Y�̃^�C�v</param>
        public void Initialize(Type featureLoader, string file)
        {
            if (featureLoader != null)
            {
                _loader = (FeatureLoaderBase)Activator.CreateInstance(featureLoader);
                _loader.Load(this, file);
            }
        }

        #region IDisposable �����o

        public override void Dispose()
        {
            base.Dispose();
            var mi = typeof(DataLinkManager).GetMethod("disposeByRootGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            mi.Invoke(this, Array.Empty<object>());
        }

        #endregion

        /// <summary>
        /// �f�[�^�����蓖�Ă�
        /// </summary>
        /// <param name="value">���蓖�āi�Q�Ɓj�����f�[�^�̃C���X�^���X</param>
        public void AssignAppData(DataHotBase value)
        {
            setAppData(value);
        }

        /// <summary>
        /// �����N�I�u�W�F�N�g�����蓖�Ă�
        /// </summary>
        /// <param name="value">�����N</param>
        public void AssignLink(DataLinkBase value)
        {
            setLink(value);
        }


        /// <summary>
        /// �p�[�c�Z�b�g�����蓖�Ă�
        /// </summary>
        /// <param name="value"></param>
        public void AssignPartsSet(PartsCollectionBase value)
        {
            setPartsData(value);
        }

        /// <summary>
        /// Share��parent��������p��
        /// </summary>
        /// <param name="parent"></param>
        public void LinkShare(DataSharingManager parent)
        {
            setShare(parent);
        }

        /// <summary>
        /// �f�[�^���擾����
        /// </summary>
        /// <returns></returns>
        public PartsCollectionBase GetPartsSet()
        {
            System.Diagnostics.Debug.WriteLineIf(Parts == null, "PartsSet���g�p����O�� AssignData�����s���Ă����悤�Ƀv���O�������Ă�������");
            return Parts;
        }

        /// <summary>
        /// �f�[�^���擾����
        /// </summary>
        /// <returns></returns>
        public DataHotBase GetData()
        {
            System.Diagnostics.Debug.WriteLineIf(Data == null, "Data���g�p����O�� AssignData�����s���Ă����悤�Ƀv���O�������Ă�������");
            return Data;
        }

        /// <summary>
        /// �f�[�^���擾����
        /// </summary>
        /// <returns></returns>
        public DataSharingManager GetShare()
        {
            //Debug.WriteLineIf(Share == null, "Share���g�p����O�� AssignData�����s���Ă����悤�Ƀv���O�������Ă�������");
            return Share;
        }

        public override void OnMouseDown(MouseState e)
        {
            base.OnMouseDown(e);
            FlushFeatureTriggers();
        }

        public override void OnMouseMove(MouseState e)
        {
            base.OnMouseMove(e);
            FlushFeatureTriggers();
        }

        public override void OnMouseUp(MouseState e)
        {
            base.OnMouseUp(e);
            FlushFeatureTriggers();
        }
        public override void OnMouseWheel(MouseState e)
        {
            base.OnMouseWheel(e);
            FlushFeatureTriggers();
        }

        public override void OnDragDrop(DragState e)
        {
            base.OnDragDrop(e);
            FlushFeatureTriggers();
        }

        public override void OnKeyDown(KeyState e)
        {
            base.OnKeyDown(e);
            FlushFeatureTriggers();
        }

        public override void OnKeyUp(KeyState e)
        {
            base.OnKeyUp(e);
            FlushFeatureTriggers();
        }

        public override void ZoomChanged(IRichPane target)
        {
            base.ZoomChanged(target);
            //FlushFeatureTriggers();	// TODO:�����́A�R�����g�ł�OK�H by Tono 2006.1.23
        }
    }
}
