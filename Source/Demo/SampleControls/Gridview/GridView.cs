using System;
using MonoGame.Squid;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace Demo.SampleControls.Gridview
{
    public class GridViewItemCollection : ActiveList<GridViewItem>
    {
    }

    public class GridColumnCollection : ActiveList<GridColumn>
    {
    }

    public class GridViewEventArgs : EventArgs
    {
        public GridColumn Column;
    }

    [Toolbox]
    public class GridView : Control
    {
        private readonly Frame _headerContainer;
        private readonly Panel _panel;

        public GridView()
        {
            Size = new Point(100, 100);

            Columns = new GridColumnCollection();
            Columns.ItemAdded += Columns_OnAdd;
            Columns.ItemRemoved += Columns_OnRemove;
            Columns.BeforeItemRemoved += Columns_BeforeItemRemoved;
            Columns.BeforeItemAdded += Columns_BeforeItemAdded;
            Columns.BeforeItemsCleared += Columns_BeforeItemsCleared;

            Items = new GridViewItemCollection();
            Items.ItemAdded += Items_OnAdd;
            Items.ItemRemoved += Items_OnRemove;
            Items.BeforeItemsCleared += Items_BeforeItemsCleared;
            Items.ItemsSorted += Items_ItemsSorted;

            Header = new Frame();
            Header.Size = new Point(20, 32);
            Header.Dock = DockStyle.Top;
            Elements.Add(Header);

            _headerContainer = new Frame();
            _headerContainer.Size = new Point(20, 32);
            _headerContainer.AutoSize = AutoSize.Horizontal;
            Header.Controls.Add(_headerContainer);

            _panel = new Panel();
            _panel.Dock = DockStyle.Fill;
            _panel.Content.AutoSize = AutoSize.HorizontalVertical;

            _panel.VScroll.ButtonUp.Visible = false;
            _panel.VScroll.ButtonDown.Visible = false;
            _panel.VScroll.Size = new Point(13, 12);
            _panel.VScroll.Slider.Style = "vscrollTrack";
            _panel.VScroll.Slider.Button.Style = "vscrollButton";
            _panel.VScroll.Dock = DockStyle.Right;
            _panel.VScroll.Margin = new Margin(4, 0, 0, 0);

            _panel.HScroll.ButtonUp.Visible = false;
            _panel.HScroll.ButtonDown.Visible = false;
            _panel.HScroll.Size = new Point(13, 12);
            _panel.HScroll.Slider.Style = "vscrollTrack";
            _panel.HScroll.Slider.Button.Style = "vscrollButton";
            _panel.HScroll.Margin = new Margin(0, 4, 0, 0);
            _panel.Content.Update += Content_OnControlUpdate;
            Elements.Add(_panel);
        }

        public Frame Header { get; }

        public GridViewItemCollection Items { get; set; }
        public GridColumnCollection Columns { get; set; }
        public GridViewItem SelectedItem { get; set; }

        public bool Multiselect { get; set; }

        public event SelectedItemChangedEventHandler SelectedItemChanged;
        public event EventHandler<GridViewEventArgs> ColumnClicked;

        protected override void OnUpdate()
        {
            base.OnUpdate();

            // we want to perform mouse scrolling if:
            // the mouse is scrolling and there is any control hovered
            if ((Gui.MouseScroll != 0) && (Desktop.HotControl != null))
                if (Hit(Gui.MousePosition.X, Gui.MousePosition.Y))
                    if ((Desktop.HotControl == this) || Desktop.HotControl.IsChildOf(this))
                        _panel.VScroll.Scroll(Gui.MouseScroll);
        }

        private void Content_OnControlUpdate(Control sender)
        {
            _headerContainer.Position = new Point(_panel.Content.Position.X, 0);
            _headerContainer.PerformLayout();
        }

        private void Columns_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var item in Items)
                RemoveItem(item);

            foreach (var col in Columns)
            {
                _headerContainer.Controls.Remove(col.Header);
                _panel.Content.Controls.Remove(col.Container);

                col.Container = null;
                col.Header = null;
            }
        }

        private void Columns_BeforeItemAdded(object sender, ListEventArgs<GridColumn> e)
        {
            foreach (var item in Items)
                RemoveItem(item);
        }

        private void Columns_BeforeItemRemoved(object sender, ListEventArgs<GridColumn> e)
        {
            foreach (var item in Items)
                RemoveItem(item);
        }

        private void Items_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var item in Items)
                RemoveItem(item);
        }

        private void Items_ItemsSorted(object sender, EventArgs e)
        {
            foreach (var item in Items)
                RemoveItem(item);

            foreach (var item in Items)
                AddItem(item);
        }

        private void Columns_OnRemove(object sender, ListEventArgs<GridColumn> e)
        {
            _headerContainer.Controls.Remove(e.Item.Header);
            _panel.Content.Controls.Remove(e.Item.Container);

            e.Item.Container = null;
            e.Item.Header = null;

            foreach (var item in Items)
                AddItem(item);
        }

        private void Columns_OnAdd(object sender, ListEventArgs<GridColumn> e)
        {
            e.Item.Click += Column_Click;
            _headerContainer.Controls.Add(e.Item.Header);
            _panel.Content.Controls.Add(e.Item.Container);

            e.Item.Index = Columns.IndexOf(e.Item);

            foreach (var item in Items)
                AddItem(item);
        }

        private void Column_Click(object sender, EventArgs e)
        {
            if (ColumnClicked != null)
                ColumnClicked(this, new GridViewEventArgs {Column = sender as GridColumn});
        }

        private void AddItem(GridViewItem item)
        {
            item.Tags.Clear();

            var i = 0;
            foreach (var column in Columns)
            {
                if (i == 0)
                {
                    column.Container.Controls.Add(item);
                }
                else
                {
                    var index = i - 1;
                    if (item.SubItems.Count > index)
                    {
                        column.Container.Controls.Add(item.SubItems[index]);
                        item.Tags.Add(item.SubItems[index]);
                    }
                    else
                    {
                        var spacer = CreateSpace(item.Size.Y);

                        column.Container.Controls.Add(spacer);
                        item.Tags.Add(spacer);
                    }
                }

                i++;
            }
        }

        private void RemoveItem(GridViewItem item)
        {
            var i = 0;
            foreach (var column in Columns)
            {
                if (i == 0)
                {
                    column.Container.Controls.Remove(item);
                }
                else
                {
                    var index = i - 1;
                    if (item.Tags.Count > index)
                        column.Container.Controls.Remove(item.Tags[index]);
                }

                i++;
            }

            item.Tags.Clear();
        }

        private Control CreateSpace(int height)
        {
            var spacer = new Control();
            spacer.Size = new Point(height, height);
            spacer.Dock = DockStyle.Top;
            return spacer;
        }

        private void Items_OnRemove(object sender, ListEventArgs<GridViewItem> e)
        {
            RemoveItem(e.Item);
        }

        private void Items_OnAdd(object sender, ListEventArgs<GridViewItem> e)
        {
            AddItem(e.Item);
        }

        private void item_OnMouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var item = sender as GridViewItem;

            if (Multiselect)
            {
                item.Selected = !item.Selected;
            }
            else
            {
                if (SelectedItem != null) SelectedItem.Selected = false;
                SelectedItem = item;
                if (SelectedItem != null) SelectedItem.Selected = true;

                // if (SelectedItemChanged != null)
                //     SelectedItemChanged(this, SelectedItem);
            }
        }
    }

    internal sealed class GridFrame : Frame
    {
        private Point _clickedPos;
        internal GridColumnHeader Header;
        private Point _oldSize;
        private readonly Button _resizeHandle;

        public GridFrame()
        {
            Scissor = true;
            AutoSize = AutoSize.Vertical;
            Dock = DockStyle.Left;

            _resizeHandle = new Button();
            _resizeHandle.Size = new Point(4, 2);
            _resizeHandle.Dock = DockStyle.Right;
            _resizeHandle.MouseDown += ResizeHandle_OnMouseDown;
            _resizeHandle.MousePress += ResizeHandle_OnMousePress;
            //ResizeHandle.Cursor = Cursors.VSplit;
            _resizeHandle.Style = "button";
            Controls.Add(_resizeHandle);
        }

        private void ResizeHandle_OnMouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            _clickedPos = Gui.MousePosition;
            _oldSize = Size;
        }

        private void ResizeHandle_OnMousePress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var p = Gui.MousePosition - _clickedPos;
            ResizeTo(_oldSize + p, AnchorStyles.Right);
            Header.Size = new Point(Size.X, Header.Size.Y);
        }
    }

    internal sealed class GridColumnHeader : Frame
    {
        private readonly Button _button;
        private Point _clickedPos;
        internal GridColumn Column;
        private Point _oldSize;
        private readonly Button _resizeHandle;

        public GridColumnHeader()
        {
            Scissor = true;
            Dock = DockStyle.Left;

            _resizeHandle = new Button();
            _resizeHandle.Size = new Point(4, 4);
            _resizeHandle.Dock = DockStyle.Right;
            _resizeHandle.MouseDown += ResizeHandle_OnMouseDown;
            _resizeHandle.MousePress += ResizeHandle_OnMousePress;
            //ResizeHandle.Cursor = Cursors.VSplit;
            //ResizeHandle.Style = "button";
            Elements.Add(_resizeHandle);

            _button = new Button();
            _button.Dock = DockStyle.Fill;
            _button.Style = "button";
            Elements.Add(_button);
        }

        public string Text
        {
            get { return _button.Text; }
            set { _button.Text = value; }
        }

        private void ResizeHandle_OnMouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            _clickedPos = Gui.MousePosition;
            _oldSize = Size;
        }

        private void ResizeHandle_OnMousePress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var p = Gui.MousePosition - _clickedPos;
            ResizeTo(_oldSize + p, AnchorStyles.Right);
            Column.Width = Size.X;
        }
    }
}