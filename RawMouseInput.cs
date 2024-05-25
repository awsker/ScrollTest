using System.Runtime.InteropServices;

public class RawMouseInput
{
    private const int RIM_TYPEMOUSE = 0;
    private const int RID_INPUT = 0x10000003;
    private const int WM_INPUT = 0x00FF;

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RAWMOUSE
    {
        [FieldOffset(0)]
        public ushort usFlags;
        [FieldOffset(4)]
        public uint ulButtons;
        [FieldOffset(4)]
        public ushort usButtonFlags;
        [FieldOffset(6)]
        public ushort usButtonData;
        [FieldOffset(8)]
        public uint ulRawButtons;
        [FieldOffset(12)]
        public int lLastX;
        [FieldOffset(16)]
        public int lLastY;
        [FieldOffset(20)]
        public uint ulExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUT
    {
        public RAWINPUTHEADER header;
        public RAWMOUSE mouse;
    }

    [DllImport("User32.dll")]
    private static extern uint GetRawInputData(IntPtr hRawInput, uint uiCommand, IntPtr pData, ref uint pcbSize, uint cbSizeHeader);

    [DllImport("User32.dll")]
    private static extern bool RegisterRawInputDevices(RAWINPUTDEVICE[] pRawInputDevices, uint uiNumDevices, uint cbSize);

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTDEVICE
    {
        public ushort usUsagePage;
        public ushort usUsage;
        public uint dwFlags;
        public IntPtr hwndTarget;
    }

    private IntPtr hwnd;

    public RawMouseInput(IntPtr hwnd)
    {
        this.hwnd = hwnd;

        RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[1];
        rid[0].usUsagePage = 0x01; // Generic desktop controls
        rid[0].usUsage = 0x02;     // Mouse
        rid[0].dwFlags = 0x00000100; // RIDEV_INPUTSINK
        rid[0].hwndTarget = hwnd;

        if (!RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE))))
        {
            throw new ApplicationException("Failed to register raw input device(s).");
        }
    }

    public event EventHandler<MouseEventArgs> MouseWheel;

    public void ProcessRawInput(Message message)
    {
        if (message.Msg != WM_INPUT)
        {
            return;
        }

        uint dwSize = 0;
        GetRawInputData(message.LParam, RID_INPUT, IntPtr.Zero, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER)));

        IntPtr buffer = Marshal.AllocHGlobal((int)dwSize);
        try
        {
            if (GetRawInputData(message.LParam, RID_INPUT, buffer, ref dwSize, (uint)Marshal.SizeOf(typeof(RAWINPUTHEADER))) != dwSize)
            {
                return;
            }

            RAWINPUT raw = (RAWINPUT)Marshal.PtrToStructure(buffer, typeof(RAWINPUT));

            if (raw.header.dwType == RIM_TYPEMOUSE && (raw.mouse.usButtonFlags & 0x0400) != 0)
            {
                OnMouseWheel(new MouseEventArgs(MouseButtons.None, 0, 0, 0, (short)raw.mouse.usButtonData));
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    protected virtual void OnMouseWheel(MouseEventArgs e)
    {
        MouseWheel?.Invoke(this, e);
    }
}
