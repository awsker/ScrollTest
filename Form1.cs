

namespace ScrollTest
{
    public partial class Form1 : Form
    {
        private RawMouseInput _rawMouse;
        public Form1()
        {
            InitializeComponent();
            this.label1.MouseWheel += OnMouseWheel;

            _rawMouse = new RawMouseInput(this.Handle);
            _rawMouse.MouseWheel += RawMouseInput_MouseWheel;
        }

        private void OnMouseWheel(object? sender, MouseEventArgs e)
        {
            if(e.Delta < 0)
            {
                this.label1.Text = "Scrolled down!";
                this.label1.BackColor = Color.LimeGreen;
            } 
            else if (e.Delta > 0)
            {
                this.label1.Text = "Scrolled up!";
                this.label1.BackColor = Color.LimeGreen;
            }
        }

        private void RawMouseInput_MouseWheel(object? sender, MouseEventArgs e)
        {
            void update()
            {
                this.label2.Text = "Raw Mouse: Scroll " + e.Delta;
            }
            if(InvokeRequired)
            {
                Invoke(update);
            } 
            else
            {
                update();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (_rawMouse != null)
            {
                _rawMouse.ProcessRawInput(m);
            }
            base.WndProc(ref m);
        }
    }
}
