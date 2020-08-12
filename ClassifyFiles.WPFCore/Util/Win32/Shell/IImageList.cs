using System;
using System.Runtime.InteropServices;

namespace ClassifyFiles.Util.Win32.Shell
{
    [ComImportAttribute()]
    [GuidAttribute("46EB5926-582E-4017-9FDF-E8998DAA0950")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IImageList
    {
        [PreserveSig]
        int Add(
            IntPtr hbmImage,
            IntPtr hbmMask,
            ref int pi);

        [PreserveSig]
        int ReplaceIcon(
            int i,
            IntPtr hicon,
            ref int pi);

        [PreserveSig]
        int SetOverlayImage(
            int iImage,
            int iOverlay);

        [PreserveSig]
        int Replace(
            int i,
            IntPtr hbmImage,
            IntPtr hbmMask);

        [PreserveSig]
        int AddMasked(
            IntPtr hbmImage,
            int crMask,
            ref int pi);

        [PreserveSig]
        int Draw(
            ref IMAGELISTDRAWPARAMS pimldp);

        [PreserveSig]
        int Remove(
            int i);

        [PreserveSig]
        int GetIcon(
            int i,
            int flags,
            ref IntPtr picon);
    };
}