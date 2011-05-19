using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace NotationalFerocity.WPF
{
    /// <summary>
    /// Extends Window and adds Interop functions, primarily for adding to the Window's
    /// context menu
    /// </summary>
    public class InteropWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        protected static extern bool InsertMenu(IntPtr hMenu, Int32 wPosition, Int32 wFlags, Int32 wIDNewItem, string lpNewItem);

        internal const Int32 WM_SYSCOMMAND = 0x112;
        internal const Int32 MF_SEPARATOR = 0x800;
        internal const Int32 MF_BYPOSITION = 0x400;
        internal const Int32 MF_STRING = 0x0;

        /// <summary>
        /// The Win32 Interop Handle for this Window
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        public IntPtr SystemMenuHandle
        {
            get
            {
                return GetSystemMenu(Handle, false);
            }
        }

        public HwndSource Source
        {
            get
            {
                return HwndSource.FromHwnd(Handle);
            }
        }

        public InteropWindow()
        {
            Loaded += InteropWindow_Loaded;
        }

        private void InteropWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Attach our WndProc handler to this Window
            Source.AddHook(WndProc);
        }
        
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if a System Command has been executed
            if (msg != WM_SYSCOMMAND)
            {
                return IntPtr.Zero;
            }

            handled = HandleWndProc(wParam);

            return IntPtr.Zero;
        }

        internal virtual bool HandleWndProc(IntPtr wParam)
        {
            throw new NotImplementedException();
        }
    }
}