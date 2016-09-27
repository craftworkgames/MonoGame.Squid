using System;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;

namespace Demo.SampleControls.Gridview
{
    public class GridColumn
    {
        private GridColumnHeader _header;
        private string _text = "Column";
        private int _width = 30;

        internal GridFrame Container;
        public object Tag;

        public GridColumn()
        {
            Container = new GridFrame();
            Header = new GridColumnHeader();
            Header.Style = "button";
            Header.MouseClick += Header_MouseClick;
            Container.Header = Header;
            Header.Column = this;
        }

        public int Index { get; internal set; }

        internal GridColumnHeader Header
        {
            get { return _header; }
            set
            {
                _header = value;

                if (_header != null)
                    _header.Text = Text;
            }
        }

        public int Width
        {
            get { return _width; }
            set
            {
                if (_width == value)
                    return;

                _width = value;

                if (Header != null)
                    Header.Size = new Point(_width, Header.Size.Y);

                if (Container != null)
                    Container.Size = new Point(_width, Container.Size.Y);
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                if (Header != null)
                    Header.Text = value;
            }
        }

        internal event EventHandler Click;

        private void Header_MouseClick(Control sender, MouseEventArgs args)
        {
            if (Click != null) Click(this, null);
        }
    }
}