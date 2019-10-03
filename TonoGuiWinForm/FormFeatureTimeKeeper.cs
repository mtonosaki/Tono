using System;
using System.Collections.Generic;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public partial class FormFeatureTimeKeeper : Form
    {
        /// <summary>
        /// ユニットを表現するクラス
        /// </summary>
        private class Unit
        {
            public double Value;
            public string Caption;

            public Unit(string cap, double value)
            {
                Value = value;
                Caption = cap;
            }

            public override string ToString()
            {
                return Caption;
            }
        }

        private class TpId
        {
            public TimeKeeper.RecordType TYPE;
            public Id ID;

            public TpId(TimeKeeper.RecordType tp, Id id)
            {
                TYPE = tp;
                ID = id;
            }
        }

        private readonly FeatureGroupRoot _root = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FormFeatureTimeKeeper(FeatureGroupRoot rootgroup)
        {
            _root = rootgroup;
            InitializeComponent();
            new FormShapePersister(this);
        }

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foFeatureTimeKeeper_Load(object sender, EventArgs e)
        {
            comboBoxUnit.Items.Add(new Unit("per hour", 3600));
            comboBoxUnit.Items.Add(new Unit("per min", 60));
            comboBoxUnit.Items.Add(new Unit("per sec", 1));
            comboBoxUnit.Items.Add(new Unit("per msec", 0.001));
            comboBoxUnit.Items.Add(new Unit("per μsec", 0.000001));

            comboBoxUnit.SelectedIndex = 2;
            comboBoxUnit.SelectedIndexChanged += new EventHandler(comboBoxUnit_SelectedIndexChanged);
            build();
            recalc();
        }

        private void comboBoxUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            recalc();
        }

        /// <summary>
        /// 終了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonRecalc_Click(object sender, EventArgs e)
        {
            recalc();
        }

        private void build()
        {
            listView1.Items.Clear();

            var fcs = new Dictionary<Id, FeatureBase>();

            foreach (var fcfg in _root.GetChildFeatureInstance())
            {
                if (fcfg is FeatureBase fc)
                {
                    fcs[fc.ID] = fc;
                }
            }

            foreach (var id in TimeKeeper.GetIDs())
            {
                foreach (var tpname in Enum.GetNames(typeof(TimeKeeper.RecordType)))
                {
                    var tp = (TimeKeeper.RecordType)Enum.Parse(typeof(TimeKeeper.RecordType), tpname);
                    var lvi = new ListViewItem
                    {
                        Tag = new TpId(tp, id)
                    };

                    if (fcs.ContainsKey(id))
                    {
                        var fc = fcs[id];
                        lvi.Text = fc.GetType().Name;
                        lvi.SubItems.Add(fc.ID.ToString());
                        lvi.SubItems.Add(tpname);
                        lvi.SubItems.Add("0");
                        lvi.SubItems.Add("0");
                        lvi.SubItems.Add("0");

                        listView1.Items.Add(lvi);
                    }
                }
            }
        }

        /// <summary>
        /// 再計算
        /// </summary>
        private void recalc()
        {
            var unit = comboBoxUnit.SelectedItem as Unit;
            foreach (ListViewItem lvi in listView1.Items)
            {
                var ti = lvi.Tag as TpId;
                lvi.SubItems[3].Text = string.Format("{0:0.000}", TimeKeeper.CalcRamda(ti.TYPE, ti.ID) * unit.Value);
                lvi.SubItems[4].Text = string.Format("{0:0.000}", TimeKeeper.CalcMu(ti.TYPE, ti.ID) * unit.Value);
                lvi.SubItems[5].Text = string.Format("{0:0}", TimeKeeper.GetN(ti.TYPE, ti.ID));
            }
        }

        private void buttonReset_Click(object sender, EventArgs e)
        {
            TimeKeeper.Reset();
            recalc();
        }
    }
}