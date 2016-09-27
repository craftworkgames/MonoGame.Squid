using System;
using System.Collections.Generic;
using Demo.SampleControls.Gridview;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace Demo.SampleControls
{
    public class SampleDesktop : Desktop
    {
        public SampleDesktop()
        {
            var skin = new Skin();

            var baseStyle = new ControlStyle();
            baseStyle.Tiling = TextureMode.Grid;
            baseStyle.Grid = new Margin(3);
            baseStyle.Texture = "button_hot.dds";
            baseStyle.Default.Texture = "button_default.dds";
            baseStyle.Pressed.Texture = "button_down.dds";
            baseStyle.SelectedPressed.Texture = "button_down.dds";
            baseStyle.Focused.Texture = "button_down.dds";
            baseStyle.SelectedFocused.Texture = "button_down.dds";
            baseStyle.Selected.Texture = "button_down.dds";
            baseStyle.SelectedHot.Texture = "button_down.dds";

            var itemStyle = new ControlStyle(baseStyle)
            {
                TextPadding = new Margin(8, 0, 8, 0),
                TextAlign = Alignment.MiddleLeft
            };

            var buttonStyle = new ControlStyle(baseStyle)
            {
                TextPadding = new Margin(0),
                TextAlign = Alignment.MiddleCenter
            };

            var tooltipStyle = new ControlStyle(buttonStyle);
            tooltipStyle.TextPadding = new Margin(8);
            tooltipStyle.TextAlign = Alignment.TopLeft;

            var inputStyle = new ControlStyle();
            inputStyle.Texture = "input_default.dds";
            inputStyle.Hot.Texture = "input_focused.dds";
            inputStyle.Focused.Texture = "input_focused.dds";
            inputStyle.TextPadding = new Margin(8);
            inputStyle.Tiling = TextureMode.Grid;
            inputStyle.Focused.Tint = ColorInt.Rgba(1, 0, 0, 1);
            inputStyle.Grid = new Margin(3);

            var windowStyle = new ControlStyle
            {
                Tiling = TextureMode.Grid,
                Grid = new Margin(9),
                Texture = "window.dds"
            };

            var frameStyle = new ControlStyle();
            frameStyle.Tiling = TextureMode.Grid;
            frameStyle.Grid = new Margin(4);
            frameStyle.Texture = "frame.dds";
            frameStyle.TextPadding = new Margin(8);

            var vscrollTrackStyle = new ControlStyle();
            vscrollTrackStyle.Tiling = TextureMode.Grid;
            vscrollTrackStyle.Grid = new Margin(3);
            vscrollTrackStyle.Texture = "vscroll_track.dds";

            var vscrollButtonStyle = new ControlStyle();
            vscrollButtonStyle.Tiling = TextureMode.Grid;
            vscrollButtonStyle.Grid = new Margin(3);
            vscrollButtonStyle.Texture = "vscroll_button.dds";
            vscrollButtonStyle.Hot.Texture = "vscroll_button_hot.dds";
            vscrollButtonStyle.Pressed.Texture = "vscroll_button_down.dds";

            var vscrollUp = new ControlStyle();
            vscrollUp.Default.Texture = "vscrollUp_default.dds";
            vscrollUp.Hot.Texture = "vscrollUp_hot.dds";
            vscrollUp.Pressed.Texture = "vscrollUp_down.dds";
            vscrollUp.Focused.Texture = "vscrollUp_hot.dds";

            var hscrollTrackStyle = new ControlStyle();
            hscrollTrackStyle.Tiling = TextureMode.Grid;
            hscrollTrackStyle.Grid = new Margin(3);
            hscrollTrackStyle.Texture = "hscroll_track.dds";

            var hscrollButtonStyle = new ControlStyle();
            hscrollButtonStyle.Tiling = TextureMode.Grid;
            hscrollButtonStyle.Grid = new Margin(3);
            hscrollButtonStyle.Texture = "hscroll_button.dds";
            hscrollButtonStyle.Hot.Texture = "hscroll_button_hot.dds";
            hscrollButtonStyle.Pressed.Texture = "hscroll_button_down.dds";

            var hscrollUp = new ControlStyle();
            hscrollUp.Default.Texture = "hscrollUp_default.dds";
            hscrollUp.Hot.Texture = "hscrollUp_hot.dds";
            hscrollUp.Pressed.Texture = "hscrollUp_down.dds";
            hscrollUp.Focused.Texture = "hscrollUp_hot.dds";

            var checkButtonStyle = new ControlStyle();
            checkButtonStyle.Default.Texture = "checkbox_default.dds";
            checkButtonStyle.Hot.Texture = "checkbox_hot.dds";
            checkButtonStyle.Pressed.Texture = "checkbox_down.dds";
            checkButtonStyle.Checked.Texture = "checkbox_checked.dds";
            checkButtonStyle.CheckedFocused.Texture = "checkbox_checked_hot.dds";
            checkButtonStyle.CheckedHot.Texture = "checkbox_checked_hot.dds";
            checkButtonStyle.CheckedPressed.Texture = "checkbox_down.dds";

            var comboLabelStyle = new ControlStyle();
            comboLabelStyle.TextPadding = new Margin(10, 0, 0, 0);
            comboLabelStyle.Default.Texture = "combo_default.dds";
            comboLabelStyle.Hot.Texture = "combo_hot.dds";
            comboLabelStyle.Pressed.Texture = "combo_down.dds";
            comboLabelStyle.Focused.Texture = "combo_hot.dds";
            comboLabelStyle.Tiling = TextureMode.Grid;
            comboLabelStyle.Grid = new Margin(3, 0, 0, 0);

            var comboButtonStyle = new ControlStyle();
            comboButtonStyle.Default.Texture = "combo_button_default.dds";
            comboButtonStyle.Hot.Texture = "combo_button_hot.dds";
            comboButtonStyle.Pressed.Texture = "combo_button_down.dds";
            comboButtonStyle.Focused.Texture = "combo_button_hot.dds";

            var multilineStyle = new ControlStyle();
            multilineStyle.TextAlign = Alignment.TopLeft;
            multilineStyle.TextPadding = new Margin(8);

            var labelStyle = new ControlStyle();
            labelStyle.TextPadding = new Margin(8, 0, 8, 0);
            labelStyle.TextAlign = Alignment.MiddleLeft;
            labelStyle.TextColor = ColorInt.Rgba(.8f, .8f, .8f, 1);
            labelStyle.BackColor = ColorInt.Rgba(1, 1, 1, .125f);
            labelStyle.Default.BackColor = 0;

            skin.Add("item", itemStyle);
            skin.Add("textbox", inputStyle);
            skin.Add("button", buttonStyle);
            skin.Add("window", windowStyle);
            skin.Add("frame", frameStyle);
            skin.Add("checkBox", checkButtonStyle);
            skin.Add("comboLabel", comboLabelStyle);
            skin.Add("comboButton", comboButtonStyle);
            skin.Add("vscrollTrack", vscrollTrackStyle);
            skin.Add("vscrollButton", vscrollButtonStyle);
            skin.Add("vscrollUp", vscrollUp);
            skin.Add("hscrollTrack", hscrollTrackStyle);
            skin.Add("hscrollButton", hscrollButtonStyle);
            skin.Add("hscrollUp", hscrollUp);
            skin.Add("multiline", multilineStyle);
            skin.Add("tooltip", tooltipStyle);
            skin.Add("label", labelStyle);

            Skin = skin;
            //Gui.SetSkin(skin);

            var cursorSize = new Point(32, 32);
            var halfSize = cursorSize/2;

            //skin.Cursors.Add(Cursors.Default, new Cursor { Texture = "cursors\\Arrow.png", Size = cursorSize, HotSpot = Point.Zero });
            //skin.Cursors.Add(Cursors.Link, new Cursor { Texture = "cursors\\Link.png", Size = cursorSize, HotSpot = Point.Zero });
            //skin.Cursors.Add(Cursors.Move, new Cursor { Texture = "cursors\\Move.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.Select, new Cursor { Texture = "cursors\\Select.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.SizeNS, new Cursor { Texture = "cursors\\SizeNS.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.SizeWE, new Cursor { Texture = "cursors\\SizeWE.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.HSplit, new Cursor { Texture = "cursors\\SizeNS.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.VSplit, new Cursor { Texture = "cursors\\SizeWE.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.SizeNESW, new Cursor { Texture = "cursors\\SizeNESW.png", Size = cursorSize, HotSpot = halfSize });
            //skin.Cursors.Add(Cursors.SizeNWSE, new Cursor { Texture = "cursors\\SizeNWSE.png", Size = cursorSize, HotSpot = halfSize });

            TooltipControl = new SimpleTooltip();

            var window1 = new SampleWindow();
            window1.Size = new Point(440, 340);
            window1.Position = new Point(40, 40);
            window1.Titlebar.Text = "Anchoring, [color=FfFfFf00]DropDown, Modal Dialog[/color]";
            window1.Resizable = true;
            window1.Parent = this;

            var label1 = new Label();
            label1.Text = "username:";
            label1.Size = new Point(122, 35);
            label1.Position = new Point(60, 100);
            label1.Parent = window1;
            label1.MousePress += label1_OnMouseDown;

            var textbox1 = new TextBox {Name = "textbox"};
            textbox1.Text = "username";
            textbox1.Size = new Point(222, 35);
            textbox1.Position = new Point(180, 100);
            textbox1.Style = "textbox";
            textbox1.Parent = window1;
            textbox1.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            var label2 = new Label();
            label2.Text = "password:";
            label2.Size = new Point(122, 35);
            label2.Position = new Point(60, 140);
            label2.Parent = window1;

            var textbox2 = new TextBox {Name = "textbox"};
            textbox2.PasswordChar = char.Parse("*");
            textbox2.IsPassword = true;
            textbox2.Text = "password";
            textbox2.Size = new Point(222, 35);
            textbox2.Position = new Point(180, 140);
            textbox2.Style = "textbox";
            textbox2.Parent = window1;
            textbox2.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            var button = new Button();
            button.Size = new Point(157, 35);
            button.Position = new Point(437 - 192, 346 - 52);
            button.Text = "login";
            button.Style = "button";
            button.Parent = window1;
            button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            //button.Cursor = Cursors.Link;
            button.MouseClick += button_OnMouseClick;

            var combo = new DropDownList();
            combo.Size = new Point(222, 35);
            combo.Position = new Point(180, 180);
            combo.Parent = window1;
            combo.Label.Style = "comboLabel";
            combo.Button.Style = "comboButton";
            combo.Listbox.Margin = new Margin(0, 6, 0, 0);
            combo.Listbox.Style = "frame";
            combo.Listbox.ClipFrame.Margin = new Margin(8, 8, 8, 8);
            combo.Listbox.Scrollbar.Margin = new Margin(0, 4, 4, 4);
            combo.Listbox.Scrollbar.Size = new Point(14, 10);
            combo.Listbox.Scrollbar.ButtonUp.Style = "vscrollUp";
            combo.Listbox.Scrollbar.ButtonUp.Size = new Point(10, 20);
            combo.Listbox.Scrollbar.ButtonDown.Style = "vscrollUp";
            combo.Listbox.Scrollbar.ButtonDown.Size = new Point(10, 20);
            combo.Listbox.Scrollbar.Slider.Margin = new Margin(0, 2, 0, 2);
            combo.Listbox.Scrollbar.Slider.Style = "vscrollTrack";
            combo.Listbox.Scrollbar.Slider.Button.Style = "vscrollButton";
            combo.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            for (var i = 0; i < 10; i++)
            {
                var item = new ListBoxItem();
                item.Text = "listboxitem";
                item.Size = new Point(100, 35);
                item.Margin = new Margin(0, 0, 0, 4);
                item.Style = "item";

                //if (i == 3)
                //    item.Selected = true;

                combo.Items.Add(item);

                // if (i == 3)
                //     combo.SelectedItem = item;
                if (i == 3)
                    item.Selected = true;
            }

            var box = new CheckBox();
            box.Size = new Point(157, 26);
            box.Position = new Point(180, 220);
            box.Text = "remember me";
            box.Parent = window1;
            box.Button.Style = "checkBox";
            box.Button.Size = new Point(26, 26);
            //box.Button.Cursor = Cursors.Link;

            var window2 = new SampleWindow();
            window2.Size = new Point(440, 340);
            window2.Position = new Point(500, 40);
            window2.Titlebar.Text = "SplitContainer, TreeView, ListBox";
            window2.Resizable = true;
            window2.Parent = this;

            var split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Parent = window2;
            split.SplitButton.Style = "button";
            split.SplitFrame1.Size = new Point(10, 10);
            split.SplitFrame2.Size = new Point(30, 10);

            var listbox1 = new ListBox();
            listbox1.Margin = new Margin(2);
            listbox1.Dock = DockStyle.Fill;
            listbox1.Scrollbar.Size = new Point(14, 10);
            listbox1.Scrollbar.Slider.Style = "vscrollTrack";
            listbox1.Scrollbar.Slider.Button.Style = "vscrollButton";
            listbox1.Scrollbar.ButtonUp.Style = "vscrollUp";
            listbox1.Scrollbar.ButtonUp.Size = new Point(10, 20);
            listbox1.Scrollbar.ButtonDown.Style = "vscrollUp";
            listbox1.Scrollbar.ButtonDown.Size = new Point(10, 20);
            listbox1.Scrollbar.Slider.Margin = new Margin(0, 2, 0, 2);
            listbox1.Multiselect = true;
            listbox1.MaxSelected = 4;
            listbox1.Parent = split.SplitFrame2;

            for (var i = 0; i < 30; i++)
            {
                var item = new ListBoxItem();
                item.Text = "listboxitem";
                item.Size = new Point(100, 26);
                item.Margin = new Margin(0, 0, 0, 1);
                item.Style = "label";
                item.Tooltip =
                    "This is a multine tooltip.\nThe second line begins here.\n[color=ff00ee55]The third line is even colored.[/color]";
                listbox1.Items.Add(item);
            }

            var treeview = new TreeView();
            treeview.Dock = DockStyle.Fill;
            treeview.Margin = new Margin(2);
            treeview.Parent = split.SplitFrame1;
            treeview.Scrollbar.Size = new Point(14, 10);
            treeview.Scrollbar.Slider.Style = "vscrollTrack";
            treeview.Scrollbar.Slider.Button.Style = "vscrollButton";
            treeview.Scrollbar.ButtonUp.Style = "vscrollUp";
            treeview.Scrollbar.ButtonUp.Size = new Point(10, 20);
            treeview.Scrollbar.ButtonDown.Style = "vscrollUp";
            treeview.Scrollbar.ButtonDown.Size = new Point(10, 20);
            treeview.Scrollbar.Slider.Margin = new Margin(0, 2, 0, 2);
            treeview.Indent = 10;

            for (var i = 0; i < 30; i++)
            {
                var node = new TreeNodeLabel();
                node.Label.Text = "node level 1";
                node.Label.TextAlign = Alignment.MiddleLeft;
                node.Label.Style = "label";
                node.Button.Size = new Point(14, 14);
                node.Size = new Point(100, 26);
                node.Tooltip = node.Label.Text;
                node.Style = "label";
                treeview.Nodes.Add(node);

                for (var i2 = 0; i2 < 3; i2++)
                {
                    var sub1 = new TreeNodeLabel();
                    sub1.Size = new Point(100, 35);
                    sub1.Label.TextAlign = Alignment.MiddleLeft;
                    sub1.Label.Style = "label";
                    sub1.Button.Size = new Point(14, 14);
                    sub1.Size = new Point(100, 26);
                    sub1.Label.Text = "node level 2";
                    sub1.Tooltip = sub1.Label.Text;
                    sub1.Style = "label";
                    node.Nodes.Add(sub1);

                    for (var i3 = 0; i3 < 3; i3++)
                    {
                        var sub2 = new TreeNodeLabel();
                        sub2.Label.Text = "node level 3";
                        sub2.Label.TextAlign = Alignment.MiddleLeft;
                        sub2.Label.Style = "label";
                        sub2.Button.Size = new Point(14, 14);
                        sub2.Size = new Point(100, 26);
                        sub2.Tooltip = sub2.Label.Text;
                        sub2.Style = "label";
                        sub1.Nodes.Add(sub2);
                    }
                }
            }

            var window3 = new SampleWindow();
            window3.Size = new Point(440, 340);
            window3.Position = new Point(40, 400);
            window3.Resizable = true;
            window3.Titlebar.Text = "Custom Control (Inheritance)";
            window3.Parent = this;

            var chatbox = new ChatBox();
            chatbox.Dock = DockStyle.Fill;
            window3.Controls.Add(chatbox);

            chatbox.Style = "frame";
            chatbox.Input.Style = "textbox";
            chatbox.Input.Margin = new Margin(8, 0, 8, 8);
            chatbox.Output.Margin = new Margin(8, 8, 8, 8);
            //chatbox.Output.Style = "textbox";
            chatbox.Scrollbar.Margin = new Margin(0, 8, 8, 8);
            chatbox.Scrollbar.Size = new Point(14, 10);
            chatbox.Scrollbar.Slider.Style = "vscrollTrack";
            chatbox.Scrollbar.Slider.Button.Style = "vscrollButton";
            chatbox.Scrollbar.ButtonUp.Style = "vscrollUp";
            chatbox.Scrollbar.ButtonUp.Size = new Point(10, 20);
            chatbox.Scrollbar.ButtonDown.Style = "vscrollUp";
            chatbox.Scrollbar.ButtonDown.Size = new Point(10, 20);
            chatbox.Scrollbar.Slider.Margin = new Margin(0, 2, 0, 2);

            var window4 = new SampleWindow();
            window4.Size = new Point(440, 340);
            window4.Position = new Point(500, 400);
            window4.Resizable = true;
            window4.Titlebar.Text = "TabControl, TextAlign";
            window4.Parent = this;

            var tabcontrol = new TabControl();
            tabcontrol.ButtonFrame.Style = "item";
            tabcontrol.Dock = DockStyle.Fill;
            tabcontrol.Parent = window4;

            for (var i = 0; i < 6; i++)
            {
                var tabPage = new TabPage();
                //tabPage.Style = "frame";
                tabPage.Margin = new Margin(0, -1, 0, 0);
                tabPage.Button.Style = "button";
                tabPage.Button.Text = "page" + i;
                tabPage.Button.Tooltip = "Click to change active tab";
                tabPage.Button.Margin = new Margin(0, 0, -1, 0);
                tabcontrol.TabPages.Add(tabPage);

                var lbl = new Label();
                lbl.Dock = DockStyle.Fill;
                lbl.Parent = tabPage;
                lbl.TextWrap = true;
                lbl.Text =
                    "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam\r\n nonumy eirmod tempor invidunt ut labore [color=ff0088ff][url=testurl]click \r\n meh![/url][/color] et dolore magna aliquyam erat, sed diam voluptua.\r\n At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
                lbl.Style = "multiline";
                lbl.BbCodeEnabled = true;
                lbl.LinkClicked += lbl_OnLinkClicked;
            }

            var window5 = new SampleWindow();
            window5.Size = new Point(440, 340);
            window5.Position = new Point(960, 40);
            window5.Resizable = true;
            window5.Titlebar.Text = "Panel, TextBox";
            window5.Parent = this;

            var panel = new Panel();
            panel.Style = "frame";
            panel.Dock = DockStyle.Fill;
            panel.Parent = window5;

            panel.ClipFrame.Margin = new Margin(8);
            panel.ClipFrame.Style = "textbox";

            panel.VScroll.Margin = new Margin(0, 8, 8, 8);
            panel.VScroll.Size = new Point(14, 10);
            panel.VScroll.Slider.Style = "vscrollTrack";
            panel.VScroll.Slider.Button.Style = "vscrollButton";
            panel.VScroll.ButtonUp.Style = "vscrollUp";
            panel.VScroll.ButtonUp.Size = new Point(10, 20);
            panel.VScroll.ButtonDown.Style = "vscrollUp";
            panel.VScroll.ButtonDown.Size = new Point(10, 20);
            panel.VScroll.Slider.Margin = new Margin(0, 2, 0, 2);

            panel.HScroll.Margin = new Margin(8, 0, 8, 8);
            panel.HScroll.Size = new Point(10, 14);
            panel.HScroll.Slider.Style = "hscrollTrack";
            panel.HScroll.Slider.Button.Style = "hscrollButton";
            panel.HScroll.ButtonUp.Style = "hscrollUp";
            panel.HScroll.ButtonUp.Size = new Point(20, 10);
            panel.HScroll.ButtonDown.Style = "hscrollUp";
            panel.HScroll.ButtonDown.Size = new Point(20, 10);
            panel.HScroll.Slider.Margin = new Margin(2, 0, 2, 0);

            for (var i = 0; i < 10; i++)
            {
                var label = new Label();
                label.Text = "label control:";
                label.Size = new Point(100, 35);
                label.Position = new Point(10, 10 + 45*i);
                panel.Content.Controls.Add(label);

                var txt = new TextBox();
                txt.Text = "lorem ipsum";
                txt.Size = new Point(222, 35);
                txt.Position = new Point(110, 10 + 45*i);
                txt.Style = "textbox";
                txt.AllowDrop = true;
                txt.TabIndex = 1 + i;
                txt.DragDrop += txt_OnDragDrop;
                txt.GotFocus += txt_OnGotFocus;
                panel.Content.Controls.Add(txt);
            }

            var window6 = new SampleWindow();
            window6.Size = new Point(440, 340);
            window6.Position = new Point(960, 400);
            window6.Resizable = true;
            window6.Titlebar.Text = "Misc";
            window6.Parent = this;

            var rnd = new Random();
            var models = new List<MyData>();
            for (var i = 0; i < 32; i++)
            {
                var data = new MyData();
                data.Name = rnd.Next().ToString();
                data.Date = DateTime.Now.AddMilliseconds(rnd.Next());
                data.Rating = rnd.Next();
                models.Add(data);
            }

            var olv = new ListView();
            olv.Dock = DockStyle.Fill;
            olv.Columns.Add(new ListView.Column {Text = "Name", Aspect = "Name", Width = 120, MinWidth = 48});
            olv.Columns.Add(new ListView.Column {Text = "Date", Aspect = "Date", Width = 120, MinWidth = 48});
            olv.Columns.Add(new ListView.Column {Text = "Rating", Aspect = "Rating", Width = 120, MinWidth = 48});
            olv.StretchLastColumn = true;
            olv.FullRowSelect = true;

            olv.CreateHeader = delegate(object sender, ListView.FormatHeaderEventArgs args)
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
                        olv.Sort<MyData>((a, b) => a.Name.CompareTo(b.Name));
                    else if (args.Column.Aspect == "Date")
                        olv.Sort<MyData>((a, b) => a.Date.CompareTo(b.Date));
                    else if (args.Column.Aspect == "Rating")
                        olv.Sort<MyData>((a, b) => a.Rating.CompareTo(b.Rating));
                };

                header.MouseDrag += delegate(Control snd, MouseEventArgs e)
                {
                    var drag = new Label();
                    drag.Size = snd.Size;
                    drag.Position = snd.Location;
                    drag.Style = snd.Style;
                    drag.Text = ((Button) snd).Text;

                    snd.DoDragDrop(drag);
                };

                header.DragLeave += delegate(Control snd, DragDropEventArgs e) { snd.Tint = -1; };
                header.DragEnter += delegate(Control snd, DragDropEventArgs e)
                {
                    if (e.Source is Button)
                    {
                        snd.Tint = ColorInt.Rgba(0, 1, 0, 1);
                    }
                    else
                    {
                        snd.Tint = ColorInt.Rgba(1, 0, 0, 1);
                        e.Cancel = true;
                    }
                };

                header.DragDrop += delegate(Control snd, DragDropEventArgs e) { snd.Tint = -1; };

                return header;
            };

            olv.CreateCell = delegate(object sender, ListView.FormatCellEventArgs args)
            {
                var text = olv.GetAspectValue(args.Model, args.Column);

                var cell = new Button
                {
                    Size = new Point(26, 26),
                    Style = "label",
                    Dock = DockStyle.Top,
                    Text = text,
                    Tooltip = text,
                    AllowDrop = true
                };

                cell.DragResponse += delegate(Control snd, DragDropEventArgs e) { snd.State = ControlState.Hot; };

                return cell;
            };

            olv.SetObjects(models);

            window6.Controls.Add(olv);
        }

        private void view_ColumnClicked(object sender, GridViewEventArgs e)
        {
            var view = sender as GridView;
            var col = e.Column;

            if (col.Tag == null) col.Tag = 0;

            var tag = (int) col.Tag;
            col.Tag = tag = 1 - tag;

            if (col.Index == 0)
            {
                if (tag == 0)
                    view.Items.Sort((a, b) => a.Text.CompareTo(b.Text));
                else
                    view.Items.Sort((b, a) => a.Text.CompareTo(b.Text));
            }
            else
            {
                if (tag == 0)
                    view.Items.Sort((a, b) => a.SubItems[col.Index - 1].Text.CompareTo(b.SubItems[col.Index - 1].Text));
                else
                    view.Items.Sort((b, a) => a.SubItems[col.Index - 1].Text.CompareTo(b.SubItems[col.Index - 1].Text));
            }
        }

        private void txt_OnGotFocus(Control sender)
        {
            var txt = sender as TextBox;
            txt.Select(0, txt.Text.Length);
        }

        //void item_OnMouseLeave(Control sender)
        //{
        //    sender.Animation.Stop();
        //    sender.Animation.Size(new Point(100, 70), 250);
        //}

        //void item_OnMouseEnter(Control sender)
        //{
        //    sender.Animation.Stop();
        //    sender.Animation.Size(new Point(100, 140), 250);
        //}

        //void b1_OnMouseClick(Control sender, MouseEventArgs args)
        //{
        //    ControlStyle style = Gui.GetStyle("multiline");
        //    style.TextAlign = (Alignment)sender.Tag;
        //}

        private void lbl_OnLinkClicked(string href)
        {
            var dialog = MessageBox.Show(new Point(300, 200), "Message Box", href, MessageBoxButtons.OkCancel, this);
            dialog.OnResult += (s, r) => dialog.Close();
            //dialog.Animation.Custom(WalkSquare(dialog));
        }

        //private System.Collections.IEnumerator WalkSquare(MessageBox dialog)
        //{
        //    yield return dialog.Animation.Position(new Point(10, 10), 1000);
        //    yield return dialog.Animation.Position(new Point(1000, 10), 1000);
        //    yield return dialog.Animation.Position(new Point(1000, 600), 1000);
        //    yield return dialog.Animation.Position(new Point(10, 600), 1000);
        //}

        private void label1_OnMouseDown(Control sender, MouseEventArgs args)
        {
            var btn = new Button();
            btn.Size = new Point(157, 26);
            btn.Text = "drag me";
            btn.Position = sender.Location;
            sender.DoDragDrop(btn);
        }

        private void txt_OnDragDrop(Control sender, DragDropEventArgs e)
        {
            if (e.Source is Label)
                ((TextBox) sender).Text = ((Button) e.DraggedControl).Text;
        }

        private void button_OnMouseClick(Control sender, MouseEventArgs args)
        {
            var dialog = MessageBox.Show(new Point(300, 200), "Message Box", "This is a modal Dialog.",
                MessageBoxButtons.OkCancel, this);
            dialog.OnResult += (s, r) => dialog.Close();
        }

        public class MyData
        {
            public DateTime Date;
            public string Name;
            public int Rating;
        }
    }
}