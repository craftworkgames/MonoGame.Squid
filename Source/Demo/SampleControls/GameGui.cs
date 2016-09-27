using System;
using System.Collections.Generic;
using MonoGame.Squid;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Skinning;
using MonoGame.Squid.Structs;
using MonoGame.Squid.Util;

namespace Demo.SampleControls
{
    public class GameGui : Desktop
    {
        public ActionBar Actionbar1;
        public ActionBar Actionbar2;
        private Frame _bottom;
        public CharacterSheet Charsheet;
        public ChatWindow Chatwindow;
        public Inventory Inventory;
        public MiniMap Minimap;
        public Skillbook Skillbook;
        public StatusBar Statusbar;

        private Frame _top;

        public GameGui()
        {
            CreateSkin();
            CreateGui();
        }

        private void CreateSkin()
        {
            var baseStyle = new ControlStyle();
            baseStyle.Tiling = TextureMode.Grid;
            baseStyle.Grid = new Margin(3);
            baseStyle.Texture = "button_default.dds";
            baseStyle.Hot.Texture = "button_hot.dds";
            baseStyle.Default.Texture = "button_default.dds";
            baseStyle.Pressed.Texture = "button_down.dds";
            baseStyle.Focused.Texture = "button_hot.dds";
            baseStyle.SelectedPressed.Texture = "button_down.dds";
            baseStyle.SelectedFocused.Texture = "button_hot.dds";
            baseStyle.Selected.Texture = "button_hot.dds";
            baseStyle.SelectedHot.Texture = "button_hot.dds";
            baseStyle.CheckedPressed.Texture = "button_down.dds";
            baseStyle.CheckedFocused.Texture = "button_down.dds";
            baseStyle.Checked.Texture = "button_down.dds";
            baseStyle.CheckedHot.Texture = "button_down.dds";

            var itemStyle = new ControlStyle(baseStyle);
            itemStyle.TextPadding = new Margin(10, 0, 0, 0);
            itemStyle.TextAlign = Alignment.MiddleLeft;

            var buttonStyle = new ControlStyle(baseStyle);
            buttonStyle.TextPadding = new Margin(0);
            buttonStyle.TextAlign = Alignment.MiddleCenter;

            var tooltipStyle = new ControlStyle(buttonStyle);
            tooltipStyle.TextPadding = new Margin(8);
            tooltipStyle.TextAlign = Alignment.TopLeft;
            tooltipStyle.Texture = "border.dds";
            tooltipStyle.Tiling = TextureMode.Grid;
            tooltipStyle.Grid = new Margin(2);
            tooltipStyle.BackColor = ColorInt.Rgba(0, 0, 0, .9f);

            var inputStyle = new ControlStyle();
            inputStyle.Texture = "input_default.dds";
            inputStyle.Hot.Texture = "input_focused.dds";
            inputStyle.Focused.Texture = "input_focused.dds";
            inputStyle.TextPadding = new Margin(8);
            inputStyle.Tiling = TextureMode.Grid;
            inputStyle.Focused.Tint = ColorInt.Rgba(1, 0, 0, 1);
            inputStyle.Grid = new Margin(3);

            var windowStyle = new ControlStyle();
            windowStyle.Tiling = TextureMode.Grid;
            windowStyle.Grid = new Margin(12);
            windowStyle.Texture = "window.dds";
            windowStyle.BackColor = ColorInt.Rgba(0, 0, 0, .9f);

            var frameStyle = new ControlStyle();
            frameStyle.Tiling = TextureMode.Grid;
            frameStyle.Grid = new Margin(2);
            frameStyle.Texture = "frame.dds";
            frameStyle.TextPadding = new Margin(8);

            var vscrollTrackStyle = new ControlStyle();
            vscrollTrackStyle.Tiling = TextureMode.Grid;
            vscrollTrackStyle.Grid = new Margin(3);
            vscrollTrackStyle.Texture = "vscroll_track.dds";

            var vscrollButtonStyle = new ControlStyle();
            vscrollButtonStyle.Tiling = TextureMode.Grid;
            vscrollButtonStyle.Grid = new Margin(4);
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

            var borderStyle = new ControlStyle();
            borderStyle.Hot.Texture = "border.dds";
            borderStyle.Pressed.Texture = "border.dds";
            borderStyle.Tiling = TextureMode.Grid;
            borderStyle.Grid = new Margin(4);

            var labelStyle = new ControlStyle();
            labelStyle.TextAlign = Alignment.TopLeft;
            labelStyle.TextPadding = new Margin(8);

            var handleNw = new ControlStyle();
            handleNw.Texture = "handleNW.dds";

            var handleNe = new ControlStyle();
            handleNe.Texture = "handleNE.dds";

            labelStyle.TextPadding = new Margin(8);

            var skin = new Skin();

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
            skin.Add("multiline", labelStyle);
            skin.Add("tooltip", tooltipStyle);
            skin.Add("border", borderStyle);
            skin.Add("handleNE", handleNe);
            skin.Add("handleNW", handleNw);

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
        }

