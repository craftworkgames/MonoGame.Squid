using System;
using System.ComponentModel;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;
using MonoGame.Squid.Xml;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// A collection of ListBoxItems
    /// </summary>
    public class ListBoxItemCollection : ActiveList<ListBoxItem> { }

    /// <summary>
    /// Delegate SelectedItemChangedEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="value">The value.</param>
    public delegate void SelectedItemChangedEventHandler(Control sender, ListBoxItem value);
    /// <summary>
    /// Delegate SelectedItemsChangedEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    public delegate void SelectedItemsChangedEventHandler(Control sender);

    /// <summary>
    /// A ListBox
    /// </summary>
    [Toolbox]
    public class ListBox : Control
    {
        private bool _skipEvents;
        private readonly Frame _itemContainer;
        private ListBoxItem _selectedItem;
        private readonly ActiveList<ListBoxItem> _selected = new ActiveList<ListBoxItem>();

        /// <summary>
        /// Raised when [selected item changed].
        /// </summary>
        public event SelectedItemChangedEventHandler SelectedItemChanged;

        /// <summary>
        /// Raised when [selected items changed].
        /// </summary>
        public event SelectedItemsChangedEventHandler SelectedItemsChanged;

        /// <summary>
        /// Gets the scrollbar.
        /// </summary>
        /// <value>The scrollbar.</value>
        public ScrollBar Scrollbar { get; private set; }

        /// <summary>
        /// Gets the clip frame.
        /// </summary>
        /// <value>The clip frame.</value>
        public Frame ClipFrame { get; private set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public ListBoxItemCollection Items { get; set; }

        /// <summary>
        /// Gets or sets the scroll.
        /// </summary>
        /// <value>The scroll.</value>
        [Hidden]
        public float Scroll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ListBox"/> is multiselect.
        /// </summary>
        /// <value><c>true</c> if multiselect; otherwise, <c>false</c>.</value>
        public bool Multiselect { get; set; }

        /// <summary>
        /// Gets or sets the max selected.
        /// </summary>
        /// <value>The max selected.</value>
        public int MaxSelected { get; set; }

        /// <summary>
        /// Gets or sets wether to scroll with the mouse wheel
        /// </summary>
        public bool MouseScroll { get; set; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        [XmlIgnore]
        public ListBoxItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;

                _skipEvents = true;

                if (_selectedItem != null)
                    _selectedItem.Selected = false;

                _skipEvents = false;

                if (Multiselect)
                    _selected.Clear();

                _selectedItem = value;

                _skipEvents = true;

                if (_selectedItem != null)
                    _selectedItem.Selected = true;

                _skipEvents = false;

                if (SelectedItemChanged != null)
                    SelectedItemChanged(this, _selectedItem);
            }
        }

        /// <summary>
        /// Gets the selected items.
        /// </summary>
        /// <value>The selected items.</value>
        [XmlIgnore]
        public ActiveList<ListBoxItem> SelectedItems
        {
            get { return _selected; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        public ListBox()
        {
            MouseScroll = true;
            MaxSelected = 0;
            Scroll = 1;
            Size = new Point(100, 100);
            Style = "listbox";

            Items = new ListBoxItemCollection();
            Items.BeforeItemAdded += Items_BeforeItemAdded;
            Items.BeforeItemsCleared += Items_BeforeItemsCleared;
            Items.ItemAdded += Items_ItemAdded;
            Items.ItemRemoved += Items_ItemRemoved;
            Items.ItemsSorted += Items_ItemsSorted;

            Scrollbar = new ScrollBar();
            Scrollbar.Dock = DockStyle.Right;
            Scrollbar.Size = new Point(25, 25);
            Elements.Add(Scrollbar);

            ClipFrame = new Frame();
            ClipFrame.Dock = DockStyle.Fill;
            ClipFrame.Scissor = true;
            Elements.Add(ClipFrame);

            _itemContainer = new Frame();
            _itemContainer.AutoSize = AutoSize.Vertical;
            ClipFrame.Controls.Add(_itemContainer);

            _selected.BeforeItemAdded += _selected_BeforeItemAdded;
            _selected.ItemAdded += _selected_ItemAdded;
            _selected.ItemRemoved += _selected_ItemRemoved;
            _selected.BeforeItemsCleared += _selected_BeforeItemsCleared;

            MouseWheel += ListBox_MouseWheel;
        }

        void ListBox_MouseWheel(Control sender, MouseEventArgs args)
        {
            if (MouseScroll)
            {
                Scrollbar.Scroll(Gui.MouseScroll);
                args.Cancel = true;
            }
        }

        void _selected_BeforeItemsCleared(object sender, EventArgs e)
        {
            _skipEvents = true;

            foreach (var item in _selected)
                item.Selected = false;

            if (SelectedItemsChanged != null)
                SelectedItemsChanged(this);

            _skipEvents = false;
        }

        void _selected_BeforeItemAdded(object sender, ListEventArgs<ListBoxItem> e)
        {
            if (!Items.Contains(e.Item)) e.Cancel = true;
        }

        void _selected_ItemRemoved(object sender, ListEventArgs<ListBoxItem> e)
        {
            if (e.Item == null) return;

            _skipEvents = true;
            e.Item.Selected = false;
            _skipEvents = false;

            if (SelectedItemsChanged != null)
                SelectedItemsChanged(this);
        }

        void _selected_ItemAdded(object sender, ListEventArgs<ListBoxItem> e)
        {
            if (e.Item == null) return;

            _skipEvents = true;
            e.Item.Selected = true;
            _skipEvents = false;

            if (SelectedItemsChanged != null)
                SelectedItemsChanged(this);
        }

        protected override void OnUpdate()
        {
            // force the width to be that of its parent
            _itemContainer.Size = new Point(ClipFrame.Size.X, _itemContainer.Size.Y);

            // move the label up/down using the scrollbar value
            if (_itemContainer.Size.Y < ClipFrame.Size.Y) // no need to scroll
            {
                Scrollbar.Visible = false; // hide scrollbar
                _itemContainer.Position = new Point(0, 0); // set fixed position
            }
            else
            {
                Scrollbar.Scale = Math.Min(1, (float)Size.Y / (float)_itemContainer.Size.Y);
                Scrollbar.Visible = true; // show scrollbar
                _itemContainer.Position = new Point(0, (int)((ClipFrame.Size.Y - _itemContainer.Size.Y) * Scrollbar.EasedValue));
            }

            if (Scrollbar.ShowAlways)
                Scrollbar.Visible = true;
        }

        void Items_BeforeItemAdded(object sender, ListEventArgs<ListBoxItem> e)
        {
            if (Items.Contains(e.Item)) e.Cancel = true;
        }

        void Items_ItemsSorted(object sender, EventArgs e)
        {
            _itemContainer.Controls.Clear();

            foreach (var item in Items)
                _itemContainer.Controls.Add(item);
        }

        void Items_ItemRemoved(object sender, ListEventArgs<ListBoxItem> e)
        {
            _itemContainer.Controls.Clear();

            if (e.Item.Selected)
            {
                if (Multiselect)
                    _selected.Remove(e.Item);
                else
                    SelectedItem = null;
            }

            e.Item.Selected = false;
            e.Item.MouseClick -= item_MouseClick;
            e.Item.SelectedChanged -= Item_SelectedChanged;

            foreach (var item in Items)
                _itemContainer.Controls.Add(item);
        }

        void Items_ItemAdded(object sender, ListEventArgs<ListBoxItem> e)
        {
            _itemContainer.Controls.Clear();

            if (e.Item.Selected)
            {
                if (Multiselect)
                    _selected.Add(e.Item);
                else
                    SelectedItem = e.Item;
            }

            e.Item.MouseClick += item_MouseClick;
            e.Item.SelectedChanged += Item_SelectedChanged;

            foreach (var item in Items)
                _itemContainer.Controls.Add(item);
        }

        void Item_SelectedChanged(Control sender)
        {
            var item = sender as ListBoxItem;

            if (_skipEvents) return;

            if (item.Selected)
            {
                if (Multiselect)
                    _selected.Add(item);
                else
                    SelectedItem = item;
            }
            else
            {
                if (Multiselect)
                    _selected.Remove(item);
                else
                    SelectedItem = null;
            }
        }

        void Items_BeforeItemsCleared(object sender, EventArgs e)
        {
            _skipEvents = true;
            foreach (var item in Items)
            {
                item.MouseClick -= item_MouseClick;
                item.SelectedChanged -= Item_SelectedChanged;
                item.Selected = false;
            }
            _skipEvents = false;

            _itemContainer.Controls.Clear();
        }

        void item_MouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var item = sender as ListBoxItem;

            if (Multiselect)
            {
                if (MaxSelected > 0)
                {
                    if (!item.Selected)
                    {
                        if (_selected.Count < MaxSelected)
                            item.Selected = true;
                    }
                    else
                        item.Selected = false;
                }
                else
                {
                    item.Selected = !item.Selected;
                }
            }
            else
            {
                SelectedItem = item;
            }
        }
    }

    /// <summary>
    /// A ListBoxItem. Inherit this to create custom items.
    /// </summary>
    public class ListBoxItem : Button, ISelectable
    {
        private bool _selected;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; set; }

        /// <summary>
        /// Raised when [selected changed].
        /// </summary>
        public event VoidEvent SelectedChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Label" /> is selected.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value == _selected) return;

                _selected = value;

                if (SelectedChanged != null)
                    SelectedChanged(this);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxItem"/> class.
        /// </summary>
        public ListBoxItem()
        {
            Size = new Point(100, 20);
            Dock = DockStyle.Top;
        }
    }
}
