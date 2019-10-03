using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

#pragma warning disable 1591, 1572, 1573

namespace Tono.GuiWinForm
{
    public partial class TVersionList : UserControl
    {
        public TVersionList()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (DesignMode)
            {
                return;
            }

            // バージョン情報を収集する
            var ass = new object[3];
            ass[0] = Assembly.GetEntryAssembly();
            ass[1] = Assembly.GetCallingAssembly();
            ass[2] = Assembly.GetExecutingAssembly();
            var ex = new ArrayList();

            for (var i = 0; i < ass.Length; i++)
            {
                var isIn = false;
                foreach (var o in ex)
                {
                    if (object.ReferenceEquals(o, ass[i]))
                    {
                        isIn = true;
                        break;
                    }
                }
                if (isIn)
                {
                    continue;
                }
                var a = (Assembly)ass[i];
                if (a == null)
                {
                    continue;
                }
                var an = a.GetName();
                var li = listView.Items.Add(an.Name);
                li.SubItems.Add(an.Version.ToString());
                li.SubItems.Add(an.CodeBase);
                ex.Add(a);
            }
            var ret = FeatureLoader2.GetUsingClasses();
            for (var i = 0; i < ret.Length; i++)
            {
                var isIn = false;
                var ss = ret[i];
                var an = ss.GetName();
                foreach (ListViewItem lvi in listView.Items)
                {
                    if (lvi.Text == an.Name)
                    {
                        isIn = true;
                        break;
                    }
                }
                if (isIn)
                {
                    continue;
                }
                var li = listView.Items.Add(an.Name);
                li.SubItems.Add(an.Version.ToString());
                li.SubItems.Add(an.CodeBase);
            }
        }
    }
}
