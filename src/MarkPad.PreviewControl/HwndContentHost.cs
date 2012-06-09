using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace MarkPad.PreviewControl
{
    public class HwndContentHost : HwndHost
    {
        /// <summary>
        /// The window handle to host.
        /// </summary>
        readonly IntPtr handle;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handle">The handle of the Window to host.</param>
        public HwndContentHost(IntPtr handle)
        {
            this.handle = handle;
        }

        /// <summary>
        /// Creates the window to be hosted.
        /// </summary>
        /// <param name="hwndParent">The window handle of the parent window.</param>
        /// <returns>The handle to the child Win32 window to create.</returns>
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            UnsafeNativeMethods.SetWindowLong(handle,
                                              UnsafeNativeMethods.GWL_STYLE, UnsafeNativeMethods.WS_CHILD);

            UnsafeNativeMethods.SetParent(
                new HandleRef(null, handle), hwndParent);

            return new HandleRef(this, handle);
        }

        /// <summary>
        /// Destroys the hosted window.
        /// </summary>
        /// <param name="hwnd">A structure that contains the window handle.</param>
        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            UnsafeNativeMethods.DestroyWindow(hwnd.Handle);
        }
    }
}