        private void CreateGui()
        {
            TooltipControl = new SimpleTooltip();
            TooltipControl.Offset = new Point(50, 50);

            _top = new Frame();
            _top.Size = new Point(24, 24);
            _top.Dock = DockStyle.Top;
            _top.Style = "button";
            Desktop.Controls.Add(_top);

            _bottom = new Frame();
            _bottom.Size = new Point(14, 14);
            _bottom.Dock = DockStyle.Bottom;
            _bottom.Style = "button";
            Desktop.Controls.Add(_bottom);

            var center = new Frame();
            center.Dock = DockStyle.Fill;
            Desktop.Controls.Add(center);

            Actionbar1 = new ActionBar();
            Actionbar2 = new ActionBar();
            Statusbar = new StatusBar();
            Minimap = new MiniMap();
            Inventory = new Inventory();
            Skillbook = new Skillbook();
            Chatwindow = new ChatWindow();
            Charsheet = new CharacterSheet();

            center.Controls.Add(Actionbar1);
            center.Controls.Add(Actionbar2);
            center.Controls.Add(Statusbar);

            Chatwindow.Show(Desktop);
            Minimap.Show(Desktop);
            Inventory.Show(Desktop);
            Charsheet.Show(Desktop);
            Skillbook.Show(Desktop);
        }
    }

    public class StatusBar : Control
    {
        private readonly Frame _frame;

        public StatusBar()
        {
            Size = new Point(44, 20);
            Dock = DockStyle.Bottom;

            _frame = new Frame();
            _frame.Dock = DockStyle.Center;
            _frame.AutoSize = AutoSize.Horizontal;
            Elements.Add(_frame);

            AddBar();
            AddBar();
            AddBar();
        }

        private void AddBar()
        {
            var bar1 = new Frame();
            bar1.Size = new Point(110, 10);
            bar1.Style = "window";
            bar1.Dock = DockStyle.Left;
            bar1.Margin = new Margin(1);
            _frame.Controls.Add(bar1);
        }

        private void button_DragDrop(Control sender, DragDropEventArgs e)
        {
            var slot = sender as ActionButton;

            slot.Item = e.DraggedControl.Tag as GuiItem;
        }
    }

    public class Character
    {
        public List<Item> Items = new List<Item>();
        public CharacterItemSlots ItemSlots = new CharacterItemSlots();
        public List<Skill> Skills = new List<Skill>();

        public Character()
        {
            foreach (ItemSlotType type in Enum.GetValues(typeof(ItemSlotType)))
                if (type != ItemSlotType.None)
                    ItemSlots.Add(type, new ItemSlot {Type = type});
        }

        public bool Equip(Item item)
        {
            if (item != null)
            {
                if (item.Equipped) return false;
                if (!ItemSlots.ContainsKey(item.SlotType)) return false;
            }

            var slot = ItemSlots[item.SlotType];
            slot.Item = item;

            return true;
        }

