using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ExtraQL
{
  class Steamworks : IDisposable
  {
    #region Interop

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    private extern static bool SteamAPI_Init();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static bool SteamAPI_IsSteamRunning();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static IntPtr SteamFriends();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static void SteamAPI_ISteamFriends_SetPersonaName(IntPtr handle, byte[] utf8name);

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static bool SteamAPI_Shutdown();

    #endregion

    // using the QL Dedicated Linux Server app-id so it won't block the QL client (282440) from starting
    public int AppID { get; set; } = 349090;

    private bool initialized;

    #region Dispose()

    ~Steamworks()
    {
      this.Dispose(false);
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
      this.Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (initialized)
      {
        SteamAPI_Shutdown();
        initialized = false;
      }
    }
    #endregion

    public bool IsSteamRunning()
    {
      return SteamAPI_IsSteamRunning();
    }

    private bool EnsureInit()
    {
      if (this.initialized)
      {
        if (SteamFriends() != IntPtr.Zero)
          return true;
        SteamAPI_Shutdown();
      }

      return this.initialized = SteamAPI_Init();
    }

    public bool SetName(string name)
    {
      string dllDir = Application.StartupPath + "\\";
      File.WriteAllText(dllDir + "steam_appid.txt", AppID.ToString());

      if (!EnsureInit())
        return false;
      
      var handle = SteamFriends();
      if (handle == IntPtr.Zero)
        return false;
      var cName = Encoding.UTF8.GetBytes(name + "\0");
      SteamAPI_ISteamFriends_SetPersonaName(handle, cName);
      return true;
    }
  }
}
