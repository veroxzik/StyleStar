using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace StyleStar
{
    class TouchWindowsHook
    {
        // Touch event window message constants [winuser.h]
        private const int WM_TOUCH = 0x0240;

        // Pointer events
        private const int WM_POINTERUPDATE = 0x245;
        private const int WM_POINTERDOWN = 0x246;
        private const int WM_POINTERUP = 0x247;

        public enum HookId
        {
            // Types of hook that can be installed using the SetWindwsHookEx function.
            WH_CALLWNDPROC = 4,
            WH_CALLWNDPROCRET = 12,
            WH_CBT = 5,
            WH_DEBUG = 9,
            WH_FOREGROUNDIDLE = 11,
            WH_GETMESSAGE = 3,
            WH_HARDWARE = 8,
            WH_JOURNALPLAYBACK = 1,
            WH_JOURNALRECORD = 0,
            WH_KEYBOARD = 2,
            WH_KEYBOARD_LL = 13,
            WH_MAX = 11,
            WH_MAXHOOK = WH_MAX,
            WH_MIN = -1,
            WH_MINHOOK = WH_MIN,
            WH_MOUSE_LL = 14,
            WH_MSGFILTER = -1,
            WH_SHELL = 10,
            WH_SYSMSGFILTER = 6,
            WH_TOUCH = 0x0240,
        };

        // Touch API defined structures [winuser.h]
        [StructLayout(LayoutKind.Sequential)]
        private struct TOUCHINPUT
        {
            public int x;
            public int y;
            public System.IntPtr hSource;
            public int dwID;
            public int dwFlags;
            public int dwMask;
            public int dwTime;
            public System.IntPtr dwExtraInfo;
            public int cxContact;
            public int cyContact;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTS
        {
            public short x;
            public short y;
        }

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        // A delegate used to create a hook callback.
        public delegate int GetMsgProc(int nCode, int wParam, ref System.Windows.Forms.Message msg);

        /// <summary>
        /// Install an application-defined hook procedure into a hook chain.
        /// </summary>
        /// <param name="idHook">Specifies the type of hook procedure to be installed.</param>
        /// <param name="lpfn">Pointer to the hook procedure.</param>
        /// <param name="hmod">Handle to the DLL containing the hook procedure pointed to by the lpfn parameter.</param>
        /// <param name="dwThreadId">Specifies the identifier of the thread with which the hook procedure is to be associated.</param>
        /// <returns>If the function succeeds, the return value is the handle to the hook procedure. Otherwise returns 0.</returns>
        [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA")]
        public static extern IntPtr SetWindowsHookEx(HookId idHook, GetMsgProc lpfn, IntPtr hmod, int dwThreadId);

        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the SetWindowsHookEx function. 
        /// </summary>
        /// <param name="hHook">Handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to SetWindowsHookEx.</param>
        /// <returns>If the function fails, the return value is zero. To get extended error information, call GetLastError.</returns>
        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hHook);

        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain.
        /// </summary>
        /// <param name="hHook">Ignored.</param>
        /// <param name="ncode">Specifies the hook code passed to the current hook procedure.</param>
        /// <param name="wParam">Specifies the wParam value passed to the current hook procedure.</param>
        /// <param name="lParam">Specifies the lParam value passed to the current hook procedure.</param>
        /// <returns>This value is returned by the next hook procedure in the chain.</returns>
        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int hHook, int ncode, int wParam, ref System.Windows.Forms.Message lParam);

        /// <summary>
        /// Translates virtual-key messages into character messages.
        /// </summary>
        /// <param name="lpMsg">Pointer to an Message structure that contains message information retrieved from the calling thread's message queue.</param>
        /// <returns>If the message is translated (that is, a character message is posted to the thread's message queue), the return value is true.</returns>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref System.Windows.Forms.Message lpMsg);

        /// <summary>
        /// Retrieves the thread identifier of the calling thread.
        /// </summary>
        /// <returns>The thread identifier of the calling thread.</returns>
        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        Point GetPoint(IntPtr _xy)
        {
            uint xy = unchecked(IntPtr.Size == 8 ? (uint)_xy.ToInt64() : (uint)_xy.ToInt32());
            int x = unchecked((short)xy);
            int y = unchecked((short)(xy >> 16));
            return new Point(x, y);
        }

        uint GetPointerID(IntPtr wParam)
        {
            return unchecked(IntPtr.Size == 8 ? (uint)wParam.ToInt64() : (uint)wParam.ToInt32()) & 0xFFFF;
        }

        Rect GetRect()
        {
            Process[] processes = Process.GetProcessesByName("StyleStar");
            Process lol = processes[0];
            IntPtr ptr = lol.MainWindowHandle;
            Rect rect = new Rect();
            GetWindowRect(ptr, ref rect);
            return rect;
        }

        Point GetAbsPoint(Point xy, Rect winSize)
        {
            int width = winSize.Right - winSize.Left;
            int height = winSize.Bottom - winSize.Top;
            float xRatio = (float)(xy.X - winSize.Left) / width;
            float yRatio = (float)(xy.Y - winSize.Top) / height;

            return new Point((int)(xRatio * 1024), (int)(yRatio * 1024));
        }

        // Handle for the created hook.
        private readonly IntPtr HookHandle;

        private readonly GetMsgProc ProcessMessagesCallback;

        private TouchCollection touchCollection;
        private MusicManager musicManager;

        private ConcurrentQueue<Message> msgQueue = new ConcurrentQueue<Message>();

        private BackgroundWorker worker = new BackgroundWorker();

        public TouchWindowsHook(TouchCollection _tc, MusicManager _mm)
        {
            // Create the delegate callback:
            this.ProcessMessagesCallback = new GetMsgProc(ProcessMessages);
            // Create the keyboard hook:
            this.HookHandle = SetWindowsHookEx(HookId.WH_GETMESSAGE, this.ProcessMessagesCallback, IntPtr.Zero, GetCurrentThreadId());

            // TouchCollection from main process
            touchCollection = _tc;
            // Music manager
            musicManager = _mm;

            worker.DoWork += Worker_DoWork;
        }

        

        public void Dispose()
        {
            // Remove the hook.
            if (HookHandle != IntPtr.Zero) UnhookWindowsHookEx(HookHandle);
        }

        // comments found in this region are all from the original author: Darg.
        private int ProcessMessages(int nCode, int wParam, ref Message msg)
        {
            bool handled = false;

            if (msg.Msg == WM_POINTERUPDATE ||
                msg.Msg == WM_POINTERDOWN ||
                msg.Msg == WM_POINTERUP)
            {
                msgQueue.Enqueue(msg);
                if(!worker.IsBusy)
                {
                    worker.RunWorkerAsync();
                }
                handled = true;
            }


            

            //Point pt;
            //uint id = 0;
            //var currentBeat = musicManager.GetCurrentBeat();
            //var rect = GetRect();

            //switch (msg.Msg)
            //{
            //    case WM_POINTERUPDATE:
            //        //Console.WriteLine("Pointer update event");
            //        pt = GetPoint(msg.LParam);
            //        id = GetPointerID(msg.WParam);
            //        //Console.WriteLine("ID: " + id + "\tX: " + pt.X + "\tY: " + pt.Y);
            //        //Console.WriteLine("Received message: " + msg);
            //        if (touchCollection != null)
            //        {
            //            var ptAbs = GetAbsPoint(pt, rect);
            //            touchCollection.UpdateID(id, ptAbs);
            //            handled = true;
            //        }
            //        break;
            //    case WM_POINTERUP:
            //        //Console.WriteLine("Pointer up event");
            //        pt = GetPoint(msg.LParam);
            //        id = GetPointerID(msg.WParam);
            //        Console.WriteLine("ID: " + id + "\tX: " + pt.X + "\tY: " + pt.Y);
            //        //Console.WriteLine("Received message: " + msg);
            //        if (touchCollection != null)
            //        {
            //            touchCollection.RemoveID(id);
            //            handled = true;
            //        }
            //        break;
            //    case WM_POINTERDOWN:
            //        Console.WriteLine("Pointer down event");
            //        pt = GetPoint(msg.LParam);
            //        id = GetPointerID(msg.WParam);
            //        Console.WriteLine("ID: " + id + "\tX: " + pt.X + "\tY: " + pt.Y);
            //        if (touchCollection != null)
            //        {
            //            var ptAbs = GetAbsPoint(pt, rect);
            //            touchCollection.Points.Add(
            //                new TouchPoint(currentBeat)
            //                {
            //                    RawX = ptAbs.X,
            //                    RawY = ptAbs.Y,
            //                    RawWidth = 128,
            //                    RawHeight = 20,
            //                    ID = id
            //                });
            //            handled = true;
            //            Console.WriteLine("Point added\tID: " + id);
            //        }
            //        //Console.WriteLine("Received message: " + msg);
            //        break;
            //    default:
            //        handled = false;
            //        break;
            //}

            if (handled)
                msg.Result = new IntPtr(1);

            // Call next hook in chain:
            return CallNextHookEx(0, nCode, wParam, ref msg);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (msgQueue.Count > 0)
            {
                Message msg;
                if (msgQueue.TryDequeue(out msg))
                {
                    Point pt;
                    uint id = 0;
                    var currentBeat = musicManager.GetCurrentBeat();
                    var rect = GetRect();

                    switch (msg.Msg)
                    {
                        case WM_POINTERUPDATE:
                            //Console.WriteLine("Pointer update event");
                            pt = GetPoint(msg.LParam);
                            id = GetPointerID(msg.WParam);
                            //Console.WriteLine("ID: " + id + "\tX: " + pt.X + "\tY: " + pt.Y);
                            //Console.WriteLine("Received message: " + msg);
                            if (touchCollection != null)
                            {
                                var ptAbs = GetAbsPoint(pt, rect);
                                touchCollection.UpdateID(id, ptAbs);
                            }
                            break;
                        case WM_POINTERUP:
                            //Console.WriteLine("Pointer up event");
                            pt = GetPoint(msg.LParam);
                            id = GetPointerID(msg.WParam);
                            //Console.WriteLine("ID: " + id + "\tX: " + pt.X + "\tY: " + pt.Y);
                            //Console.WriteLine("Received message: " + msg);
                            if (touchCollection != null)
                            {
                                touchCollection.RemoveID(id);
                            }
                            break;
                        case WM_POINTERDOWN:
                            //Console.WriteLine("Pointer down event");
                            pt = GetPoint(msg.LParam);
                            id = GetPointerID(msg.WParam);
                            //Console.WriteLine("ID: " + id + "\tX: " + pt.X + "\tY: " + pt.Y);
                            if (touchCollection != null)
                            {
                                var ptAbs = GetAbsPoint(pt, rect);
                                touchCollection.Points.TryAdd(
                                    id, 
                                    new TouchPoint(currentBeat)
                                    {
                                        RawX = ptAbs.X,
                                        RawY = ptAbs.Y,
                                        RawWidth = 128,
                                        RawHeight = 20,
                                        ID = id
                                    });
                                //Console.WriteLine("Point added\tID: " + id);
                            }
                            //Console.WriteLine("Received message: " + msg);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