        public bool Equip(Item item, ItemSlot slot)
        {
            if (slot == null) return false;

            if (item != null)
                if (item.SlotType != slot.Type) return false;

            slot.Item = item;

            return true;
        }
    }

    public class CharacterItemSlots : Dictionary<ItemSlotType, ItemSlot>
    {
    }

    public class ItemSlot
    {
        private Item _item;

        public ItemSlotType Type { get; set; }

        public Item Item
        {
            get { return _item; }

            set
            {
                if (value != null)
                    if (value.SlotType != Type) return;

                if (_item != null)
                    _item.Equipped = false;

                _item = value;

                if (_item != null)
                    _item.Equipped = true;
            }
        }
    }

    public enum ItemSlotType
    {
        None,
        Head,
        Shoulders,
        Back,
        Chest,
        Waist,
        Legs,
        Feet,
        Arms,
        Hands,
        Neck,
        Finger,
        MainHand,
        OffHand
    }

    public static class GameLogic
    {
        public static Character Player;

        static GameLogic()
        {
            Player = new Character();

            var rnd = new Random();
            var icons = new List<Icon>();

            for (var y = 0; y < 10; y++)
                for (var x = 0; x < 10; x++)
                {
                    var icon = new Icon();
                    icon.Texture = "weapon-icons.png";
                    icon.Rect = new Rectangle(2 + x*60, 2 + y*60, 56, 56);
                    icons.Add(icon);
                }

            for (var i = 0; i < 32; i++)
            {
                var slot = rnd.Next(14);

                var tooltip = "" +
                              "[color=ffffff00]" + "Scorching Waraxe of Absorbtion" + "[/color]" + Environment.NewLine +
                              (ItemSlotType) slot + Environment.NewLine +
                              "\r\n" +
                              "Strength: " + "[color=ff00ff00]" + rnd.Next(60) + "[/color]" + Environment.NewLine +
                              "Vitality: " + "[color=ff00ff00]" + rnd.Next(60) + "[/color]" + Environment.NewLine +
                              "Wisdom: " + "[color=ff00ff00]" + rnd.Next(60) + "[/color]" + Environment.NewLine +
                              "Critical Hit: " + "[color=ff00ff00]" + rnd.Next(60) + "[/color]" + Environment.NewLine +
                              "\r\n" +
                              "[color=ff0066ff]" + "4% Life Steal" + "[/color]" + Environment.NewLine +
                              "[color=ff0066ff]" + "25% Chance to fart on hit" + "[/color]" + Environment.NewLine +
                              "[color=ff0066ff]" + "11% Chance to cast Fireburst on attack" + "[/color]" +
                              Environment.NewLine +
                              "[color=ff0066ff]" + "Cannot be used when drunk" + "[/color]";

                Player.Items.Add(new Item
                {
                    Name = "item",
                    SlotType = (ItemSlotType) slot,
                    Icon = icons[rnd.Next(icons.Count)],
                    Count = rnd.Next(50),
                    Tooltip = tooltip
                });
            }

            for (var i = 0; i < 32; i++)
            {
                var tooltip = "Name" + Environment.NewLine +
                              "Cost: " + "[color=ffff00ff]" + rnd.Next(100) + "[/color]" + Environment.NewLine +
                              "Cooldown: " + "[color=ff00ff00]" + rnd.Next(60) + "[/color] sec" + Environment.NewLine +
                              "\nHateful Saint's Abjuration of the Vorticies of Secrets\n" +
                              "Blind Spectre's Communion of the Ancient Edge";

                Player.Skills.Add(new Skill
                {
                    Name = "skill",
                    Icon = icons[rnd.Next(icons.Count)],
                    Tooltip = tooltip
                });
            }
        }
    }

    public class GuiItem
    {
        public int Count;
        public Icon Icon;
        public string Name;
        public string Tooltip;
    }

    public class Icon
    {
        public string Name;
        public Rectangle Rect;
        public string Texture;
    }

    public class Skill : GuiItem
    {
    }

