using System;
using System.Collections;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// FeatureSwitch の概要の説明です。
    /// パラメータで指定したフィーチャーのスイッチを切り替えることができます。
    /// 指定できるのは、フィーチャークラス名、フィーチャーインスタンス名です。
    /// 例１：	FeatureWheelScroll,FeatureWheelZoom	→　両フィーチャークラス名のスイッチング
    /// 例２：	FeatureWheelScroll,@ZoomZoom		→　FeatureWheelScrollクラスのインスタンス全部と ZoomZoomというインスタンス名のフィーチャーのスイッチング
    /// 参考）フィーチャーのインスタンス名は、XML の name属性で指定できる、FeatureBase.Nameプロパティのことです。
    /// </summary>
    public class FeatureSwitch : FeatureControlBridgeBase, IMultiTokenListener, IAutoRemovable
    {
        #region UNDO/REDOタグ TagFeatureSwitch
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

        /// <summary>読み込み完了を示すトークンID</summary>
        protected static readonly NamedId _tokenReadCompleted = NamedId.FromName("TokenReadCompleted");
        /// <summary>選択中のパーツという意味でシリアライズするID</summary>
        protected static readonly NamedId _featureDataID = NamedId.FromName("FeatureDataSerializeID");
        /// <summary>トークンで切り替える</summary>
        protected static readonly NamedId _tokenSwitch = NamedId.FromName("TokenChangeFeatureSwitches");

        private readonly NamedId[] _tokens = new NamedId[] { _tokenReadCompleted, _tokenSwitch };

        /// <summary>共有変数：インターロック名とスイッチオブジェクト</summary>
        private IDictionary _interlockGroups;

        /// <summary>
        /// 初期化
        /// </summary>
        public override void OnInitInstance()
        {
            _interlockGroups = (IDictionary)Share.Get("SwitchInterlockGroup", typeof(Hashtable));
        }


        /// <summary>
        /// パラメータを取得し、フィーチャーやトークン発行などを行う
        /// </summary>
        private void resetSwitch()
        {
            var ss = GetParamString().Split(new char[] { ',' });
            var fis = GetRoot().GetChildFeatureInstance();
            var tokenLidName = string.Empty;
            foreach (var s in ss)
            {
                var s2 = s.Trim();
                if (s2.StartsWith("[")) // チェックボックス関連は関係ない
                {
                    continue;
                }
                // トークン、シェアを設定する
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
                    // フィーチャーのEnabledを設定する
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
            // トークンを投げる（フィーチャーのEnableを設定した後）
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
        /// 文字列からパラメータを解析・実行する
        /// </summary>
        /// <param name="param"></param>
        public override void ParseParameter(string param)
        {
            var ss = param.Split(new char[] { ',' });
            foreach (var s in ss)
            {
                var s2 = s.Trim();

                // インターロックグループ
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
                // チェックボタンのイベント登録
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
        /// データ消去されたときのイベント
        /// </summary>
        protected virtual void OnDataCleared()
        {
            _isOn = _initialState;  // スイッチをリセットする
            resetSwitch();
        }

        public override void OnCommandParameter(string arg)
        {
            Start(null);
        }

        /// <summary>
        /// 切り替え
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
                    // UNDO/REDOしながら、スイッチ切り替え
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
                        // UNDO/REDOしながら、グループの相手をOFFにする。
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
        /// スイッチを実行する
        /// </summary>
        /// <param name="onoff"></param>
        public void Switch(bool onoff)
        {
            _isOn = onoff;
            resetSwitch();
        }

        /// <summary>
        /// スイッチの動作が可能かどうか（FALSE）だと、トークン処理もしない
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
        /// メニュースタートできるかどうかのフラグ（トークン処理はする）
        /// </summary>
        public override bool CanStart => _canStart;

        #region 未使用
#if false
		/// <summary>
		/// 自分をOFFにして、他の誰かをONにする
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
        /// メニュースタート可否スイッチを変更する
        /// </summary>
        /// <param name="sw">スタート可否</param>
        protected void setCanStart(bool sw)
        {
            _canStart = sw;
            if (_checkBox != null)
            {
                ThreadSafe.SetEnabled(_checkBox, _canStart);
            }
        }

        #region IMultiTokenListener メンバ

        public virtual NamedId[] MultiTokenTriggerID => _tokens;

        #endregion
    }
}
