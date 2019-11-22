// Copyright (c) Manabu Tonosaki All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Illustios = System.Collections.Generic.List<Tono.GuiWinForm.PartsIllusionProjector>;
using Layers = System.Collections.Generic.Dictionary<int/*���C��ID*/, System.Collections.Generic.List<Tono.GuiWinForm.PartsBase>>;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    /// <summary>
    /// daPartsset �̊T�v�̐����ł��B
    /// �����̃p�[�c���Ǘ�����N���X
    /// </summary>
    public class PartsCollection : PartsCollectionBase
    {
        #region �S�p�[�c�񋓗p�N���X
        /// <summary>
        /// �p�[�c�񋓐���
        /// </summary>
        private class PartsEnumerator : IPartsEnumerator
        {
            private readonly IDictionary _orgData;
            private IDictionaryEnumerator _rpe;
            private IDictionaryEnumerator _lve;
            private IEnumerator _pte;

            public PartsEnumerator(IDictionary value)
            {
                _orgData = value;
                Reset();
            }

            #region IPartsEnumerator �����o

            public PartsBase Parts => null;

            public IRichPane Pane => null;

            #endregion

            #region IEnumerator �����o

            public void Reset()
            {
                _rpe = _orgData.GetEnumerator();
                _lve = null;
                _pte = null;
            }

            public object Current
            {
                get
                {
                    if (_pte == null)
                    {
                        return null;
                    }
                    var pe = new PartsEntry
                    {
                        Parts = (PartsBase)_pte.Current,
                        Pane = (IRichPane)_rpe.Key,
                        LayerLevel = (int)_lve.Key
                    };
                    return pe;
                }
            }

            public bool MoveNext()
            {
                if (_pte == null)
                {
                    if (_lve == null)
                    {
                        if (_rpe.MoveNext() == false)
                        {
                            return false;
                        }
                        _lve = ((IDictionary)_rpe.Value).GetEnumerator();
                        return MoveNext();
                    }
                    if (_lve.MoveNext() == false)
                    {
                        _lve = null;
                        return MoveNext();
                    }
                    _pte = ((ICollection)_lve.Value).GetEnumerator();
                    return MoveNext();
                }
                if (_pte.MoveNext())
                {
                    return true;
                }
                _pte = null;
                return MoveNext();
            }

            #endregion
        }

        #endregion
        #region �����i�V���A���C�Y����j

        /// <summary>
        /// �f�[�^���Ǘ�����z��
        /// </summary>
        protected Dictionary<IRichPane, Layers> _data = new Dictionary<IRichPane, Layers>();  /*<IRichPane, ArrayList<dpBase>>*/

        /// <summary>
        /// ���C���[�ԍ��\�[�g�p
        /// </summary>
        protected Dictionary<IRichPane, List<int>> _layerNos = new Dictionary<IRichPane, List<int>>();

        /// <summary>
        /// ���C���[�\���X�C�b�` true=�\��
        /// </summary>
        protected Dictionary<int, bool> _layerVisibleSwitch = new Dictionary<int, bool>();

        /// <summary>
        /// �g�p����C�����[�W����
        /// </summary>
        protected Dictionary<IRichPane, Illustios> _projectors = new Dictionary<IRichPane, Illustios>();    /*<IRichPane, ArrayIllustios>*/

        /// <summary>
        /// �C�����[�W�����X�N���[������I���W�i������������L�[
        /// </summary>
        protected Dictionary<IRichPane, IRichPane> _projectorsRevKey = new Dictionary<IRichPane, IRichPane>();

        #endregion

        /// <summary>
        /// �f�t�H���g�R���X�g���N�^
        /// </summary>
        public PartsCollection() : base()
        {
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        public PartsCollection(IRichPane pane, ICollection tars) : base()
        {
            foreach (PartsBase pt in tars)
            {
                Add(pane, pt);
            }
        }

        /// <summary>
        /// �������R���X�g���N�^
        /// </summary>
        public PartsCollection(IRichPane pane, ICollection<PartsBase> tars) : base()
        {
            foreach (var pt in tars)
            {
                Add(pane, pt);
            }
        }

        /// <summary>
        /// �\���X�C�b�`
        /// </summary>
        /// <param name="layerlevel"></param>
        /// <param name="sw"></param>
        public void SetLayerVisible(int layerlevel, bool sw)
        {
            _layerVisibleSwitch[layerlevel] = sw;
        }

        /// <summary>
        /// ���C���[���w�肵�ăp�[�c���擾����
        /// �i�ŏ��Ɍ��������y�[���̂݁j
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public virtual IEnumerable<PartsBase> GetLayerParts(int layer)
        {
            foreach (var layers in _data.Values)
            {
                if (layers.TryGetValue(layer, out var ret))
                {
                    return ret;
                }
            }
            return new List<PartsBase>();
        }

        /// <summary>
        /// ���C���[��ύX����
        /// </summary>
        /// <param name="pane"></param>
        /// <param name="layer_from"></param>
        /// <param name="parts"></param>
        /// <param name="layer_to"></param>
        public void MovePartsLayer(IRichPane pane, int layer_from, PartsBase parts, int layer_to)
        {
            var ls = _data[pane];
            if (ls.TryGetValue(layer_from, out var listf) == false)
            {
                listf = new List<PartsBase>();
            }
            if (ls.TryGetValue(layer_to, out var listt) == false)
            {
                listt = makeNewLayer(pane, layer_to, ls);
            }
            listf.Remove(parts);
            listt.Remove(parts);
            listt.Add(parts);
        }

        /// <summary>
        /// ���C���[���w�肵�ăp�[�c���擾����
        /// �i�ŏ��ɂ݂������y�[���̂݁j
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="retPane">�w�背�C���[��������y�[�����Ԃ�</param>
        /// <returns>�p�[�c�R���N�V����</returns>
        public virtual IList<PartsBase> GetLayerParts(int layer, out IRichPane retPane)
        {

            foreach (var de in _data)
            {
                var layers = de.Value;
                if (layers.TryGetValue(layer, out var ret))
                {
                    retPane = de.Key;
                    return ret;
                }
            }
            retPane = null;
            return new List<PartsBase>();
        }

        /// <summary>
        /// ���C���[���w�肵�ăp�[�c���擾����
        /// </summary>
        /// <param name="layer">�w�背�C���[</param>
        /// <param name="pane">�w��y�[��</param>
        /// <returns></returns>
        public virtual IList<PartsBase> GetLayerParts(int layer, IRichPane pane)
        {
            var layers = _data[pane];
            if (layers.TryGetValue(layer, out var ret))
            {
                return ret;
            }
            return new List<PartsBase>();
        }

        /// <summary>
        /// �w��ID�i�ʏ��uRowKey�j���w�肵�āAdpBase.LT.Y == pos.ID �̍s�����ׂĂ�߂�
        /// </summary>
        /// <param name="pos">�����L�[</param>
        /// <returns>�w��L�[�ɍ��v����p�[�c�Q</returns>
        public override IList<PartsBase> GetPartsByLocationID(Id pos)
        {
            var ret = new List<PartsBase>();

            foreach (var layers in _data.Values)
            {
                foreach (var list in layers.Values)
                {
                    foreach (var dp in list)
                    {
                        if (dp.Rect.LT.Y == pos.Value)
                        {
                            ret.Add(dp);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// �y�[�����w�肵�āA�w��ID�i�ʏ��uRowKey�j���w�肵�āA
        /// dpBase.LT.Y == pos.ID �̍s�̎w��y�[���̕`��̈���ɂ���p�[�c��߂�
        /// </summary>
        /// <param name="rp">�`��Ώۃy�[��</param>
        /// <param name="pos">�����L�[</param>
        /// <returns>�w��L�[�ɍ��v����p�[�c�Q</returns>
        public override IList<PartsBase> GetPartsByLocationID(IRichPane rp, Id pos)
        {
            // HACK: Slow GetPartsByLocationID method
            var ret = new List<PartsBase>();

            foreach (var layers in _data.Values)
            {
                foreach (var list in layers.Values)
                {
                    foreach (var dp in list)
                    {
                        if (dp.Rect.LT.Y == pos.Value)
                        {
                            ret.Add(dp);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// �w��p�[�c�Əd�Ȃ��Ă���p�[�c�����ׂĎ擾����
        /// </summary>
        /// <param name="partsClass">�p�[�c�̃N���X�^�C�v typeof(object)�őS��</param>
        /// <param name="tar">�擾�Ώ�</param>
        /// <param name="rp">�y�[��</param>
        /// <param name="checkIllustion"></param>
        /// <returns>�p�[�c�̃R���N�V����</returns>
        public override ICollection<PartsBase> GetOverlappedParts(Type partsClass, PartsBase tar, IRichPane rp, bool checkIllustion)
        {
            //HACK: Slow GetOverlappedParts method
            var ret = new List<PartsBase>();

            if (_data.TryGetValue(rp, out var layers))
            {
                var lnos = _layerNos[rp];
                for (var lnosid = 0; lnosid < lnos.Count; lnosid++)
                {
                    if (_layerVisibleSwitch[lnos[lnosid]])
                    {
                        var list = layers[lnos[lnosid]];
                        foreach (var dp in list)
                        {
                            var t = dp.GetType();
                            if (dp != tar && (t.Equals(partsClass) || t.IsSubclassOf(partsClass)))
                            {
                                if (IsOverlapped(rp, tar, rp, dp, checkIllustion))
                                {
                                    ret.Add(dp);
                                }
                            }
                        }
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// �C�����[�W�����v���W�F�N�^��o�^����
        /// </summary>
        /// <param name="original">���e��</param>
        /// <param name="idtext"></param>
        /// <returns></returns>
        public PartsIllusionProjector AddIllusionProjector(IRichPane original, string idtext)
        {
            var ret = new PartsIllusionProjector(original, idtext);
            if (_projectors.TryGetValue(original, out var prs) == false)
            {
                _projectors.Add(original, prs = new Illustios());
            }
            prs.Add(ret);
            _projectorsRevKey.Add(ret.ScreenPane, original);
            return ret;
        }

        /// <summary>
        /// �w�肵���p�[�c���y�[�����A���C���[�Ȃ��ōŏ�Ɉړ�����
        /// </summary>
        /// <param name="tar">�ړ�������p�[�c</param>
        public virtual void MovePartsZOrderToTop(PartsBase tar)
        {
            foreach (var layers in _data.Values)
            {
                foreach (var list in layers.Values)
                {
                    if (list.Contains(tar))
                    {
                        list.Remove(tar);
                        list.Insert(list.Count, tar);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// �ǉ�����
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <param name="layerLevel"></param>
        private void _addProc(IRichPane target, PartsBase value, int layerLevel)
        {
            try
            {

                if (!_data.TryGetValue(target, out var layers))
                {
                    // �y�[����o�^
                    _data.Add(target, new Layers());    // TONO
                    _addProc(target, value, layerLevel);
                    return;
                }
                if (!layers.TryGetValue(layerLevel, out var ps))
                {
                    // ���C���[��o�^
                    makeNewLayer(target, layerLevel, layers);
                    _addProc(target, value, layerLevel);
                    return;
                }
                ps.Add(value);
            }
            catch (System.NullReferenceException)
            {
                _data[target] = new Layers();
                _addProc(target, value, layerLevel);
            }
        }

        /// <summary>
        /// �V�������C���[�����
        /// </summary>
        /// <param name="target"></param>
        /// <param name="layerLevel"></param>
        /// <param name="layers"></param>
        /// <returns></returns>
        private List<PartsBase> makeNewLayer(IRichPane target, int layerLevel, Layers layers)
        {
            List<PartsBase> ret;
            layers[layerLevel] = ret = new List<PartsBase>();
            _layerVisibleSwitch[layerLevel] = true;

            if (_layerNos.TryGetValue(target, out var lnos) == false)
            {
                lnos = _layerNos[target] = new List<int>();

            }
            if (lnos.Contains(layerLevel) == false)
            {
                lnos.Add(layerLevel);
                lnos.Sort();
            }
            return ret;
        }

        /// <summary>
        /// �p�[�c��ǉ�����
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value">�ǉ�����p�[�c</param>
        /// <param name="layerLevel"></param>
        public override void Add(IRichPane target, PartsBase value, int layerLevel)
        {
            lock (_data)
            {
                base.Add(target, value, layerLevel);    // �K�{

                _addProc(target, value, layerLevel);
            }
        }

        /// <summary>
        /// �w��y�[����Enable�ȃv���W�F�N�^���X�g���擾����
        /// ���̊֐��́ARichPaneBinder���w�肵���ۂɁA�I���W�i����Ԃ��H�v���s��
        /// ����ɂ��A_projectors[�y�[��]���\�ƂȂ�
        /// </summary>
        /// <param name="tar">���X�g���擾���邽�߂̃L�[�ƂȂ�y�[��</param>
        /// <param name="isAll">true=Enabled=false���ΏۂɊ܂߂�2011.3.8</param>
        /// <returns>�v���W�F�N�^���X�g</returns>
        protected Illustios getProjectors(IRichPane tar, bool isAll)
        {
            if (tar is RichPaneBinder)
            {
                // �l�ƂȂ�y�[�����烊�X�g���擾
                if (_projectorsRevKey.TryGetValue(tar, out var key))
                {
                    if (_projectors.TryGetValue(key, out var ret))
                    {
                        var cpy = new Illustios();
                        foreach (var ip in ret)
                        {
                            if (ip.Enabled || isAll)
                            {
                                cpy.Add(ip);
                            }
                        }
                        return cpy;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            // �L�[�ƂȂ�y�[�����烊�X�g���擾
            if (_projectors.TryGetValue(tar, out var ret2))
            {
                var cpy = new Illustios();
                foreach (var ip in ret2)
                {
                    if (ip.Enabled || isAll)
                    {
                        cpy.Add(ip);
                    }
                }
                return cpy;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// �w�肵����̃p�[�c�̏d�Ȃ蔻�������
        /// </summary>
        /// <param name="p1">�p�[�c�P</param>
        /// <param name="p2">�p�[�c�Q</param>
        /// <param name="isIllusionCheck">true = �C�����[�W�������l������</param>
        /// <returns>true = �d�Ȃ��Ă��� / false = �d�Ȃ��Ă��Ȃ�</returns>
        public override bool IsOverlapped(IRichPane pane1, PartsBase parts1, IRichPane pane2, PartsBase parts2, bool isIllusionCheck)
        {
            try
            {
                if (isIllusionCheck)
                {
                    foreach (IRichPane pp1 in PartsIllusionProjector.GetEnumerator(pane1, getProjectors(pane1, false), parts1))
                    {
                        var sr1 = parts1.GetScRect(pp1, parts1.Rect);
                        foreach (IRichPane pp2 in PartsIllusionProjector.GetEnumerator(pane2, getProjectors(pane2, false), parts2))
                        {
                            var sr2 = parts2.GetScRect(pp2, parts2.Rect);
                            var union = sr1 & sr2;
                            if (union != null)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    if (pane1 == pane2)
                    {
                        var pt1 = parts1.GetPtRect(parts1.Rect);
                        var pt2 = parts2.GetPtRect(parts2.Rect);
                        return pt1.IsIn(pt2);
                    }
                    else
                    {
                        var sr1 = parts1.GetScRect(pane1, parts1.Rect);
                        var sr2 = parts2.GetScRect(pane2, parts2.Rect);
                        return sr1.IsIn(sr2);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("IsOverlapped�͎��̗�O�ŃL�����Z��; " + e.Message);
            }
            return false;
        }

        /// <summary>
        /// �`�悳����iPaint�C�x���g������s����邽�߁A���[�U�[�͎��s�֎~
        /// </summary>
        public override void ProvideDrawFunction()
        {
            try
            {
                for (IDictionaryEnumerator de = _data.GetEnumerator(); de.MoveNext();)
                {
                    var pane = (IRichPane)de.Key;
                    pane.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    // �S�̂ɉe������`�揈��
                    var clipRect = pane.GetPaintClipRect() & pane.GetPaneRect();
                    if (clipRect != null)   // �N���b�v�ƃy�C���g�̗̈���̂ݕ`�悷��
                    {
                        PartsBase.Mask(pane); // �y�[���̈���}�X�N����
                                              //System.Diagnostics.Debug.WriteLine(clipRect.ToString());
                        using (Brush brush = new SolidBrush(pane.Control.BackColor))
                        {
                            pane.Graphics.FillRectangle(brush, clipRect); // �w�i��`��
                        }

                        // ���C���[�Ń��[�v����
                        var layers = (Layers)de.Value;
                        var lnos = _layerNos[pane];
                        for (var layerid = 0; layerid < lnos.Count; layerid++)
                        {
                            if (_layerVisibleSwitch[lnos[layerid]])
                            {
                                IEnumerable<PartsBase> pts = layers[lnos[layerid]];
                                // �`�悷��
                                drawLayer(pane, lnos[layerid], pts);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("�`�撆�ɗ�O�G" + e.Message);
            }
        }

        /// <summary>
        /// ���C���[��`��
        /// </summary>
        /// <param name="pane">�`�悷��y�[��</param>
        /// <param name="pts">�`�悷��p�[�c</param>
        protected virtual void drawLayer(IRichPane pane, int layerid, IEnumerable<PartsBase> pts)
        {
            foreach (var dp in pts)
            {
                foreach (IRichPane pp in PartsIllusionProjector.GetEnumerator(pane, getProjectors(pane, false), dp))
                {
#if DEBUG
                    try
                    {
                        dp.Draw(pp);
                    }
                    catch (Exception ex)
                    {
                        LOG.WriteLineException(ex);
                        //throw exinner;	// �����Ƀu���[�N�|�C���g��݂���ƁA�ǂ̃p�[�c�ŗ�O��������������ł���
                    }
#else
                    dp.Draw(pp);
#endif
                }
            }
        }

        /// <summary>
        /// �w�肵���p�[�c�R���N�V��������A�Y������p�[�c����������
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="pane"></param>
        /// <param name="isSelectableOnly"></param>
        /// <param name="pcol"></param>
        /// <param name="rp">�y�[����Ԃ��i�C�����[�W�����v���W�F�N�^�̃y�[�����Ԃ�̂ŁApane�ƈقȂ�ꍇ������j</param>
        /// <returns></returns>
        private PartsBase getparts(ScreenPos pos, IRichPane pane, bool isSelectableOnly, IList pcol, out IRichPane rp)
        {
            for (var pidx = pcol.Count - 1; pidx >= 0; pidx--)
            {
                var dp = (PartsBase)pcol[pidx];

                if (dp is IPartsVisible)
                {
                    if (((IPartsVisible)dp).Visible == false)
                    {
                        continue;
                    }
                }
                if (isSelectableOnly)
                {
                    if (dp is IPartsSelectable == false)
                    {
                        continue;
                    }
                }
                // �v���W�F�N�^��ʂ��āA�p�[�c���W�𒲍�����
                foreach (IRichPane pp in PartsIllusionProjector.GetEnumerator(pane, getProjectors(pane, false), dp))
                {
                    if (dp.IsOn(pos, pp) != PartsBase.PointType.Outside)
                    {
                        rp = pp;
                        return dp;
                    }
                }
            }
            rp = null;
            return null;
        }

        /// <summary>
        /// �w��̈悩�����ʒu�̃p�[�c��T��
        /// </summary>
        /// <param name="pos">�ʒu</param>
        /// <param name="pane">�����y�[��</param>
        /// <param name="layer">�������C���[</param>
        /// <param name="isSelectableOnly">�I���\�̂�</param>
        /// <returns>�擾�ł����p�[�c / null=�Ȃ�</returns>
        public override PartsBase GetPartsAt(ScreenPos pos, IRichPane pane, int layer, bool isSelectableOnly)
        {
            PartsBase ret = null;
            if (_data.TryGetValue(pane, out var layers))
            {
                if (layers.TryGetValue(layer, out var pcol))
                {
                    ret = getparts(pos, pane, isSelectableOnly, pcol, out var rp);
                }
            }
            return ret;
        }

        /// <summary>
        /// �w��}�E�X���W�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <param name="pos">�p�[�c���W</param>
        /// <returns>�擾�ł����p�[�c / null=�Ȃ�</returns>
        public override PartsBase GetPartsAt(ScreenPos pos, bool isSelectableOnly, out IRichPane rp)
        {
            rp = null;
            if (isInSkipzone(pos))
            {
                return null;
            }

            // ���b�`�y�[���ɂ�郋�[�v
            for (IDictionaryEnumerator de = _data.GetEnumerator(); de.MoveNext();)
            {
                var pane = (IRichPane)de.Key;

                if (pane.GetPaneRect().IsIn(pos) == false)
                {
                    continue;
                }

                // ���C���[�ɂ�郋�[�v
                for (var layerde = ((IDictionary)de.Value).GetEnumerator(); layerde.MoveNext();)
                {
                    if (_layerVisibleSwitch[(int)layerde.Key])
                    {
                        // ���ׂẴp�[�c�����ɒ�������A�ᑬ�̏����B�C�ɓ���Ȃ���΋@�\���I�[�o�[���C�h���Ă�������
                        // �������A�ȉ��̃v���W�F�N�^���l���������ɂ���K�v������܂��B
                        var pcol = (IList)layerde.Value;
                        var ret = getparts(pos, pane, isSelectableOnly, pcol, out rp);
                        if (ret != null)
                        {
                            return ret;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// �p�[�c��񋓂��邽�߂�IEnumerator���擾����
        /// </summary>
        /// <returns>IEnumerator</returns>
        public override IPartsEnumerator GetEnumerator()
        {
            return new PartsEnumerator(_data);
        }

        /// <summary>
        /// �S�y�[���ɓo�^����Ă���p�[�c�̐������v����
        /// </summary>
        public override int Count
        {
            get
            {
                var n = 0;
                foreach (var layers in _data.Values)
                {
                    foreach (IList<PartsBase> lde in layers.Values)
                    {
                        n += lde.Count;
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// �w��p�[�c���폜����
        /// </summary>
        /// <param name="value"></param>
        public override void Remove(PartsBase value)
        {
            base.Remove(value); // �K�{

            foreach (var layers in _data.Values)
            {
                foreach (var co in layers.Values)
                {
                    if (co.Contains(value))
                    {
                        co.Remove(value);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// ���ׂĂ��폜����
        /// </summary>
        public override void Clear()
        {
            lock (_data)
            {
                foreach (var de in _data.Keys)
                {
                    de.Invalidate(de.GetPaneRect());    // �p�[�c�̗̈���ĕ`�悷��
                }
                _data.Clear();
                _layerVisibleSwitch.Clear();
                _layerNos.Clear();
            }
        }

        /// <summary>
        /// �w�肵���^���폜����
        /// </summary>
        /// <param name="type"></param>
        public override int Clear(Type type)
        {
            lock (_data)
            {
                var dels = new List<PartsBase>();
                foreach (var kv in _data)
                {
                    var rp = kv.Key;
                    var layers = kv.Value;
                    foreach (var pts in layers.Values)
                    {
                        foreach (var p in pts)
                        {
                            if (p.GetType().IsSubclassOf(type) || p.GetType() == type)
                            {
                                dels.Add(p);
                            }
                        }
                    }
                }
                foreach (var p in dels)
                {
                    Remove(p);
                }
                return dels.Count;
            }
        }

        /// <summary>
        /// �w��y�[���̃p�[�c���폜����
        /// </summary>
        public override void Clear(IRichPane targetPane)
        {
            lock (_data)
            {
                if (_data.TryGetValue(targetPane, out var ls))
                {
                    ls.Clear();
                    targetPane.Invalidate(targetPane.GetPaneRect());    // �폜�����l�q���ĕ`��
                }
            }
        }

        /// <summary>
        /// �w��y�[���Ŏw�背�C���[�̃p�[�c���폜����
        /// </summary>
        public override void Clear(IRichPane targetPane, int layerLevel)
        {
            lock (_data)
            {
                if (_data.TryGetValue(targetPane, out var ls))
                {
                    if (ls.TryGetValue(layerLevel, out var ps))
                    {
                        ps.Clear();
                        targetPane.Invalidate(targetPane.GetPaneRect());    // �폜�����l�q���ĕ`��
                    }
                }
            }
        }

        /// <summary>
        /// �S�v�f���R�s�[����i�R�s�[��̓R�s�[���ƑS�������ɂȂ�j
        /// �e�v�f�i�p�[�c�j��Clone����Ȃ��ŎQ�ƂƂȂ�
        /// </summary>
        public override object Clone()
        {
            lock (_data)
            {
                var ret = new PartsCollection();
                copyBasePropertyTo(ret);
                foreach (PartsEntry pe in this)
                {
                    ret.Add(pe);
                }
                return ret;
            }
        }

        /// <summary>
        /// �̈�X�V��\�񂷂�i�v���W�F�N�^���T�|�[�g���Ă���̂ŁA������g�p���Ă��������j
        /// </summary>
        /// <param name="parts">�X�V����p�[�c</param>
        /// <param name="rp">�g�p����y�[��</param>
        public override void Invalidate(PartsBase parts, IRichPane rp)
        {
            try
            {
                foreach (IRichPane pp in PartsIllusionProjector.GetEnumerator(rp, getProjectors(rp, false), parts))
                {
                    base.Invalidate(parts, pp);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// �w��y�[���ɂ���w��^�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <param name="dpType">�^</param>
        /// <returns>�p�[�c�̃C���X�^���X�̎Q�� / null = ������Ȃ�����</returns>
        public override PartsBase GetSample(IRichPane rp, Type dpType)
        {
            lock (_data)
            {
                if (_data.TryGetValue(rp, out var layers))
                {
                    foreach (var pts in layers.Values)
                    {
                        foreach (var p in pts)
                        {
                            if (p.GetType().IsSubclassOf(dpType) || p.GetType() == dpType)
                            {
                                return p;
                            }
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// �w��y�[���ɂ���w��^�̃p�[�c���ЂƂ擾����
        /// </summary>
        /// <param name="rp">�y�[��</param>
        /// <param name="dpType">�^</param>
        /// <returns>�p�[�c�̃C���X�^���X�̎Q�� / null = ������Ȃ�����</returns>
        public override PartsBase GetSample()
        {
            lock (_data)
            {
                foreach (var layers in _data.Values)
                {
                    foreach (var pts in layers.Values)
                    {
                        foreach (var p in pts)
                        {
                            return p;
                        }
                    }
                }
                return null;
            }
        }
    }
}
