using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ExtraQL
{
  class Steamworks
  {
    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    private extern static bool SteamAPI_Init();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static IntPtr SteamFriends();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static void SteamAPI_ISteamFriends_SetPersonaName(IntPtr handle, byte[] utf8name);

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static bool SteamAPI_Shutdown();

    // using the QL Dedicated Linux Server app-id so it won't block the QL client (282440) from starting
    public static int AppID { get; set; } = 349090;

    public static bool SetName(string name)
    {
      string dllDir = Application.StartupPath + "\\";
      File.WriteAllText(dllDir + "steam_appid.txt", AppID.ToString());
            
      if (SteamAPI_Init())
      {
        try
        {
          var handle = SteamFriends();
          if (handle == IntPtr.Zero)
            return false;
          var cName = Encoding.UTF8.GetBytes(name + "\0");
          SteamAPI_ISteamFriends_SetPersonaName(handle, cName);
          return true;
        }
        finally
        {
          SteamAPI_Shutdown();
        }
      }
      return false;
    }
  }
}