    public class Item : GuiItem
    {
        public bool Equipped;
        public ItemSlotType SlotType;
        public bool Stackable;
    }

    public class ActionButton : Control
    {
        private GuiItem _item;
        private readonly Button _button;
        private readonly Label _counter;
        private readonly ImageControl _image;

        public GuiItem Item;

        public ActionButton()
        {
            _image = new ImageControl();
            _image.Dock = DockStyle.Fill;
            _image.NoEvents = true;
            _image.Texture = "border.dds";
            _image.TextureRect = new Rectangle(0, 0, 64, 64);
            Elements.Add(_image);

            _button = new Button();
            _button.Dock = DockStyle.Fill;
            _button.Style = "border";
            _button.NoEvents = true;
            Elements.Add(_button);

            _counter = new Label();
            _counter.Dock = DockStyle.Top;
            _counter.Size = new Point(16, 16);
            _counter.TextAlign = Alignment.MiddleRight;
            _counter.NoEvents = true;
            Elements.Add(_counter);
        }

        public ActionButton(GuiItem item)
            : this()
        {
            Item = item;
        }

        protected override void OnStateChanged()
        {
            base.OnStateChanged();
            _button.State = State;
        }

        protected override void OnUpdate()
        {
            if (Item != null)
            {
                Tooltip = Item.Tooltip;
                _image.Texture = Item.Icon.Texture;
                _image.TextureRect = Item.Icon.Rect;
                _counter.Visible = Item.Count > 0;
                _counter.Text = Item.Count.ToString();
            }

            base.OnUpdate();
        }

        protected override void DrawStyle(Style style, float opacity)
        {
        }

        protected override void DrawText(Style style, float opacity)
        {
        }
    }

    public class Inventory : SampleWindow
    {
        private readonly Frame _bottom;
        private readonly FlowLayoutFrame _flow;
        private readonly Frame _left;
        private readonly TextBox _search;
        private readonly Frame _top;
        private readonly ScrollView _view;

        public Inventory()
        {
            Resizable = true;
            Size = new Point(400, 300);
            Position = new Point(400, 200);
            Titlebar.Text = "Inventory";

            _left = new Frame();
            _left.Size = new Point(50, 50);
            _left.Dock = DockStyle.Left;
            _left.Style = "window";
            Controls.Add(_left);

            for (var i = 0; i < 6; i++)
            {
                var slot = new ActionButton();
                slot.Size = new Point(40, 40);
                slot.Dock = DockStyle.Top;
                slot.Margin = new Margin(5, 5, 5, 0);
                _left.Controls.Add(slot);
            }

            _top = new Frame();
            _top.Size = new Point(58, 58);
            _top.Dock = DockStyle.Top;
            Controls.Add(_top);

            _search = new TextBox();
            _search.Size = new Point(200, 28);
            _search.Dock = DockStyle.Bottom;
            _search.Style = "textbox";
            _search.Text = "Search...";
            _search.Margin = new Margin(8, 0, 8, 0);
            _search.GotFocus += delegate { _search.Text = ""; };
            _search.LostFocus += delegate { _search.Text = "Search..."; };
            _top.Controls.Add(_search);

            _bottom = new Frame();
            _bottom.Size = new Point(38, 38);
            _bottom.Dock = DockStyle.Bottom;
            _bottom.Style = "border";
            _bottom.Margin = new Margin(-1, 0, 0, 0);
            Controls.Add(_bottom);

            _view = new ScrollView();
            _view.Dock = DockStyle.Fill;
            _view.AllowDrop = true;
            Controls.Add(_view);

            _flow = new FlowLayoutFrame();
            _flow.FlowDirection = FlowDirection.LeftToRight;
            _flow.HSpacing = _flow.VSpacing = 4;
            _flow.AutoSize = AutoSize.Vertical;
            _flow.Dock = DockStyle.Top;
            _view.Controls.Add(_flow);

            Refresh();
        }

