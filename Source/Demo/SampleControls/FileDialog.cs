using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;

namespace Demo.SampleControls
{
    public class PathInfo
    {
        public bool IsDirectory;
        public string Name;
        public long Size;
        public string Type;
    }

    public class FileDialog : Window
    {
        public FileDialog()
        {
            Padding = new Margin(8);
            Resizable = true;

            Split = new SplitContainer();
            Split.Dock = DockStyle.Fill;
            Controls.Add(Split);

            Tree = new TreeView();
            Tree.Dock = DockStyle.Fill;
            Tree.SelectedNodeChanged += Tree_SelectedNodeChanged;
            Tree.Indent = 8;
            Tree.Scrollbar.Size = new Point(14, 10);
            Tree.Scrollbar.Slider.Style = "vscrollTrack";
            Tree.Scrollbar.Slider.Button.Style = "vscrollButton";
            Tree.Scrollbar.ButtonUp.Style = "vscrollUp";
            Tree.Scrollbar.ButtonUp.Size = new Point(10, 20);
            Tree.Scrollbar.ButtonDown.Style = "vscrollUp";
            Tree.Scrollbar.ButtonDown.Size = new Point(10, 20);
            Tree.Scrollbar.Slider.Margin = new Margin(0, 2, 0, 2);
            Split.SplitFrame1.Controls.Add(Tree);

            View = new ListView();
            View.Dock = DockStyle.Fill;
            View.Columns.Add(new ListView.Column {Text = "Name", Aspect = "Name", Width = 120, MinWidth = 64});
            View.Columns.Add(new ListView.Column {Text = "Size", Aspect = "Size", Width = 120, MinWidth = 64});
            View.Columns.Add(new ListView.Column {Text = "Type", Aspect = "Type", Width = 120, MinWidth = 64});
            View.StretchLastColumn = false;
            View.FullRowSelect = true;

            View.CreateHeader = delegate(object sender, ListView.FormatHeaderEventArgs args)
            {
                var header = new Button
                {
                    Dock = DockStyle.Fill,
                    Text = args.Column.Text,
                    AllowDrop = true
                };

                header.MouseClick += delegate
                {
                    if (args.Column.Aspect == "Name")
                        View.Sort<PathInfo>((a, b) => a.Name.CompareTo(b.Name));
                    else if (args.Column.Aspect == "Size")
                        View.Sort<PathInfo>((a, b) => a.Size.CompareTo(b.Size));
                    else if (args.Column.Aspect == "Type")
                        View.Sort<PathInfo>((a, b) => a.Type.CompareTo(b.Type));
                };

                header.MouseDrag += delegate(Control snd, MouseEventArgs e)
                {
                    var drag = new Label();
                    drag.Size = snd.Size;
                    drag.Position = snd.Location;
                    drag.Style = snd.Style;
                    drag.Text = ((Button) snd).Text;

                    DoDragDrop(drag);
                };

                header.DragDrop += delegate { };

                return header;
            };

            View.CreateCell = delegate(object sender, ListView.FormatCellEventArgs args)
            {
                var aspect = View.GetAspectValue(args.Model, args.Column);

                return new Button
                {
                    Scissor = false,
                    Size = new Point(28, 28),
                    Dock = DockStyle.Top,
                    Text = aspect,
                    Tooltip = aspect,
                    TextAlign = Alignment.MiddleLeft,
                    Style = "label",
                    AutoEllipsis = true
                };
            };

            Split.SplitFrame2.Controls.Add(View);
        }

        public SplitContainer Split { get; set; }
        public TreeView Tree { get; set; }
        public ListView View { get; set; }

        private void Tree_SelectedNodeChanged(Control sender, TreeNode value)
        {
            View.SetObjects(null);

            try
            {
                if (value != null)
                {
                    var path = (string) value.Value;

                    var directories = Directory.GetDirectories(path);
                    var files = Directory.GetFiles(path);

                    var objects = new List<PathInfo>();
                    foreach (var dir in directories)
                    {
                        var info = new DirectoryInfo(dir);

                        var i = new PathInfo();
                        i.Name = info.Name;
                        i.Type = "Folder";
                        i.Size = 0;
                        objects.Add(i);
                    }

                    foreach (var file in files)
                    {
                        var info = new FileInfo(file);

                        var i = new PathInfo();
                        i.Name = info.Name;
                        i.Type = info.Extension;
                        i.Size = info.Length;
                        objects.Add(i);
                    }

                    View.SetObjects(objects);
                }
            }
            catch
            {
            }
        }

        public override void Show(Desktop target)
        {
            Tree.Nodes.Clear();

            var dirs = Environment.GetLogicalDrives();

            foreach (var dir in dirs)
            {
                var node = new TreeNodeLabel();
                node.Label.Text = dir;
                node.Label.TextAlign = Alignment.MiddleLeft;
                node.Label.Style = "label";
                node.Button.Size = new Point(14, 14);
                node.Size = new Point(100, 26);
                node.Value = dir;
                node.Style = "label";
                node.ExpandedChanged += node_ExpandedChanged;
                Tree.Nodes.Add(node);
            }

            base.Show(target);
        }

        private void node_ExpandedChanged(Control sender)
        {
            var node = sender as TreeNode;
            if (node.Expanded)
                if (node.Tag == null)
                {
                    node.Tag = true;
                    AddNodes(node);
                }
        }

        private void AddNodes(TreeNode parent)
        {
            var path = (string) parent.Value;
            var dirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);

            foreach (var dir in dirs)
            {
                var node = new TreeNodeLabel();
                node.Label.TextAlign = Alignment.MiddleLeft;
                node.Label.Text = Path.GetFileName(dir);
                node.Label.Style = "label";
                node.Button.Size = new Point(14, 14);
                node.Size = new Point(100, 26);
                node.Value = dir;
                node.Style = "label";
                node.ExpandedChanged += node_ExpandedChanged;
                parent.Nodes.Add(node);
            }
        }
    }
}