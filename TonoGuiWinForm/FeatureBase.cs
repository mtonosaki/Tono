using System;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureBase �̊T�v�̐����ł��B
    /// �t�B�[�`���[�N���X�̊�{�N���X
    /// </summary>
    public abstract class FeatureBase : DataLinkManager
    {
        #region �t�@�C���Z���N�^�@�t�B�[�`���[��
        /// <summary>
        /// �t�@�C���Z���N�^�@�t�B�[�`���[��
        /// </summary>
        public class FileSelectorAdapter
        {
            private string _filename = "";
            private string _filter = "*.*";
            private bool _isSave = false;
            private readonly IList _hlList = new ArrayList();
            private readonly PersistManager.RecorderBridge IR;

            /// <summary>
            /// �R���X�g���N�^
            /// </summary>
            public FileSelectorAdapter(PersistManager.RecorderBridge irPersister)
            {
                IR = irPersister;
            }

            public bool IsSave
            {
                get => _isSave;
                set => _isSave = value;
            }


            /// <summary>
            /// ���s����
            /// </summary>
            /// <returns></returns>
            public DialogResult ShowDialog()
            {
                object ret;

                {
#if false  // fFileSelector�g�p�̏ꍇ
				// �C���X�^���X����
				const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
				ConstructorInfo ci = typeof(fFileSelector).GetConstructor(flags, null, new Type[]{}, null);				
				fFileSelector fs = (fFileSelector)ci.Invoke(new object[]{});
				foreach( string s in _hlList )
				{
					fs.AddHighlightExt(s);
				}
				fs.FileName = this._filename;
				fs.Filter = this._filter;

				// ShowDialog���s
				MethodInfo mi = typeof(fFileSelector).GetMethod("showDialog", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				object ret = mi.Invoke(fs, new object[]{});
#else      // OpenFileDialog�g�p�̏ꍇ
                    IR.Save(new DeviceRecord.TagSkipStart(), NamedId.FromName("InputRecorderSaveSerializeID"));

                    FileDialog fs;
                    if (_isSave)
                    {
                        fs = new SaveFileDialog
                        {
                            CheckFileExists = false
                        };
                    }
                    else
                    {
                        fs = new OpenFileDialog
                        {
                            CheckFileExists = true
                        };
                    }
                    fs.FileName = _filename;
                    //string f = "";
                    //string[] fis = _filter.Split(new char[] { '|' });
                    //for (int i = 0; i < fis.Length; i++)
                    //{
                    //    string ff = fis[i];
                    //    if (ff == "*.*")
                    //    {
                    //        f += "All files|*.*|";
                    //        continue;
                    //    }
                    //    if (ff.StartsWith("*."))
                    //    {
                    //        f += ff.Substring(2).ToUpper() + " files|" + ff + "|";
                    //    }
                    //}
                    //if (f.EndsWith("|"))
                    //{
                    //    f = f.Substring(0, f.Length - 1);
                    //}
                    //fs.Filter = f;
                    fs.Filter = _filter;
#endif
                    ret = fs.ShowDialog();

                    // ����L�^
                    IR.Save(new DeviceRecord.TagFileSelect((DialogResult)ret, fs.FileName), NamedId.FromName("InputRecorderSaveSerializeID"));

                    _filename = fs.FileName;
                    fs.Dispose();

                    IR.Save(new DeviceRecord.TagSkipEnd(), NamedId.FromName("InputRecorderSaveSerializeID"));
                }
                // �t�@�C�����ȊO�́A���ׂă��Z�b�g����
                _hlList.Clear();
                _filter = "*.*";

                return (DialogResult)ret;
            }

            /// <summary>
            /// �n�C���C�g�̊g���q���w�肷��
            /// </summary>
            /// <param name="ext">�g���q</param>
            public void AddHighlightExt(string ext)
            {
                _hlList.Add(ext);
            }

            /// <summary>
            /// �t�@�C����
            /// </summary>
            public string FileName
            {
                get => _filename;
                set => _filename = value;
            }

            /// <summary>
            /// �t�B���^ ��F  *.doc|*.txt|*.rtf
            /// </summary>
            public string Filter
            {
                set => _filter = value;
            }
        }

        #endregion

        internal class forMultiTokenDummy : FeatureBase
        {
        }

        /// <summary>
        /// �K����������g�[�N���𔭍s�ł���t�B�[�`���[�I�u�W�F�N�g
        /// </summary>
        /// <returns></returns>
        public static FeatureBase ForMultiTokenDummy()
        {
            return new forMultiTokenDummy();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
        }

        #region �����i�V���A���C�Y����j
        /// <summary>�C���X�^���X����肷�邽�߂�ID</summary>
        private Id _instanceID;
        /// <summary>�ŏI�\���J�[�\��</summary>
        public static System.Windows.Forms.Cursor Cursor = Cursors.Default;
        /// <summary>���C�x���g���̕\���J�[�\��</summary>
        protected System.Windows.Forms.Cursor _EventCursor = Cursors.Default;
        /// <summary>�p�����[�^������̕ۑ�</summary>
        private readonly string _paramString = "";

        /// <summary>�t�B�[�`���[�̖��O</summary>
        private string _featureName = null;

        /// <summary>�t�B�[�`���[���g�p���邩�ǂ����̃t���O</summary>
        private bool _isEnable = true;
        #endregion
        #region �����i�V���A���C�Y���Ȃ��j

        /// <summary>�g�[�N�����s��Record����^�C�~���O</summary>
        protected NamedId startToken;
        private static int _instanceCounter = 1;
        /// <summary>
        /// Enable�v���p�e�B�[�ύX�̒m�点
        /// </summary>
        protected EventHandler<EventArgs> EnableChanged;

        /// <summary>�ꎞ�I�ȃg�[�N��ID�i���j���[��{�^������̋N���Ɏg�p�j</summary>
        private NamedId _temporaryTokenListenerID = null;
        /// <summary>
        /// REDO�p��ID
        /// </summary>
        public static readonly NamedId REDO = NamedId.FromName("PersisterForRedo");
        /// <summary>
        /// UNDO�p��ID
        /// </summary>
        public static readonly NamedId UNDO = NamedId.FromName("PersisterForUndo");

        private FileSelectorAdapter _fileSelector = null;

        #endregion

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        protected FeatureBase()
        {
            _instanceID = new Id { Value = _instanceCounter++ };
            startToken = NamedId.FromName("FeatureGotToken");
            _featureName = GetType().Name + "#" + _instanceID.Value.ToString();
        }

        /// <summary>
        /// �C���X�^���X��
        /// </summary>
        public string Name
        {
            get => _featureName;
            set => _featureName = value;
        }

        /// <summary>
        /// �����N��������R�}���h�p�����[�^
        /// </summary>
        public string CommandParameter { get; set; } = null;

        /// <summary>
        /// �R�}���h�p�����[�^���w�肳��Ă��鎞�A�N�����Ɏ����I�ɃR�[�������
        /// </summary>
        /// <param name="arg">�R�}���h���C��  /s=aaa  �Ȃ�Aaaa ���i�[�����</param>
        public virtual void OnCommandParameter(string arg)
        {
        }

        /// <summary>
        /// �R�}���h�p�����[�^��O
        /// </summary>
        public class CommandParameterException : Exception
        {
            public CommandParameterException() : base()
            {
            }
            public CommandParameterException(string mes) : base(mes)
            {
            }
        }

        /// <summary>
        /// �t�@�C���Z���N�^
        /// </summary>
        public FileSelectorAdapter FileSelector
        {
            get
            {
                if (_fileSelector == null)
                {
                    _fileSelector = new FileSelectorAdapter(Persister[NamedId.FromName("RecorderDeviceInput")]);
                }
                return _fileSelector;
            }
        }

        /// <summary>
        /// �p�����[�^�������Ԃ�
        /// </summary>
        public string GetParamString()
        {
            return _paramString;
        }

        /// <summary>
        /// �t�B�[�`���[�����񂩂�g�p���邩�ǂ����̃t���O
        /// </summary>
        public virtual bool Enabled
        {
            get => _isEnable;
            set
            {
                var pre = _isEnable;
                _isEnable = value;
                if (_isEnable != pre)
                {
                    EnableChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// �t�B�[�`���[�̃f�t�H���g�̕�������쐬����
        /// </summary>
        /// <returns>�C���X�^���X�̕�����</returns>
        public override string ToString()
        {
            return GetType().Name + "[" + _instanceID.ToString() + "]";
        }


        /// <summary>
        /// �p�����[�^���������͂���
        /// </summary>
        /// <param name="param"></param>
        public virtual void ParseParameter(string param)
        {
        }

        /// <summary>
        /// �t�B�[�`���[���J�n�ł����Ԃ��ǂ������O���ɒʒm�����i
        /// </summary>
        public virtual bool CanStart => Enabled;

        /// <summary>
        /// ���j���[�̃`�F�b�N�ɘA��������
        /// </summary>
        public virtual bool Checked => false;

        /// <summary>
        /// ���b�Z�[�W���擾����
        /// </summary>
        protected Mes Mes
        {
            
            get => Mes.Current;
        }

        /// <summary>
        /// ���t�B�[�`���[�ɃY�[���ύX�C�x���g�𑗏o����
        /// </summary>
        protected void SendZoomChanged()
        {
            if (this is IZoomListener)
            {
                foreach (var rp in ((IZoomListener)this).ZoomEventTargets)
                {
                    ((IZoomListener)this).ZoomChanged(rp);
                }
            }
        }

        /// <summary>
        /// ���t�B�[�`���[�ɃX�N���[���ύX�C�x���g�𑗏o����
        /// </summary>
        protected void SendScrollChanged()
        {
            if (this is IScrollListener)
            {
                foreach (var rp in ((IScrollListener)this).ScrollEventTargets)
                {
                    ((IScrollListener)this).ScrollChanged(rp);
                }
            }
        }

        /// <summary>
        /// ���ȌĂяo���̗\��
        /// �i�d�g�݁F���g�[�N���������I�ɔ��s����ē��삷��j
        /// </summary>
        public void RequestStartup()
        {
            RequestStartup(Id.Nothing);
        }

        /// <summary>
        /// ���ȌĂяo���̗\��
        /// �i�d�g�݁F�ꎞ�g�[�N���������I�ɔ��s����ē��삷��j
        /// </summary>
        /// <param name="id">ID�𖾎����邱�Ƃ��ł���</param>
        public void RequestStartup(Id id)
        {
            if (id.IsNothing())
            {
                var t = GetType();
                _temporaryTokenListenerID = NamedId.FromName("request@@" + t.AssemblyQualifiedName + "@@" + ID.ToString());
            }
            else
            {
                _temporaryTokenListenerID = NamedId.FromIDNoName(new Id { Value = id.Value });
            }
            Token.Add(_temporaryTokenListenerID, this);
            GetRoot().FlushFeatureTriggers();
            //GetRoot().InvokeStartupToken(); // TONO
        }

        /// <summary>
        /// �������Ẵg�[�N�����s���āA�������̃g�[�N������������
        /// </summary>
        /// <returns>
        /// ���s�����g�[�N��ID
        /// </returns>
        internal NamedId invokeToken()
        {
            var isNeedUrgentToken = false;


            // ���s�m�F
            NamedId need = null;


            if (this is ITokenListener)
            {
                var id = ((ITokenListener)this).TokenTriggerID;
                if (Token.Contains(id))
                {
                    need = id;
                }
            }
            if (need == null && this is IMultiTokenListener)
            {
                foreach (var id in ((IMultiTokenListener)this).MultiTokenTriggerID)
                {
                    if (Token.Contains(id))
                    {
                        need = id;
                        break;
                    }
                }
            }

            if (_temporaryTokenListenerID != null) // �Վ��g�[�N���̊m�F�i�����g�[�N���Ɨ����������ꍇ�A�����炪�g�p�����j
            {
                if (Token.Contains(_temporaryTokenListenerID))
                {
                    need = _temporaryTokenListenerID;
                    isNeedUrgentToken = true;
                }
            }
            if (need != null)
            {
                if (CanStart)
                {
                    var sw = new StopWatch();

                    // �g�[�N�����s��ʒm����ً}�Վ��g�[�N��
                    if (isNeedUrgentToken)
                    {
                        GetRoot().SetUrgentToken(startToken, need, this);
                    }

                    // �X�^�[�g
                    Start(need);

                    var sec = sw.Stop();
                    if (sec >= 1.0)
                    {
                        var s = GetType().Name + (this is FeatureThreadBase ? " (Thread)" : "");
                        System.Diagnostics.Debug.WriteLine(s + " " + sec.ToString("0.000") + "[sec]");
                    }
                }
            }
            return need;
        }

        /// <summary>
        /// �f�[�^���g�p�������������K�v�Ȃ炱���Ɏ�������
        /// </summary>
        public virtual void OnInitInstance()
        {
        }

        /// <summary>
        /// �C�x���g�N��
        /// </summary>
        public virtual void Start(NamedId who)
        {
        }

        /// <summary>
        /// �C�x���g�N��
        /// </summary>
        public virtual void Start(NamedId who, object arg)
        {
        }

        /// <summary>
        /// �J�[�\�����Z�b�g����i�t�@�C�i���C�Y�j
        /// </summary>
        protected void onCursorSeFinalizert()
        {
            if (Cursor != null)
            {
                //((IControlUI)Pane).Cursor = Cursor;
                var ts = new ThreadUtil();
                ts.SetCursorControl(Pane.Control, Cursor);
            }
        }

        #region IID �����o

        /// <summary>
        /// 
        /// </summary>
        public Id ID => _instanceID;

        #endregion
    }
}