        private void Refresh()
        {
            _flow.Controls.Clear();

            foreach (var item in GameLogic.Player.Items)
            {
                if (item.Equipped) continue;

                var button = new ActionButton(item);
                button.Size = new Point(40, 40);
                button.Style = "button";
                button.MouseDrag += button_MouseDrag;

                _flow.Controls.Add(button);
            }
        }

        private void button_MouseDrag(Control sender, MouseEventArgs args)
        {
            var slot = sender as ActionButton;

            var image = new ImageControl();
            image.Texture = slot.Item.Icon.Texture;
            image.TextureRect = slot.Item.Icon.Rect;
            image.Size = new Point(40, 40);
            image.Position = Gui.MousePosition;
            image.Tag = slot.Item;

            DoDragDrop(image);
        }
    }

    public class ScrollView : Control
    {
        public Frame Content;
        public Panel Panel;

        public ScrollView()
        {
            Dock = DockStyle.Fill;
            Margin = new Margin(2);

            Panel = new Panel();
            Panel.Dock = DockStyle.Fill;
            Panel.VScroll.ButtonUp.Visible = false;
            Panel.VScroll.ButtonDown.Visible = false;
            Panel.VScroll.Size = new Point(13, 12);
            Panel.VScroll.Slider.Style = "window";
            Panel.VScroll.Slider.Button.Style = "button";
            Panel.VScroll.Dock = DockStyle.Right;
            Panel.VScroll.Margin = new Margin(4, 0, 0, 0);
            Panel.HScroll.Size = new Point(0, 0);
            Elements.Add(Panel);

            Content = new Frame();
            Content.AutoSize = AutoSize.Vertical;
            Content.Dock = DockStyle.Top;
            Panel.Content.Controls.Add(Content);
        }

        public ControlCollection Controls
        {
            get { return Content.Controls; }
        }

        public void Scroll(int value)
        {
            Panel.VScroll.Value = value;
        }

        protected override void OnUpdate()
        {
            if ((Gui.MouseScroll != 0) && (Desktop.HotControl != null))
                if (Hit(Gui.MousePosition.X, Gui.MousePosition.Y))
                    if ((Desktop.HotControl == this) || Desktop.HotControl.IsChildOf(this))
                    {
                        Panel.VScroll.MouseScrollSpeed = 64f/(Panel.Content.Size.Y - Panel.ClipFrame.Size.Y);
                        Panel.VScroll.Scroll(Gui.MouseScroll);
                    }

            base.OnUpdate();
        }
    }

    public class Skillbook : SampleWindow
    {
        private readonly FlowLayoutFrame _flow;
        private TextBox _searchfield;
        private readonly ScrollView _view;

        public Skillbook()
        {
            Resizable = true;
            Size = new Point(400, 300);
            Position = new Point(400, 200);
            Titlebar.Text = "Skills";

            _view = new ScrollView();
            _view.Dock = DockStyle.Fill;
            Controls.Add(_view);

            _flow = new FlowLayoutFrame();
            _flow.FlowDirection = FlowDirection.LeftToRight;
            _flow.HSpacing = _flow.VSpacing = 4;
            _flow.AutoSize = AutoSize.Vertical;
            _flow.Dock = DockStyle.Top;
            _view.Controls.Add(_flow);

            Refresh();
        }

        private void Refresh()
        {
            _flow.Controls.Clear();

            foreach (var item in GameLogic.Player.Skills)
            {
                var button = new ActionButton(item);
                button.Size = new Point(40, 40);
                button.Style = "button";
                button.MouseDrag += button_MouseDrag;

                _flow.Controls.Add(button);
            }
        }

        private void button_MouseDrag(Control sender, MouseEventArgs args)
        {
            var slot = sender as ActionButton;

            var image = new ImageControl();
            image.Texture = slot.Item.Icon.Texture;
            image.TextureRect = slot.Item.Icon.Rect;
            image.Size = new Point(40, 40);
            image.Position = sender.Location;
            image.Tag = slot.Item;

            DoDragDrop(image);
        }
    }

