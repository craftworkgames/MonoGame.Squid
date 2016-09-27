using System;
using System.Collections.Generic;
using MonoGame.Squid.Controls;

namespace MonoGame.Squid.Util
{
    /// <summary>
    /// A collection of controls. This class requires a parent Control.
    /// </summary>
    public class ControlCollection : ActiveList<Control>
    {
        internal bool IsLocked;

        private bool _dirtyAdd;
        private bool _dirtyRemove;

        private readonly List<Control> _toAdd = new List<Control>();

        internal void Cleanup()
        {
            if (_dirtyAdd)
            {
                foreach (var c in _toAdd)
                {
                    base.Add(c);

                    c._isElement = false;
                    c.IsRemoved = false;
                    c.ParentControl = Parent;
                }

                _toAdd.Clear();
                _dirtyAdd = false;
            }

            if (_dirtyRemove)
            {
                var count = Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    if (this[i].IsRemoved)
                    {
                        var child = this[i];
                        this.RemoveAt(i);
                        child.Parent = null;
                    }
                }

                _dirtyRemove = false;
            }
        }

        public Control Parent { get; private set; }

        public ControlCollection(Control parent)
        {
            Parent = parent;

            BeforeItemAdded += Items_BeforeItemAdded;
            BeforeItemRemoved += Items_BeforeItemRemoved;
            BeforeItemsCleared += Items_BeforeItemsCleared;
        }

        void Items_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var control in this)
                control.ParentControl = null;
        }

        void Items_BeforeItemRemoved(object sender, ListEventArgs<Control> e)
        {
            // item is Null or item is our parent
            if (e.Item == null || e.Item == Parent)
            {
                e.Cancel = true;
                return;
            }

            // we dont own this item
            if (e.Item._isElement || e.Item.Parent != Parent)
            {
                e.Cancel = true;
                return;
            }

            e.Item.IsRemoved = true;
            _dirtyRemove = true;

            if (IsLocked)
                e.Cancel = true;
            else
                e.Item.ParentControl = null;
        }

        void Items_BeforeItemAdded(object sender, ListEventArgs<Control> e)
        {
            // item is Null or item is our parent
            if (e.Item == null || e.Item == Parent)
            {
                e.Cancel = true;
                return;
            }

            // we already own this item
            if (e.Item.Parent == Parent)
            {
                e.Cancel = true;
                return;
            }

            if (e.Item.Container != null)
            {
                if (!e.Item.Container.Controls.Remove(e.Item))
                {
                    e.Cancel = true;
                    return;
                }
            }

            e.Item._isElement = false;

            if (IsLocked)
            {
                e.Cancel = true;
                _toAdd.Add(e.Item);
                _dirtyAdd = true;
                return;
            }

            e.Item.IsRemoved = false;
            e.Item.ParentControl = Parent;
        }
    }

    /// <summary>
    /// A collection of elements. This class requires a parent Control.
    /// </summary>
    public class ElementCollection : ActiveList<Control>
    {
        internal bool IsLocked;

        private bool _dirtyAdd;
        private bool _dirtyRemove;

        private readonly List<Control> _toAdd = new List<Control>();

        internal void Cleanup()
        {
            if (_dirtyAdd)
            {
                foreach (var c in _toAdd)
                {
                    base.Add(c);

                    c._isElement = true;
                    c.IsRemoved = false;
                    c.ParentControl = Parent;
                }

                _toAdd.Clear();
                _dirtyAdd = false;
            }

            if (_dirtyRemove)
            {
                var count = Count;
                for (var i = count - 1; i >= 0; i--)
                {
                    if (this[i].IsRemoved)
                    {
                        var child = this[i];
                        this.RemoveAt(i);
                        child.ParentControl = null;
                    }
                }

                _dirtyRemove = false;
            }
        }

        public Control Parent { get; private set; }

        public ElementCollection(Control parent)
        {
            Parent = parent;

            BeforeItemAdded += Items_BeforeItemAdded;
            BeforeItemRemoved += Items_BeforeItemRemoved;
            BeforeItemsCleared += Items_BeforeItemsCleared;
        }

        void Items_BeforeItemsCleared(object sender, EventArgs e)
        {
            foreach (var control in this)
                control.ParentControl = null;
        }

        void Items_BeforeItemRemoved(object sender, ListEventArgs<Control> e)
        {
            // item is Null or item is our parent
            if (e.Item == null || e.Item == Parent)
            {
                e.Cancel = true;
                return;
            }

            // we dont own this item
            if (e.Item.Parent != Parent)
            {
                e.Cancel = true;
                return;
            }

            e.Item.IsRemoved = true;
            _dirtyRemove = true;

            if (IsLocked)
                e.Cancel = true;
            else
                e.Item.ParentControl = null;
        }

        void Items_BeforeItemAdded(object sender, ListEventArgs<Control> e)
        {
            // item is Null or item is our parent
            if (e.Item == null || e.Item == Parent)
            {
                e.Cancel = true;
                return;
            }

            // we already own this item
            if (e.Item.Parent == Parent)
            {
                e.Cancel = true;
                return;
            }

            if (e.Item.Parent != null)
            {
                e.Cancel = true;
                return;
            }

            e.Item._isElement = true;

            if (IsLocked)
            {
                e.Cancel = true;
                _toAdd.Add(e.Item);
                _dirtyAdd = true;
                return;
            }

            e.Item.IsRemoved = false;
            e.Item.ParentControl = Parent;
        }
    }

}
