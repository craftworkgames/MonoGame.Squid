using System;
using System.Collections.Generic;
using System.ComponentModel;
using MonoGame.Squid.Interfaces;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace MonoGame.Squid.Controls
{
    /// <summary>
    /// Delegate SelectedNodeChangedEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="value">The value.</param>
    public delegate void SelectedNodeChangedEventHandler(Control sender, TreeNode value);

    /// <summary>
    /// A TreeView
    /// </summary>
    [Toolbox]
    public class TreeView : Control
    {
        private readonly Frame _itemContainer;
        private TreeNode _selectedNode;

        /// <summary>
        /// Raised when [selected node changed].
        /// </summary>
        public event SelectedNodeChangedEventHandler SelectedNodeChanged;

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
        /// Gets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public ActiveList<TreeNode> Nodes { get; private set; }

        /// <summary>
        /// Gets or sets the selected node.
        /// </summary>
        /// <value>The selected node.</value>
        public TreeNode SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                if (value == _selectedNode) return;

                if (_selectedNode != null) 
                    _selectedNode.Selected = false;

                _selectedNode = value;

                if (_selectedNode != null) 
                    _selectedNode.Selected = true;

                if (SelectedNodeChanged != null)
                    SelectedNodeChanged(this, _selectedNode);
            }
        }

        /// <summary>
        /// Gets or sets the indent.
        /// </summary>
        /// <value>The indent.</value>
        [DefaultValue(0)]
        public int Indent { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeView"/> class.
        /// </summary>
        public TreeView()
        {
            Size = new Point(100, 100);
            Style = "treeview";

            Nodes = new ActiveList<TreeNode>();
            Nodes.ItemAdded += Nodes_ItemAdded;
            Nodes.ItemRemoved += Nodes_ItemRemoved;
            Nodes.BeforeItemsCleared += Nodes_BeforeItemsCleared;

            Scrollbar = new ScrollBar();
            Scrollbar.Dock = DockStyle.Right;
            Scrollbar.Size = new Point(25, 25);
            Scrollbar.Orientation = Orientation.Vertical;
            Elements.Add(Scrollbar);

            ClipFrame = new Frame();
            ClipFrame.Dock = DockStyle.Fill;
            ClipFrame.Scissor = true;
            Elements.Add(ClipFrame);

            _itemContainer = new Frame();
            _itemContainer.AutoSize = AutoSize.Vertical;
            _itemContainer.Parent = ClipFrame;

            MouseWheel += TreeView_MouseWheel;
        }

        void TreeView_MouseWheel(Control sender, MouseEventArgs args)
        {
            Scrollbar.Scroll(Gui.MouseScroll);
            args.Cancel = true;
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

        void Nodes_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var node in Nodes)
                Nodes_ItemRemoved(sender, new ListEventArgs<TreeNode>(node));
        }

        void Nodes_ItemRemoved(object sender, ListEventArgs<TreeNode> e)
        {
            e.Item.ExpandedChanged -=Item_ExpandedChanged;
            e.Item.SelectedChanged -=Item_SelectedChanged;
            e.Item.Treeview = null;

            _itemContainer.Controls.Remove(e.Item);

            foreach (var child in e.Item.Nodes)
                Nodes_ItemRemoved(sender, new ListEventArgs<TreeNode>(child));
        }

        void Item_SelectedChanged(Control sender)
        {
            var node = sender as TreeNode;
            if (node == null) return;

            if (node.Selected)
                SelectedNode = node;
            else if (node == SelectedNode)
                SelectedNode = null;
        }

        void Item_ExpandedChanged(Control sender)
        {
            var node = sender as TreeNode;
           
            if (!node.Expanded)
            {
                var nodes = FindExpandedNodes(node);
                foreach (var child in nodes)
                {
                    child.ExpandedChanged -= Item_ExpandedChanged;
                    child.SelectedChanged -= Item_SelectedChanged;
                    child.Treeview = null;

                    _itemContainer.Controls.Remove(child);
                }
            }
            else
            {
                var i = _itemContainer.Controls.IndexOf(node) + 1;
                var nodes = FindExpandedNodes(node);
                foreach (var child in nodes)
                {
                    child.ExpandedChanged += Item_ExpandedChanged;
                    child.SelectedChanged += Item_SelectedChanged;
                    child.Treeview = this;

                    _itemContainer.Controls.Insert(i, child);
                    i++;
                }
            }
        }

        void Nodes_ItemAdded(object sender, ListEventArgs<TreeNode> e)
        {
            e.Item.NodeDepth = 0;
            e.Item.ExpandedChanged += Item_ExpandedChanged;
            e.Item.SelectedChanged += Item_SelectedChanged;
            e.Item.Treeview = this;

            _itemContainer.Controls.Add(e.Item);
        }

        void item_OnSelect(object sender, EventArgs e)
        {
            var node = sender as TreeNode;

            if (SelectedNode != null) SelectedNode.Selected = false;
            SelectedNode = node;
            if (SelectedNode != null) SelectedNode.Selected = true;

            if (SelectedNodeChanged != null)
                SelectedNodeChanged(this, SelectedNode);
        }

        internal void RemoveNode(TreeNode node)
        {
            _itemContainer.Controls.Remove(node);

            var nodes = FindExpandedNodes(node);
            foreach (var child in nodes)
            {
                child.ExpandedChanged -= Item_ExpandedChanged;
                child.SelectedChanged -= Item_SelectedChanged;
                _itemContainer.Controls.Remove(child);
            }
        }

        private List<TreeNode> FindExpandedNodes(TreeNode parent)
        {
            var list = new List<TreeNode>();

            foreach (var node in parent.Nodes)
            {
                list.Add(node);

                if (node.Expanded)
                    list.AddRange(FindExpandedNodes(node));
            }

            return list;
        }
    }

    /// <summary>
    /// A collection of TreeNodes
    /// </summary>
    public class TreeNodeCollection : ActiveList<TreeNode> { }

    /// <summary>
    /// A TreeNode. Inherit this to create custom nodes.
    /// </summary>
    public class TreeNode : Control, ISelectable
    {
        internal TreeView Treeview;
        private bool _selected;
        private bool _expanded;
        private bool _suspendEvents;

        /// <summary>
        /// Raised when [on selected changed].
        /// </summary>
        public event VoidEvent SelectedChanged;

        /// <summary>
        /// Raised when [on exppanded changed].
        /// </summary>
        public event VoidEvent ExpandedChanged;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TreeNode"/> is selected.
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
        /// Gets or sets a value indicating whether this <see cref="TreeNode"/> is expanded.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        [DefaultValue(false)]
        public bool Expanded
        {
            get { return _expanded; }
            set
            {
                if (value == _expanded) return;
                _expanded = value;

                if (!_suspendEvents)
                {
                    if (ExpandedChanged != null)
                        ExpandedChanged(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value { get; set; }

        /// <summary>
        /// Gets the node depth.
        /// </summary>
        /// <value>The node depth.</value>
        public int NodeDepth { get; internal set; }

        /// <summary>
        /// Gets or sets the nodes.
        /// </summary>
        /// <value>The nodes.</value>
        public TreeNodeCollection Nodes { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public TreeNode Parent { get; private set; }

        public TreeNode()
        {
            Nodes = new TreeNodeCollection();
            Nodes.ItemAdded += Nodes_ItemAdded;
            Nodes.ItemRemoved += Nodes_ItemRemoved;
            Nodes.BeforeItemsCleared += Nodes_BeforeItemsCleared;

            Size = new Point(100, 20);
            Dock = DockStyle.Top;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (Treeview != null && Treeview.Indent != 0)
            {
                var m = Margin;
                Margin = new Margin(Treeview.Indent * NodeDepth, m.Top, m.Right, m.Bottom);
            }
        }

        void Nodes_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var node in Nodes)
            {
                node.Parent = null;

                if (Treeview != null)
                    Treeview.RemoveNode(node);
            }
        }

        void Nodes_ItemRemoved(object sender, ListEventArgs<TreeNode> e)
        {
            if (Treeview != null)
                Treeview.RemoveNode(e.Item);

            e.Item.Parent = null;
        }

        void Nodes_ItemAdded(object sender, ListEventArgs<TreeNode> e)
        {
            e.Item.NodeDepth = NodeDepth + 1;
            e.Item.Parent = this;

            if (Treeview != null && Expanded)
            {
                _suspendEvents = true;
                Expanded = false;
                Expanded = true;
                _suspendEvents = false;
            }
        }

        public void Remove()
        {
            if (Parent != null)
                Parent.Nodes.Remove(this);
            else if (Treeview != null)
            {
                Treeview.Nodes.Remove(this);
            }
        }
    }

    /// <summary>
    /// A TreeNode using a DropDownButton and a Button to expand.
    /// </summary>
    public class TreeNodeDropDown : TreeNode
    {
        /// <summary>
        /// Gets the button.
        /// </summary>
        /// <value>The button.</value>
        public Button Button { get; private set; }
        /// <summary>
        /// Gets the drop down button.
        /// </summary>
        /// <value>The drop down button.</value>
        public DropDownButton DropDownButton { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeDropDown"/> class.
        /// </summary>
        public TreeNodeDropDown()
        {
            Button = new Button();
            Button.Size = new Point(20, 20);
            Button.Margin = new Margin(6);
            Button.Dock = DockStyle.Left;
            Button.MouseClick += Button_MouseClick;
            Elements.Add(Button);

            DropDownButton = new DropDownButton();
            DropDownButton.Size = new Point(20, 20);
            DropDownButton.Dock = DockStyle.Fill;
            Elements.Add(DropDownButton);
        }

        /// <summary>
        /// Button_s the mouse click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        void Button_MouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            Expanded = !Expanded;
        }
    }

    /// <summary>
    /// A TreeNode using a Label and a Button to expand
    /// </summary>
    public class TreeNodeLabel : TreeNode
    {
        /// <summary>
        /// Gets the button.
        /// </summary>
        /// <value>The button.</value>
        public Button Button { get; private set; }
        /// <summary>
        /// Gets the label.
        /// </summary>
        /// <value>The label.</value>
        public Label Label { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNodeLabel"/> class.
        /// </summary>
        public TreeNodeLabel()
        {
            Button = new Button();
            Button.Size = new Point(20, 20);
            Button.Margin = new Margin(6);
            Button.Dock = DockStyle.Left;
            Button.MouseClick += Button_MouseClick;
            Elements.Add(Button);

            Label = new Button();
            Label.Size = new Point(20, 20);
            Label.Dock = DockStyle.Fill;
            Label.MouseClick += Label_MouseClick;
            Label.NoEvents = true;
            Elements.Add(Label);

            MouseClick += Label_MouseClick;
        }

        //protected override void OnStateChanged()
        //{
        //    Label.State = State;
        //}

        void Label_MouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            Selected = true;
        }

        void Button_MouseClick(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            Expanded = !Expanded;
        }
    }
}