    public class CharacterSheet : SampleWindow
    {
        private Frame _bottom;
        private readonly Frame _left;
        private readonly Frame _paperdoll;
        private readonly Frame _right;
        private readonly SplitContainer _split;
        private readonly ScrollView _stats;

        private Frame _top;

        public CharacterSheet()
        {
            Resizable = true;
            Size = new Point(400, 300);
            Position = new Point(100, 100);
            Titlebar.Text = "Character";

            _split = new SplitContainer();
            _split.Dock = DockStyle.Fill;
            Controls.Add(_split);

            _stats = new ScrollView();
            _stats.Dock = DockStyle.Fill;
            _split.SplitFrame1.Controls.Add(_stats);

            _paperdoll = new Frame();
            _paperdoll.Dock = DockStyle.Fill;
            _split.SplitFrame2.Controls.Add(_paperdoll);

            var image = new ImageControl {Texture = "paperdoll.jpg"};
            image.Dock = DockStyle.Fill;
            _paperdoll.Controls.Add(image);

            //top = new Frame { Size = new Point(44, 44), Dock = DockStyle.Top }; paperdoll.Controls.Add(top);
            //bottom = new Frame { Size = new Point(44, 44), Dock = DockStyle.Bottom }; paperdoll.Controls.Add(bottom);
            _left = new Frame {Size = new Point(44, 44), Dock = DockStyle.Left};
            _paperdoll.Controls.Add(_left);
            _right = new Frame {Size = new Point(44, 44), Dock = DockStyle.Right};
            _paperdoll.Controls.Add(_right);

            var i = 0;
            foreach (var pair in GameLogic.Player.ItemSlots)
            {
                var button = new ActionButton();
                button.Size = new Point(40, 40);
                button.Style = "button";
                button.Dock = DockStyle.Top;
                button.Margin = new Margin(2);
                button.AllowDrop = true;
                button.DragDrop += button_DragDrop;
                button.Tooltip = pair.Key.ToString();
                button.Tag = pair.Value;

                if (i < 6)
                    _left.Controls.Add(button);
                else
                    _right.Controls.Add(button);

                i++;
            }
        }

        public void Hightlight(ItemSlotType type)
        {
        }

        private void button_DragDrop(Control sender, DragDropEventArgs e)
        {
            var slot = sender as ActionButton;

            if (e.DraggedControl.Tag is Item)
            {
                var item = e.DraggedControl.Tag as Item;
                var itemSlot = slot.Tag as ItemSlot;

                if (GameLogic.Player.Equip(item, itemSlot))
                    slot.Item = item;
            }
        }
    }

    public class ActionBar : Frame
    {
        public ActionBar()
        {
            Size = new Point(44, 44);
            Dock = DockStyle.Bottom;

            var frame = new Frame();
            frame.Dock = DockStyle.CenterX;
            frame.AutoSize = AutoSize.Horizontal;
            Controls.Add(frame);

            for (var i = 0; i < 8; i++)
            {
                var button = new ActionButton();
                button.Size = new Point(40, 40);
                button.Style = "button";
                button.Dock = DockStyle.Left;
                button.Margin = new Margin(2);
                button.AllowDrop = true;
                button.DragDrop += button_DragDrop;
                frame.Controls.Add(button);
            }
        }

        private void button_DragDrop(Control sender, DragDropEventArgs e)
        {
            var slot = sender as ActionButton;

            slot.Item = e.DraggedControl.Tag as GuiItem;
        }
    }


    public class MiniMap : Window
    {
        private Point clickedPos;
        private readonly Button _handle;
        private readonly Frame _map;
        private Point _oldSize;
        private readonly Frame _toolbar;

