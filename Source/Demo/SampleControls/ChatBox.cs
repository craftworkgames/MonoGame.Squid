using System;
using MonoGame.Squid;
using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;

namespace Demo.SampleControls
{
    public class ChatBox : Control
    {
        public ChatBox()
        {
            Size = new Point(100, 100);

            Input = new TextBox();
            Input.Size = new Point(100, 35);
            Input.Dock = DockStyle.Bottom;
            Input.TextCommit += Input_OnTextCommit;
            Elements.Add(Input);

            Scrollbar = new ScrollBar();
            Scrollbar.Dock = DockStyle.Right;
            Scrollbar.Size = new Point(25, 25);
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
            Frame.Controls.Add(Output);
        }

        public TextBox Input { get; }
        public Label Output { get; }
        public ScrollBar Scrollbar { get; }
        public Frame Frame { get; }

        private void Input_OnTextCommit(object sender, EventArgs e)
        {
            Append(Input.Text); // try to append the text
            Input.Text = string.Empty;
            Input.Focus();
        }

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
                Output.Position = new Point(0, (int) ((Frame.Size.Y - Output.Size.Y)*Scrollbar.Value));
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
}