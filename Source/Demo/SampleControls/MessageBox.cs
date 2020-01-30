using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;

namespace Demo.SampleControls
{
    public class MessageBox : Dialog
    {
        private readonly Frame _buttonFrame;
        private readonly Label _messageLabel;
        private readonly Label _titleLabel;

        private MessageBox(string title, string message)
        {
            Modal = true; // make sure its modal
            Scissor = false;
            Padding = new Margin(7);

            _titleLabel = new Label();
            _titleLabel.Size = new Point(100, 35);
            _titleLabel.Dock = DockStyle.Top;
            _titleLabel.Text = title;
            _titleLabel.MouseDown += delegate { StartDrag(); };
            _titleLabel.MouseUp += delegate { StopDrag(); };
            //TitleLabel.Cursor = Cursors.Move;
            _titleLabel.Style = "frame";
            _titleLabel.Margin = new Margin(0, 0, 0, -1);
            Controls.Add(_titleLabel);

            _buttonFrame = new Frame();
            _buttonFrame.Size = new Point(100, 35);
            _buttonFrame.Dock = DockStyle.Bottom;
            Controls.Add(_buttonFrame);

            _messageLabel = new Label();
            _messageLabel.Dock = DockStyle.Fill;
            _messageLabel.TextWrap = true;
            _messageLabel.Text = message;
            Controls.Add(_messageLabel);
        }

        public static MessageBox Show(Point size, string title, string message, MessageBoxButtons buttons,
            Desktop target)
        {
            var box = new MessageBox(title, message);
            box.Size = size;
            box.Position = (target.Size - size)/2;
            box.InitButtons(buttons);
            box.Show(target);
            return box;
        }

        private void InitButtons(MessageBoxButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    AddButton("OK", DialogResult.Ok, 1);
                    break;
                case MessageBoxButtons.OkCancel:
                    AddButton("Cancel", DialogResult.Cancel, 2);
                    AddButton("OK", DialogResult.Ok, 2);
                    break;
                case MessageBoxButtons.RetryCancel:
                    AddButton("Cancel", DialogResult.Cancel, 2);
                    AddButton("Retry", DialogResult.Retry, 2);
                    break;
                case MessageBoxButtons.YesNo:
                    AddButton("No", DialogResult.No, 2);
                    AddButton("Yes", DialogResult.Yes, 2);
                    break;
                case MessageBoxButtons.YesNoCancel:
                    AddButton("No", DialogResult.No, 3);
                    AddButton("Cancel", DialogResult.Cancel, 3);
                    AddButton("Yes", DialogResult.Yes, 3);
                    break;
                case MessageBoxButtons.AbortRetryIgnore:
                    AddButton("Retry", DialogResult.Retry, 3);
                    AddButton("Ignore", DialogResult.Ignore, 3);
                    AddButton("Abort", DialogResult.Abort, 3);
                    break;
            }
        }

        private void AddButton(string text, DialogResult result, int divide)
        {
            var button = new Button();
            button.Style = "button";
            //button.Cursor = Cursors.Link;
            button.Margin = new Margin(2);
            button.Size = new Point(Size.X/(divide + 1), 35);
            button.Text = text;
            button.Tag = result;
            button.Dock = DockStyle.Right;
            button.MouseClick += button_OnMouseClick;
            _buttonFrame.Controls.Add(button);
        }

        private void button_OnMouseClick(Control sender, MouseEventArgs args)
        {
            //if (args.Button > 0) return;

            //Animation.Stop();
            //Animation.Custom(FadeAndClose(sender));
            Result = (DialogResult)sender.Tag;
            Close();
        }

        //}

        //    Close();

        //    Result = (DialogResult)sender.Tag;
        //    yield return Animation.Opacity(0, 500);
        //{

        //private System.Collections.IEnumerator FadeAndClose(Control sender)
    }

    public enum MessageBoxButtons
    {
        Ok = 0,
        OkCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5
    }
}