        public MiniMap()
        {
            Style = "window";
            Size = new Point(256, 256);
            MaxSize = new Point(512, 512);
            MinSize = new Point(128, 128);
            Resizable = false;

            _handle = new Button {Size = new Point(16, 16), Style = "handleNW"};
            //handle.Cursor = Cursors.SizeNWSE;
            _handle.MouseDown += handle_OnMouseDown;
            _handle.MousePress += handle_OnMousePress;
            Elements.Add(_handle);

            _toolbar = new Frame {Size = new Point(28, 28)};
            _toolbar.Dock = DockStyle.Right;
            _toolbar.Style = "frame";
            Elements.Add(_toolbar);

            for (var i = 0; i < 5; i++)
            {
                var button = new Button {Size = new Point(24, 26)};
                button.Dock = DockStyle.Top;
                button.Margin = new Margin(2, 2, 2, 0);
                _toolbar.Controls.Add(button);
            }

            _map = new Frame();
            _map.Dock = DockStyle.Fill;
            _map.Scissor = true;
            _map.Margin = new Margin(1);
            Controls.Add(_map);

            var image = new ImageControl();
            image.Texture = "minimap.jpg";
            image.Size = new Point(1024, 1024);
            image.MousePress += image_MousePress;
            _map.Controls.Add(image);
        }

        private void image_MousePress(Control sender, MouseEventArgs args)
        {
            sender.Position += Gui.MouseMovement;
        }

        private void handle_OnMouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            clickedPos = Gui.MousePosition;
            _oldSize = Size;
        }

