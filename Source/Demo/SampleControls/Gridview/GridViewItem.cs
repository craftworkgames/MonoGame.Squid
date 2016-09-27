using System.Collections.Generic;
using System.ComponentModel;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace Demo.SampleControls.Gridview
{
    public class GridViewItem : GridViewSubItem
    {
        internal List<Control> Tags = new List<Control>();

        public GridViewItem()
        {
            SubItems = new ActiveList<GridViewSubItem>();
            Size = new Point(100, 20);
            Dock = DockStyle.Top;
        }

        public object Value { get; set; }

        public ActiveList<GridViewSubItem> SubItems { get; set; }

        protected override void OnLateUpdate()
        {
            if (Equals(Desktop.HotControl))
                foreach (var item in SubItems)
                    item.State = State;
            else
                foreach (var item in SubItems)
                    if (item.Equals(Desktop.HotControl))
                    {
                        State = item.State;

                        foreach (var i in SubItems)
                            if (!i.Equals(item))
                                i.State = item.State;
                        break;
                    }
        }
    }

    public class GridViewSubItem : Label
    {
        private bool _selected;

        public GridViewSubItem()
        {
            Size = new Point(100, 20);
            Dock = DockStyle.Top;
            //Style = "item";
        }

        [DefaultValue(false)]
        public new bool Selected
        {
            get { return _selected; }
            set
            {
                if (value == _selected) return;

                _selected = value;

                if (OnSelectedChanged != null)
                    OnSelectedChanged(this);
            }
        }

        public event VoidEvent OnSelectedChanged;
    }
}