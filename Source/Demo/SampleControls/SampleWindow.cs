using MonoGame.Squid.Controls;
using MonoGame.Squid.Structs;

namespace Demo.SampleControls
{
    public class SampleWindow : Window
    {
        public SampleWindow()
        {
            AllowDragOut = true;
            Padding = new Margin(4);

            Titlebar = new TitleBar();
            Titlebar.Dock = DockStyle.Top;
            Titlebar.Size = new Point(122, 35);
            Titlebar.MouseDown += delegate { StartDrag(); };
            Titlebar.MouseUp += delegate { StopDrag(); };
            //Titlebar.Cursor = Cursors.Move;
            Titlebar.Style = "frame";
            Titlebar.Margin = new Margin(-4, -4, -4, -1);
            Titlebar.Button.MouseClick += Button_OnMouseClick;
            Titlebar.TextAlign = Alignment.MiddleLeft;
            Titlebar.BbCodeEnabled = true;
            AllowDragOut = false;

            Controls.Add(Titlebar);
        }

        public TitleBar Titlebar { get; }

        private void Button_OnMouseClick(Control sender, MouseEventArgs args)
        {
            //Animation.Custom(FadeAndClose());
        }

        //    yield return Animation.Opacity(0, 500);
        //{

        //private System.Collections.IEnumerator FadeAndClose()
        //    Close();
        //}
    }

    public class TitleBar : Label
    {
        public TitleBar()
        {
            Button = new Button();
            Button.Size = new Point(30, 30);
            Button.Style = "button";
            Button.Tooltip = "Close Window";
            Button.Dock = DockStyle.Right;
            Button.Margin = new Margin(0, 8, 8, 8);
            Elements.Add(Button);
        }

        public Button Button { get; }
    }
}