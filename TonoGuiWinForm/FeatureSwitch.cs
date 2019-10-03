using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureSwitch �̊T�v�̐����ł��B
    /// �p�����[�^�Ŏw�肵���t�B�[�`���[�̃X�C�b�`��؂�ւ��邱�Ƃ��ł��܂��B
    /// �w��ł���̂́A�t�B�[�`���[�N���X���A�t�B�[�`���[�C���X�^���X���ł��B
    /// ��P�F	FeatureWheelScroll,FeatureWheelZoom	���@���t�B�[�`���[�N���X���̃X�C�b�`���O
    /// ��Q�F	FeatureWheelScroll,@ZoomZoom		���@FeatureWheelScroll�N���X�̃C���X�^���X�S���� ZoomZoom�Ƃ����C���X�^���X���̃t�B�[�`���[�̃X�C�b�`���O
    /// �Q�l�j�t�B�[�`���[�̃C���X�^���X���́AXML �� name�����Ŏw��ł���AFeatureBase.Name�v���p�e�B�̂��Ƃł��B
    /// </summary>
    public class FeatureSwitch : FeatureControlBridgeBase, IMultiTokenListener, IAutoRemovable
    {
        #region UNDO/REDO�^�O TagFeatureSwitch
        public class TagFeatureSwitch
        {
            public Id switchingFeatureID;
            public bool sw;

            private TagFeatureSwitch()
            {
            }
            public TagFeatureSwitch(FeatureBase fc, bool featureSw)
            {
                switchingFeatureID = fc.ID;
                sw = featureSw;
            }
            public override string ToString()
            {
                return GetType().Name + " FeatureID=" + switchingFeatureID.ToString() + " Sw=" + sw.ToString();
            }

        }
        #endregion

        private bool _isOn = true;
        private bool _canStart = true;
        private bool _initialState = true;
        private System.Windows.Forms.CheckBox _checkBox = null;
        private string _interlockGroup = "";

        /// <summary>�ǂݍ��݊����������g�[�N��ID</summary>
        protected static readonly NamedId _tokenReadCompleted = NamedId.FromName("TokenReadCompleted");
        /// <summary>�I�𒆂̃p�[�c�Ƃ����Ӗ��ŃV���A���C�Y����ID</summary>
        protected static readonly NamedId _featureDataID = NamedId.FromName("FeatureDataSerializeID");
        /// <summary>�g�[�N���Ő؂�ւ���</summary>
        protected static readonly NamedId _tokenSwitch = NamedId.FromName("TokenChangeFeatureSwitches");

        private readonly NamedId[] _tokens = new NamedId[] { _tokenReadCompleted, _tokenSwitch };

        /// <summary>���L�ϐ��F�C���^�[���b�N���ƃX�C�b�`�I�u�W�F�N�g</summary>
        private IDictionary _interlockGroups;

        /// <summary>
        /// ������
        /// </summary>
        public override void OnInitInstance()
        {
            _interlockGroups = (IDictionary)Share.Get("SwitchInterlockGroup", typeof(Hashtable));
        }


        /// <summary>
        /// �p�����[�^���擾���A�t�B�[�`���[��g�[�N�����s�Ȃǂ��s��
        /// </summary>
        private void resetSwitch()
        {
            var ss = GetParamString().Split(new char[] { ',' });
            var fis = GetRoot().GetChildFeatureInstance();
            var tokenLidName = string.Empty;
            foreach (var s in ss)
            {
                var s2 = s.Trim();
                if (s2.StartsWith("[")) // �`�F�b�N�{�b�N�X�֘A�͊֌W�Ȃ�
                {
                    continue;
                }
                // �g�[�N���A�V�F�A��ݒ肷��
                var com = s2.Split(new char[] { '=' });
                if (com.Length == 2)
                {
                    if (com[0].Trim().Equals("Command", StringComparison.CurrentCultureIgnoreCase))
                    {
                        switch (com[1].Trim().ToUpper())
                        {
                            case "REDRAW":
                                Pane.Invalidate(null);
                                break;
                            case "KEEPOFF":
                                ThreadSafe.SetChecked(_checkBox, false);
                                _isOn = false;
                                break;

                        }
                    }
                    if (com[0].Trim().Equals("Token", StringComparison.CurrentCultureIgnoreCase))
                    {
                        tokenLidName = com[1].Trim();
                    }
                    if (com[0].Trim().Equals("ShareBool", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var b = (DataSharingManager.Boolean)Share.Get(com[1].Trim(), typeof(DataSharingManager.Boolean));
                        b.value = _isOn;
                    }
                }
                else
                {
                    // �t�B�[�`���[��Enabled��ݒ肷��
                    foreach (FeatureBase fi in fis)
                    {
                        if (s2.StartsWith("@"))
                        {
                            if (fi.Name == s2.Substring(1))
                            {
                                fi.Enabled = _isOn;
                            }
                        }
                        else
                        {
                            if (fi.GetType().Name == s2)
                            {
                                fi.Enabled = _isOn;
                            }
                        }
                    }
                }
            }
            // �g�[�N���𓊂���i�t�B�[�`���[��Enable��ݒ肵����j
            if (_isNoToken == false)
            {
                if (tokenLidName != string.Empty)
                {
                    Token.Add(NamedId.FromName(tokenLidName), this);
                }
            }
            else
            {
                _isNoToken = false;
            }
        }

        /// <summary>
        /// �����񂩂�p�����[�^����́E���s����
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            var ss = param.Split(new char[] { ',' });
            foreach (var s in ss)
            {
                var s2 = s.Trim();

                // �C���^�[���b�N�O���[�v
                var id = s2.IndexOf("(");
                if (id >= 0)
                {
                    var gr = s2.Substring(id + 1);
                    _interlockGroup = gr.Substring(0, gr.Length - 2);
                    var fcs = (IList)_interlockGroups[_interlockGroup];
                    if (fcs == null)
                    {
                        _interlockGroups[_interlockGroup] = fcs = new ArrayList();
                    }
                    fcs.Add(this);

                    s2 = s2.Substring(0, id) + "]";
                }
                // �`�F�b�N�{�^���̃C�x���g�o�^
                if (s2.StartsWith("["))
                {
                    var s3 = s2.Substring(1, s2.Length - 2);
                    var sss = s3.Split(new char[] { '=' });
                    s2 = sss[0];
                    if (sss.Length >= 2)
                    {
                        if (Const.IsFalse(sss[1]))
                        {
                            _isOn = false;
                        }
                    }
                    _initialState = _isOn;
                    _checkBox = (System.Windows.Forms.CheckBox)GetControl(s2);
                    if (_checkBox != null)
                    {
                        AddTrigger(_checkBox, "Click", new EventHandler(onClickCheck));
                        ThreadSafe.SetChecked(_checkBox, _isOn);
                        ThreadSafe.SetEnabled(_checkBox, _canStart);
                    }
                }
            }
            _isNoToken = true;
            Finalizers.Add(new FinalizeManager.Finalize(resetSwitch));
        }
        private bool _isNoToken = false;

        private void onClickCheck(object sender, EventArgs e)
        {
            Start(null);
        }

        /// <summary>
        /// �f�[�^�������ꂽ�Ƃ��̃C�x���g
        /// </summary>
        protected virtual void OnDataCleared()
        {
            _isOn = _initialState;  // �X�C�b�`�����Z�b�g����
            resetSwitch();
        }

        public override void OnCommandParameter(string arg)
        {
            Start(null);
        }

        /// <summary>
        /// �؂�ւ�
        /// </summary>
        public override void Start(NamedId who)
        {
            if (_tokenSwitch.Equals(who))
            {
                var sw = (FeatureSwitch.TagFeatureSwitch)Share.Get("TokenChangeFeatureSwitches", typeof(FeatureSwitch.TagFeatureSwitch));
                if (ID == sw.switchingFeatureID)
                {
                    _isOn = sw.sw;
                    if (_checkBox != null)
                    {
                        ThreadSafe.SetChecked(_checkBox, _isOn);
                    }
                }
                else
                {
                    return;
                }
            }
            else
            if (_tokenReadCompleted.Equals(who) == false)
            {
                if (_interlockGroup == "")
                {
                    // UNDO/REDO���Ȃ���A�X�C�b�`�؂�ւ�
                    Persister[UNDO].StartChunk(GetType().Name + ".Start");
                    Persister[REDO].StartChunk(GetType().Name + ".Start");

                    Persister[UNDO].Save(new TagFeatureSwitch(this, _isOn), _featureDataID);
                    _isOn = (_isOn ? false : true);
                    Persister[REDO].Save(new TagFeatureSwitch(this, _isOn), _featureDataID);

                    Persister[UNDO].EndChunk();
                    Persister[REDO].EndChunk();
                }
                else
                {
                    if (_isOn == false)
                    {
                        // UNDO/REDO���Ȃ���A�O���[�v�̑����OFF�ɂ���B
                        Persister[UNDO].StartChunk(GetType().Name + ".Start");
                        Persister[REDO].StartChunk(GetType().Name + ".Start");

                        var ig = (IList)_interlockGroups[_interlockGroup];
                        foreach (FeatureSwitch swf in ig)
                        {
                            if (object.ReferenceEquals(this, swf))
                            {
                                continue;
                            }
                            if (swf._isOn != false)
                            {
                                Persister[UNDO].Save(new TagFeatureSwitch(swf, swf._isOn), _featureDataID);
                                swf._isOn = false;
                                Persister[REDO].Save(new TagFeatureSwitch(swf, swf._isOn), _featureDataID);
                                if (swf._checkBox != null)
                                {
                                    ThreadSafe.SetChecked(swf._checkBox, swf._isOn);
                                }
                                swf.resetSwitch();
                            }
                        }
                        Persister[UNDO].Save(new TagFeatureSwitch(this, _isOn), _featureDataID);
                        _isOn = true;
                        Persister[REDO].Save(new TagFeatureSwitch(this, _isOn), _featureDataID);

                        Persister[UNDO].EndChunk();
                        Persister[REDO].EndChunk();
                    }
                }
            }
            if (_checkBox != null)
            {
                ThreadSafe.SetChecked(_checkBox, _isOn);
            }

            resetSwitch();
        }


        public override bool Checked
        {
            get
            {
                if (_checkBox != null)
                {
                    ThreadSafe.SetChecked(_checkBox, _isOn);
                }
                return _isOn;
            }
        }

        /// <summary>
        /// �X�C�b�`�����s����
        /// </summary>
        /// <param name="onoff"></param>
        public void Switch(bool onoff)
        {
            _isOn = onoff;
            resetSwitch();
        }

        /// <summary>
        /// �X�C�b�`�̓��삪�\���ǂ����iFALSE�j���ƁA�g�[�N�����������Ȃ�
        /// </summary>
        public override bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (_checkBox != null)
                {
                    ThreadSafe.SetEnabled(_checkBox, value);
                }
            }
        }


        /// <summary>
        /// ���j���[�X�^�[�g�ł��邩�ǂ����̃t���O�i�g�[�N�������͂���j
        /// </summary>
        public override bool CanStart => _canStart;

        #region ���g�p
#if false
		/// <summary>
		/// ������OFF�ɂ��āA���̒N����ON�ɂ���
		/// </summary>
		private void setFalse()
		{
			IList ig = (IList)_interlockGroups[_interlockGroup];
			foreach( FeatureSwitch swf in ig )
			{
				if( object.ReferenceEquals(this, swf))
				{
					continue;
				}
				swf.RequestStartup();
				break;
			}
		}
#endif
        #endregion

        /// <summary>
        /// ���j���[�X�^�[�g�ۃX�C�b�`��ύX����
        /// </summary>
        /// <param name="sw">�X�^�[�g��</param>
        protected void setCanStart(bool sw)
        {
            _canStart = sw;
            if (_checkBox != null)
            {
                ThreadSafe.SetEnabled(_checkBox, _canStart);
            }
        }

        #region IMultiTokenListener �����o

        public virtual NamedId[] MultiTokenTriggerID => _tokens;

        #endregion
    }
}
