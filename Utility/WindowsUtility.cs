// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

#if UNITY_STANDALONE_WIN

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class WindowsUtility
{
    #region API
    #region 常量
    public const int GWL_STYLE = -16;              //设定一个新的窗口风格。
    public const int GWL_EXSTYLE = -20;            //设定一个新的扩展风格。
    public const int WS_BORDER = 0x00800000;       //window with border
    public const int WS_CAPTION = 0x00C00000;      //window with a title bar with border
    public const int WS_SYSMENU = 0x00080000;      //window with no borders etc.
    public const int WS_MAXIMIZE = 0x01000000;
    public const int WS_MAXIMIZEBOX = 0x00010000;
    public const int WS_MINIMIZE = 0x20000000;
    public const int WS_MINIMIZEBOX = 0x00020000;
    public const int WS_SIZEBOX = 0x00040000;
    public const int WS_VISIBLE = 0x10000000;
    public const int WS_TABSTOP = 0x00010000;
    public const int WS_CLIPCHILDREN = 0x02000000;
    public const int WS_CLIPSIBLINGS = 0x04000000;
    #endregion
    #region windowRect
    public struct Rect
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }
        public int Width { get { return Right - Left; } }
        public int Height { get { return Bottom - Top; } }

        public override string ToString()
        {
            return "(l: " + Left + ", r: " + Right + ", t: " + Top + ", b: " + Bottom + ")";
        }
    }
    #endregion
    /// <summary>
    /// 设置窗口层级，如果newParent为zero则设置为最顶层
    /// </summary>
    /// <param name="child"></param>
    /// <param name="newParent"></param>
    /// <returns></returns>
    [DllImport("user32")]
    public static extern IntPtr SetParent(IntPtr child, IntPtr newParent);

    [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
    public static extern bool SetWindowPos(System.IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

    public delegate bool EnumWindowsProc(System.IntPtr hWnd, System.IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    public static extern IntPtr GetDesktopWindow();

    /// <summary>
    /// 设置窗口风格
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="nIndex"></param>
    /// <param name="dwNewLong"></param>
    /// <returns></returns>

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    /// <summary>
    /// 获取窗口风格
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="nIndex"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    internal static extern int MapWindowPoints(IntPtr hWndFrom, IntPtr hWndTo, [In, Out] ref Rect rect, [MarshalAs(UnmanagedType.U4)] int cPoints);

    /// <summary>
    /// 枚举当前的所有窗口
    /// </summary>
    /// <param name="enumProc"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    [DllImport("user32.dll")]
    public static extern bool EnumWindows(EnumWindowsProc enumProc, System.IntPtr lParam);

    /// <summary>
    /// 枚举指定窗口的所有子窗口
    /// </summary>
    /// <param name="window"></param>
    /// <param name="callback"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    [DllImport("user32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

    public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern int GetWindowThreadProcessId(System.IntPtr handle, out int processId);
    /// <summary>
    /// 查找主窗体
    /// </summary>
    /// <param name="lpClassName"></param>
    /// <param name="lpWindowName"></param>
    /// <returns></returns>

    [DllImport("user32.dll", EntryPoint = "FindWindow")]  //声明FindWindowAPI
    public extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(string lpClassName, string lpWindowName);

    // Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    public static extern IntPtr FindWindowByCaptionEx(IntPtr ZeroOnly, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int GetWindowText(System.IntPtr hWnd, StringBuilder text, int nMaxCount);

    [DllImport("user32.dll")]
    public static extern int GetWindowTextLength(System.IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();
    /// <summary>
    /// 获取窗体大小
    /// </summary>
    /// <param name="hwnd"></param>
    /// <param name="rectangle"></param>
    /// <returns></returns>

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(System.IntPtr hwnd, ref Rect rectangle);

    //键鼠操作

    [DllImport("user32.dll")]
    public static extern int SetCursorPos(int x, int y); //设置光标位置
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(ref int x, ref int y); //获取光标位置
    [DllImport("user32.dll")]
    public static extern void mouse_event(MouseEventFlags flags, int dx, int dy, uint data, UIntPtr extraInfo); //鼠标事件

    #region 图像
    [DllImport("user32.dll")]

    public static extern IntPtr GetWindowDC(

       IntPtr hwnd

       );

    [DllImport("gdi32.dll")]

    public static extern IntPtr CreateCompatibleBitmap(

           IntPtr hdc, // handle to DC

           int nWidth, // width of bitmap, in pixels

           int nHeight // height of bitmap, in pixels

           );

    [DllImport("gdi32.dll")]

    public static extern IntPtr CreateCompatibleDC(

            IntPtr hdc // handle to DC

            );

    [DllImport("gdi32.dll")]

    public static extern IntPtr SelectObject(

           IntPtr hdc, // handle to DC

           IntPtr hgdiobj // handle to object

           );

    [DllImport("user32.dll")]

    public static extern bool PrintWindow(

           IntPtr hwnd, // Window to copy,Handle to the window that will be copied. 

           IntPtr hdcBlt, // HDC to print into,Handle to the device context. 

           UInt32 nFlags // Optional flags,Specifies the drawing options. It can be one of the following values. 

           );

    [DllImport("gdi32.dll")]

    public static extern int DeleteDC(

           IntPtr hdc // handle to DC

           );

    [DllImport("gdi32.dll")]

    public static extern int DeleteObject(IntPtr hdc);
    #endregion
    #endregion
    #region 便捷方法
    /// <summary>
    /// 获取指定进程的主窗口
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    public static IntPtr GetMainWindowHandle(int processId)
    {
        var MainWindowHandle = IntPtr.Zero;
        EnumWindows(((hWnd, lParam) =>
        {
            int id;
            GetWindowThreadProcessId(hWnd, out id);

            if (id == processId)
            {
                MainWindowHandle = hWnd;
                return false;
            }
            //if (PID == lParam &&
            //    IsWindowVisible(hWnd) &&
            //    GetWindow(hWnd, GW_OWNER) == IntPtr.Zero)
            //{
            //    MainWindowHandle = hWnd;
            //    return false;
            //}

            return true;

        }), new IntPtr(processId));

        return MainWindowHandle;
    }
    /// <summary>
    /// 获取指定进程的所有窗口
    /// </summary>
    /// <param name="processId"></param>
    /// <returns></returns>
    public static IntPtr[] GetProcessWindows(int processId)
    {
        List<IntPtr> output = new List<IntPtr>();
        IntPtr winPtr = IntPtr.Zero;
        do
        {
            winPtr = FindWindowEx(IntPtr.Zero, winPtr, null, null);
            int id;
            GetWindowThreadProcessId(winPtr, out id);
            if (id == processId)
                output.Add(winPtr);
        } while (winPtr != IntPtr.Zero);

        return output.ToArray();
    }

    /// <summary>
    /// 获取进程的窗口大小
    /// </summary>
    /// <param name="process"></param>
    /// <param name="rect"></param>
    /// <returns></returns>
    public static bool GetProcessRect(System.Diagnostics.Process process, ref Rect rect)
    {
        IntPtr[] winPtrs = GetProcessWindows(process.Id);

        for (int i = 0; i < winPtrs.Length; i++)
        {
            bool gotRect = GetWindowRect(winPtrs[i], ref rect);
            if (gotRect && (rect.Left != 0 && rect.Top != 0))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 设置当前进程窗口位置和大小
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sizeX"></param>
    /// <param name="sizeY"></param>
    public static void SetWindowPosition(int x, int y, int sizeX = 0, int sizeY = 0)
    {
        System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
        process.Refresh();

        EnumWindows(delegate (System.IntPtr wnd, System.IntPtr param)
        {
            int id;
            GetWindowThreadProcessId(wnd, out id);
            if (id == process.Id)
            {
                SetWindowPos(wnd, 0, x, y, sizeX, sizeY, sizeX * sizeY == 0 ? 1 : 0);
                return false;
            }

            return true;
        }, System.IntPtr.Zero);
    }
    ///// <summary>
    ///// 获取窗体截图
    ///// </summary>
    ///// <param name="hWnd"></param>
    ///// <param name="Width"></param>
    ///// <param name="Height"></param>
    ///// <returns></returns>
    //public static UnityEngine.Texture2D Capture(IntPtr hWnd)
    //{
    //    try
    //    {
    //        // SetWindowLong(hWnd, GWL_STYLE, WS_MINIMIZE);
    //       // SetParent(hWnd, IntPtr.Zero);
    //        var hscrdc = GetWindowDC(hWnd);
    //        var rect = new Rect();
    //        GetWindowRect(hWnd, ref rect);
    //        IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, rect.Width, rect.Height);
    //        IntPtr hmemdc = CreateCompatibleDC(hscrdc);
    //        SelectObject(hmemdc, hbitmap);
    //        PrintWindow(hWnd, hmemdc, 0);
    //        System.Drawing.Bitmap bmp = System.Drawing.Bitmap.FromHbitmap(hbitmap);

    //        DeleteDC(hscrdc);//删除用过的对象

    //        DeleteObject(hbitmap);//删除用过的对象

    //        DeleteDC(hmemdc);//删除用过的对象
    //        var tex = new UnityEngine.Texture2D(0, 0);
    //        using (var ms = new System.IO.MemoryStream())
    //        {
    //            bmp.Save(ms, bmp.RawFormat);
    //            tex.LoadImage(ms.ToArray());
    //        }
    //        bmp.Dispose();
    //        return tex;
    //    }
    //    catch (Exception e)
    //    {
    //        UnityEngine.Debug.LogException(e);
    //        return null;
    //    }
    //}
    /// <summary>
    /// 获取窗体标题
    /// </summary>
    /// <param name="hWnd"></param>
    /// <returns></returns>
    public static string GetWindowTitle(IntPtr hWnd)
    {
        int length = GetWindowTextLength(hWnd) + 1;
        StringBuilder name = new StringBuilder(length);
        GetWindowText(hWnd, name, length);
        return name.ToString();
    }
    #endregion
}
[Flags]
public enum MouseEventFlags
{
    LeftDown = 0x00000002,
    LeftUp = 0x00000004,
    MiddleDown = 0x00000020,
    MiddleUp = 0x00000040,
    Move = 0x00000001,
    Absolute = 0x00008000,
    RightDown = 0x00000008,
    RightUp = 0x00000010
}

#endif