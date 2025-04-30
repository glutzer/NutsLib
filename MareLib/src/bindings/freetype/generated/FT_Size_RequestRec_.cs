using System;
using System.Runtime.InteropServices;

namespace FreeTypeSharp;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct FT_Size_RequestRec_
{
    public FT_Size_Request_Type_ type;
    public IntPtr width;
    public IntPtr height;
    public uint horiResolution;
    public uint vertResolution;
}