using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

class InputHook : IDisposable
{
    const int WH_KEYBOARD_LL = 13;
    const int WH_MOUSE_LL = 14;

    const int WM_KEYDOWN = 0x100;
    const int WM_SYSKEYDOWN = 0x104;
    const int WM_LBUTTONDOWN = 0x201;
    const int WM_RBUTTONDOWN = 0x204;
    const int WM_MBUTTONDOWN = 0x207;
    const int WM_MOUSEWHEEL = 0x20A;
    const int WM_MOUSEMOVE = 0x200;

    [DllImport("user32.dll")]
    static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetModuleHandle(string lpModuleName);

    delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

    IntPtr kbHook;
    IntPtr mouseHook;
    LowLevelProc kbProc;
    LowLevelProc mouseProc;
    Point lastMousePos;
    bool firstMouseMove = true;

    public InputHook()
    {
        kbProc = KbCallback;
        mouseProc = MouseCallback;
        IntPtr hMod = GetModuleHandle(null);

        kbHook = SetWindowsHookEx(WH_KEYBOARD_LL, kbProc, hMod, 0);
        mouseHook = SetWindowsHookEx(WH_MOUSE_LL, mouseProc, hMod, 0);
    }

    IntPtr KbCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            Environment.Exit(0);

        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    IntPtr MouseCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = (int)wParam;

            if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN ||
                msg == WM_MBUTTONDOWN || msg == WM_MOUSEWHEEL)
            {
                Environment.Exit(0);
            }
            else if (msg == WM_MOUSEMOVE)
            {
                if (firstMouseMove)
                {
                    firstMouseMove = false;
                    lastMousePos = Cursor.Position;
                }
                else
                {
                    Point pos = Cursor.Position;
                    if (Math.Abs(pos.X - lastMousePos.X) > 3 ||
                        Math.Abs(pos.Y - lastMousePos.Y) > 3)
                        Environment.Exit(0);
                }
            }
        }

        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (kbHook != IntPtr.Zero) { UnhookWindowsHookEx(kbHook); kbHook = IntPtr.Zero; }
        if (mouseHook != IntPtr.Zero) { UnhookWindowsHookEx(mouseHook); mouseHook = IntPtr.Zero; }
    }
}

class DvdScreensaver : Form
{
    [DllImport("user32.dll")]
    static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public int Left, Top, Right, Bottom;
    }

    const int WS_CHILD = 0x40000000;

    WebView2 webView;
    InputHook hook;
    IntPtr previewHwnd;
    bool isPreview;

    public DvdScreensaver(IntPtr previewHwnd)
    {
        this.previewHwnd = previewHwnd;
        isPreview = previewHwnd != IntPtr.Zero;

        BackColor = Color.FromArgb(10, 10, 10);
        FormBorderStyle = FormBorderStyle.None;

        if (isPreview)
        {
            ShowInTaskbar = false;
        }
        else
        {
            WindowState = FormWindowState.Maximized;
            TopMost = true;
            Cursor.Hide();
        }

        webView = new WebView2();
        webView.Dock = DockStyle.Fill;
        Controls.Add(webView);

        Load += OnLoad;
        FormClosed += OnFormClosed;
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            if (isPreview && previewHwnd != IntPtr.Zero)
            {
                cp.Parent = previewHwnd;
                cp.Style |= WS_CHILD;
            }
            return cp;
        }
    }

    async void OnLoad(object sender, EventArgs e)
    {
        if (isPreview && previewHwnd != IntPtr.Zero)
        {
            if (GetClientRect(previewHwnd, out RECT rect))
            {
                Size = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            Location = Point.Empty;
        }
        else
        {
            hook = new InputHook();
        }

        await webView.EnsureCoreWebView2Async(null);

        string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index.html");
        if (!File.Exists(htmlPath))
            htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "index.html");

        if (File.Exists(htmlPath))
            webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
    }

    void OnFormClosed(object sender, FormClosedEventArgs e)
    {
        hook?.Dispose();
    }

    [STAThread]
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        IntPtr previewHwnd = IntPtr.Zero;

        if (args.Length > 0)
        {
            string arg = args[0].ToLowerInvariant().TrimStart('/').TrimStart('-');

            if (arg == "c")
            {
                MessageBox.Show(
                    "DVD Screensaver\n\nThe classic bouncing DVD logo.\nBuilt with HTML, CSS & JavaScript.",
                    "DVD Screensaver",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (arg == "p" && args.Length > 1)
            {
                string hwndStr = args[1];
                if (hwndStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    hwndStr = hwndStr.Substring(2);
                previewHwnd = new IntPtr(long.Parse(hwndStr,
                    System.Globalization.NumberStyles.HexNumber));
            }
        }

        Application.Run(new DvdScreensaver(previewHwnd));
    }
}
