using System;
using System.Runtime.InteropServices;

namespace ClassifyFiles.Util.Win32.Shell
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct SHELLEXECUTEINFO
    {
        public int cbSize;
        public uint fMask;
        public IntPtr hwnd;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpVerb;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpFile;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpParameters;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpDirectory;

        public int nShow;
        public IntPtr hInstApp;
        public IntPtr lpIDList;

        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpClass;

        public IntPtr hkeyClass;
        public uint dwHotKey;
        public IntPtr hIcon;
        public IntPtr hProcess;

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(SHELLEXECUTEINFO left, SHELLEXECUTEINFO right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SHELLEXECUTEINFO left, SHELLEXECUTEINFO right)
        {
            return !(left == right);
        }
    }
}