// (c) 2019 Manabu Tonosaki
// Licensed under the MIT license.

namespace Tono.GuiWinForm
{
    /// <summary>
    /// Data linkage Manager
    /// </summary>
    public abstract class DataLinkManager
    {
        #region �����i�V���A���C�Y����j---------------------------------------------

        /// <summary>�f�[�^�̎Q��</summary>
        private PartsCollectionBase _partsData = null;

        /// <summary>�A�v���P�[�V�����ŗL�̃f�[�^</summary>
        private DataHotBase _appData = null;

        /// <summary>���L�ϐ��Ǘ��I�u�W�F�N�g</summary>
        private DataSharingManager _share = null;

        /// <summary>�p�[�c�ƃA�v���f�[�^�����т���I�u�W�F�N�g</summary>
        private DataLinkBase _link = null;

        /// <summary>�e�t�B�[�`���[�O���[�v</summary>
        private DataLinkManager _parent = null;

        /// <summary>�t�@�C�i���C�U�i���ׂẴt�B�[�`�����s��ɓ��삷��\��I�u�W�F�N�g�j</summary>
        private FinalizeManageBuffer _finalizers = null;

        /// <summary>�i�����Ǘ�</summary>
        private PersistManager _persister = null;

        /// <summary>
        /// �g�[�N�������Ă������߂̔�
        /// </summary>
        private TokenTray _tokenTray = null;

        #endregion
        #region �����i�V���A���C�Y���Ȃ��j------------------------------------------

        /// <summary>�����N���郊�b�`�y�[��</summary>
        private TGuiView _pane = null;

        /// <summary>�^�C�}�[����</summary>
        private GuiTimer _timer = null;

        /// <summary>�Ō�ɃN���b�N�iMouseDown�j�������̃p�[�c</summary>
        private DataSharingManager.Object _clickParts = null;

        /// <summary>
        /// _clickParts�����������Ƃ��̃y�[���i�C�����[�W�����y�[���ł���΁ABinder�y�[���I�u�W�F�N�g�ɂȂ�j
        /// </summary>
        private DataSharingManager.Object _clickPane = null;

        #endregion

        /// <summary>
        /// ���������K�v�ł���Ώ��������鏈��
        /// </summary>
        private void init()
        {
            _clickParts = _clickParts == null ? new DataSharingManager.Object() : _clickParts;
            _clickPane = _clickPane == null ?  new DataSharingManager.Object() : _clickPane;
            _tokenTray = _tokenTray== null ?  new TokenTray() : _tokenTray;
            _finalizers = _finalizers == null ? new FinalizeManageBuffer() : _finalizers;
            _persister = _persister== null ? new PersistManager() : _persister;
            _timer = _timer == null ? new GuiTimer() : _timer;
        }

        /// <summary>
        /// ���[�g�O���[�v��Dispose����
        /// </summary>
        private void disposeByRootGroup()
        {
            _timer?.Dispose();
            _timer = null;
        }

