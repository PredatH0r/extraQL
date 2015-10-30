using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ExtraQL
{
  partial class Steamworks
  {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Init();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr SteamFriends();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void SetPersonaName(IntPtr handle, byte[] utf8name);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate bool Shutdown();


    // using the QL Dedicated Linux Server app-id so it won't block the QL client (282440) from starting
    public static int AppID { get; set; } = 349090;


    public static bool SetName(string name)
    {
      string dllDir = Application.StartupPath + "\\";
      bool ok = false;

      // using the QL Dedicated Linux Server app-id so it won't block the QL client (282440) from starting
      File.WriteAllText(dllDir + "steam_appid.txt", AppID.ToString());

      var hModule = Win32.LoadLibrary(dllDir + "steam_api.dll");
      if (hModule == IntPtr.Zero)
        return false;

      IntPtr pInit = Win32.GetProcAddress(hModule, "SteamAPI_Init");
      Init init = (Init)Marshal.GetDelegateForFunctionPointer(pInit, typeof(Init));

      if (init())
      {
        IntPtr pSteamFriends = Win32.GetProcAddress(hModule, "SteamFriends");
        SteamFriends steamFriends = (SteamFriends)Marshal.GetDelegateForFunctionPointer(pSteamFriends, typeof(SteamFriends));

        IntPtr pSetPersonaName = Win32.GetProcAddress(hModule, "SteamAPI_ISteamFriends_SetPersonaName");
        SetPersonaName setPersonaName = (SetPersonaName)Marshal.GetDelegateForFunctionPointer(pSetPersonaName, typeof(SetPersonaName));

        IntPtr pShutdown = Win32.GetProcAddress(hModule, "SteamAPI_Shutdown");
        Shutdown shutdown = (Shutdown)Marshal.GetDelegateForFunctionPointer(pShutdown, typeof(Shutdown));

        var handle = steamFriends();
        var cName = Encoding.UTF8.GetBytes(name + "\0");
        setPersonaName(handle, cName);
        ok = true;

        shutdown();
      }
      Win32.FreeLibrary(hModule);
      return ok;
    }
  }
}