        private void handle_OnMousePress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            ResizeTo(_oldSize + (clickedPos - Gui.MousePosition), AnchorStyles.Top | AnchorStyles.Left);
        }

        protected override void OnUpdate()
        {
            Position = Parent.Size - Size - new Point(0, 14);

            base.OnUpdate();
        }
    }

    public class ChatTab : Control
    {
        public ChatTab()
        {
            Size = new Point(100, 100);
            Dock = DockStyle.Fill;

            Scrollbar = new ScrollBar();
            Scrollbar.Dock = DockStyle.Left;
            Scrollbar.Size = new Point(25, 25);
            Scrollbar.Margin = new Margin(0, 8, 8, 8);
            Scrollbar.Size = new Point(14, 10);
            Scrollbar.Slider.Style = "vscrollTrack";
            Scrollbar.Slider.Button.Style = "vscrollButton";
            Scrollbar.ButtonUp.Style = "vscrollUp";
            Scrollbar.ButtonUp.Size = new Point(10, 20);
            Scrollbar.ButtonDown.Style = "vscrollUp";
            Scrollbar.ButtonDown.Size = new Point(10, 20);
            Scrollbar.Slider.Margin = new Margin(0, 2, 0, 2);
            Scrollbar.ShowAlways = true;
            Elements.Add(Scrollbar);

            Frame = new Frame();
            Frame.Dock = DockStyle.Fill;
            Frame.Scissor = true;
            Elements.Add(Frame);

            Output = new Label();
            Output.BbCodeEnabled = true;
            Output.TextWrap = true;
            Output.AutoSize = AutoSize.Vertical;
            Output.Text =
                "Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet. Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam erat, sed diam voluptua. At vero eos et accusam et justo duo dolores et ea rebum. Stet clita kasd gubergren, no sea takimata sanctus est Lorem ipsum dolor sit amet.";
            Output.Style = "multiline";
            Output.Margin = new Margin(8, 8, 8, 8);
            Output.TextAlign = Alignment.BottomLeft;
            Frame.Controls.Add(Output);
        }

        public Label Output { get; }
        public ScrollBar Scrollbar { get; }
        public Frame Frame { get; }

        protected override void OnUpdate()
        {
            // force the width to be that of its parent
            Output.Size = new Point(Frame.Size.X, Output.Size.Y);

            // move the label up/down using the scrollbar value
            if (Output.Size.Y < Frame.Size.Y) // no need to scroll
            {
                Scrollbar.Visible = false; // hide scrollbar
                Output.Position = new Point(0, Frame.Size.Y - Output.Size.Y); // set fixed position
            }
            else
            {
                Scrollbar.Scale = Math.Min(1, Frame.Size.Y/(float) Output.Size.Y);
                Scrollbar.Visible = true; // show scrollbar
                Output.Position = new Point(0, (int) ((Frame.Size.Y - Output.Size.Y)*Scrollbar.EasedValue));
            }

            // the mouse is scrolling and there is any control hovered
            if ((Gui.MouseScroll != 0) && (Desktop.HotControl != null))
                if (Hit(Gui.MousePosition.X, Gui.MousePosition.Y))
                    if ((Desktop.HotControl == this) || Desktop.HotControl.IsChildOf(this))
                        Scrollbar.Scroll(Gui.MouseScroll);
        }

        public void Append(string text)
        {
            // check for null/empty
            if (string.IsNullOrEmpty(text))
                return;

            // return if only whitespaces were entered
            if (text.Trim().Length == 0)
                return;

            var prefix = ""; // "[Username]: ";

            if (string.IsNullOrEmpty(Output.Text))
                Output.Text = prefix + text;
            else
                Output.Text += Environment.NewLine + prefix + text;

            Scrollbar.Value = 1; // scroll down
        }
    }

    public class ChatWindow : Window
    {
        private ChatTab _chat;

        private Point clickedPos;
        private readonly Button _handle;
        public TextBox Input;
        private Point _oldSize;
        private readonly TabControl _tabs;

        public ChatWindow()
        {
            Style = "window";
            Resizable = false;
            Size = new Point(384, 256);
            MinSize = new Point(192, 128);
            MaxSize = new Point(786, 512);

            _handle = new Button {Size = new Point(16, 16), Position = new Point(384 - 16, 0), Style = "handleNE"};
            _handle.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            //handle.Cursor = Cursors.SizeNESW;
            _handle.MouseDown += handle_OnMouseDown;
            _handle.MousePress += handle_OnMousePress;
            Elements.Add(_handle);

            Input = new TextBox();
            Input.Size = new Point(100, 35);
            Input.Dock = DockStyle.Bottom;
            Input.TextCommit += Input_OnTextCommit;
            Input.Style = "textbox";
            Input.Margin = new Margin(2);
            Controls.Add(Input);

            _tabs = new TabControl();
            _tabs.Dock = DockStyle.Fill;
            _tabs.ButtonFrame.Size = new Point(28, 28);
            Controls.Add(_tabs);

            AddPage("Global");
            AddPage("Party");
            AddPage("System");
        }

        private void handle_OnMouseDown(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            clickedPos = Gui.MousePosition;
            _oldSize = Size;
        }

        private void handle_OnMousePress(Control sender, MouseEventArgs args)
        {
            if (args.Button > 0) return;

            var p = Gui.MousePosition - clickedPos;
            p.Y = clickedPos.Y - Gui.MousePosition.Y;

            ResizeTo(_oldSize + p, AnchorStyles.Top | AnchorStyles.Right);
        }

        private void AddPage(string name)
        {
            var page = new TabPage();
            page.Button.Text = name;
            page.Button.Size = new Point(100, 10);

            var close = new Button();
            close.Size = new Point(16, 16);
            close.Dock = DockStyle.Right;
            close.Margin = new Margin(6);
            page.Button.Controls.Add(close);

            var config = new Button();
            config.Size = new Point(16, 16);
            config.Dock = DockStyle.Left;
            config.Margin = new Margin(6);
            page.Button.Controls.Add(config);

            page.Button.Update += delegate
            {
                config.Visible = page.Button.Checked;
                close.Visible = page.Button.Checked || page.Button.Hit(Gui.MousePosition.X, Gui.MousePosition.Y);
            };

            page.Button.Controls.Add(close);
            _chat = new ChatTab();
            page.Controls.Add(_chat);

            _tabs.TabPages.Add(page);
        }

        private void Input_OnTextCommit(object sender, EventArgs e)
        {
            // Append(Input.Text); // try to append the text
            Input.Text = string.Empty;
            Input.Focus();
        }

        protected override void OnUpdate()
        {
            Position = new Point(0, Parent.Size.Y - Size.Y - 14);

            base.OnUpdate();
        }
    }
}