        /// <summary>
        /// �֘A�t������y�[�����w�肷��
        /// </summary>
        /// <param name="pane">�y�[��</param>
        protected void setPane(TGuiView pane)
        {
            _pane = pane;
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected void setAppData(DataHotBase value)
        {
            System.Diagnostics.Debug.Assert(_appData == null, "setAppData�̓��[�g�O���[�v�t�B�[�`���[�Ɉ�x�������s�������ȃ��\�b�h�ł�");
            _appData = value;
            init();
        }

        /// <summary>
        /// �֘A�t�����郊���N�I�u�W�F�N�g���w�肷��
        /// </summary>
        /// <param name="value">�����N�I�u�W�F�N�g</param>
        protected void setLink(DataLinkBase value)
        {
            System.Diagnostics.Debug.Assert(_link == null, "setLink�̓��[�g�O���[�v�t�B�[�`���[�Ɉ�x�������s�������ȃ��\�b�h�ł�");
            _link = value;
            init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        protected void setPartsData(PartsCollectionBase value)
        {
            System.Diagnostics.Debug.Assert(_partsData == null, "setPartsData�̓��[�g�O���[�v�t�B�[�`���[�Ɉ�x�������s�������ȃ��\�b�h�ł�");
            _partsData = value;
            init();
        }

        /// <summary>
        /// ���L�ϐ��Ǘ��C���X�^���X���w�肷��
        /// </summary>
        /// <param name="value">���L�ϐ��Ǘ��C���X�^���X</param>
        protected void setShare(DataSharingManager value)
        {
            if (_share != null)
            {
                System.Diagnostics.Debug.Assert(_share.Count == 0, "����Share�𗘗p������ALinkShare�ő���ffShare�ƌ��т��͕̂s�\�ł�");
            }
            _share = value;
            init();
        }

        /// <summary>
        /// �^�C�}�[����I�u�W�F�N�g
        /// </summary>
        protected GuiTimer Timer
        {
            get
            {
                System.Diagnostics.Debug.Assert(_timer != null, "Dispose���ꂽ�t�B�[�`���[�ł��BTimer�͎g�p�ł��܂���");
                return _timer;
            }
        }

        /// <summary>
        /// �����N
        /// </summary>
        protected DataLinkBase Link => _link;

        /// <summary>
        /// �g�[�N��
        /// </summary>
        protected TokenTray Token => _tokenTray;

        /// <summary>
        /// �t�@�C�i���C�U�[�Ǘ��I�u�W�F�N�g
        /// </summary>
        protected FinalizeManageBuffer Finalizers => _finalizers;

        /// <summary>
        /// �i�����Ǘ��I�u�W�F�N�g
        /// </summary>
        protected PersistManager Persister => _persister;

        /// <summary>
        /// �A�v���P�[�V�����f�[�^
        /// </summary>
        protected DataHotBase Data => _appData;

        /// <summary>
        /// �p�[�c�f�[�^
        /// </summary>
        protected PartsCollectionBase Parts => _partsData;

        /// <summary>
        /// ���L�ϐ�
        /// </summary>
        protected DataSharingManager Share => _share;

        /// <summary>
        /// �}�U�[�y�[���i��{�ɂȂ�cFeatureRich)
        /// </summary>
        protected IRichPane Pane => _pane;

        /// <summary>
        /// �Ō�ɃN���b�N�������̃p�[�c
        /// </summary>
        protected PartsBase ClickParts
        {
            get => (PartsBase)_clickParts.value;
            set => _clickParts.value = value;
        }

        /// <summary>
        /// �Ō�ɃN���b�N�����Ƃ���̃y�[���i�C�����[�W�����Ȃ�Binder�y�[�����Ԃ�j
        /// </summary>
        protected IRichPane ClickPane
        {
            get => (IRichPane)_clickPane.value;
            set => _clickPane.value = value;
        }

        /// <summary>
        /// �C���X�^���X�𐶐�����B��̕��@�B���[�U�[�͎����ŃC���X�^���X�����֎~
        /// </summary>
        protected DataLinkManager()
        {
        }

        /// <summary>
        /// �t�B�[�`���[�Œʂ��Ďg�p����f�[�^���֘A����
        /// </summary>
        /// <param name="theInstanceWhoHasEmptyData">�܂��f�[�^���L���Ă��Ȃ�fgBase/FeatureBase</param>
        protected void linkDataTo(DataLinkManager theInstanceWhoHasEmptyData)
        {
            theInstanceWhoHasEmptyData._appData = _appData;
            theInstanceWhoHasEmptyData._partsData = _partsData;
            theInstanceWhoHasEmptyData._pane = _pane;
            theInstanceWhoHasEmptyData._share = _share;
            theInstanceWhoHasEmptyData._finalizers = _finalizers;
            theInstanceWhoHasEmptyData._persister = _persister;
            theInstanceWhoHasEmptyData._tokenTray = _tokenTray;
            theInstanceWhoHasEmptyData._link = _link;
            theInstanceWhoHasEmptyData._timer = _timer;
            theInstanceWhoHasEmptyData._parent = this;
            theInstanceWhoHasEmptyData._clickParts = _clickParts;
            theInstanceWhoHasEmptyData._clickPane = _clickPane;
        }

        /// <summary>
        /// ���[�g�t�B�[�`���[�O���[�v��Ԃ�
        /// </summary>
        /// <returns>���[�g�t�B�[�`���[�O���[�v</returns>
        public FeatureGroupBase GetRoot()
        {
            DataLinkManager ret;
            for (ret = this; ret._parent != null; ret = ret._parent)
            {
                ;
            }

            if (ret is FeatureGroupBase)
            {
                return (FeatureGroupBase)ret;
            }
            System.Diagnostics.Debug.Assert(ret != null, "���̃t�B�[�`���[�Z�b�g�́A���[�g��fgBase�h���N���X���g�p���Ă��܂���");
            return null;
        }
    }
}
