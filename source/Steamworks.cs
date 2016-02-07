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
    private extern static bool SteamAPI_Shutdown();



    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static IntPtr SteamUser();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern ulong SteamAPI_ISteamUser_GetSteamID(IntPtr instancePtr);



    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static IntPtr SteamFriends();

    [DllImport("steam_api.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private extern static void SteamAPI_ISteamFriends_SetPersonaName(IntPtr handle, byte[] utf8name);

    #endregion

    // using the QL Dedicated Linux Server app-id so it won't block the QL client (282440) from starting
    public ulong AppID { get; set; } = 349090;

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

      string dllDir = Application.StartupPath + "\\";

      // try the configured AppID
      File.WriteAllText(dllDir + "steam_appid.txt", AppID.ToString());
      this.initialized = SteamAPI_Init();

      // fallback to Steamworks SDK Redist AppID
      if (!this.initialized && AppID != 1007)
      {
        File.WriteAllText(dllDir + "steam_appid.txt", "1007");
        this.initialized = SteamAPI_Init();
      }

      return this.initialized;
    }

    public bool SetName(string name)
    {
      if (!EnsureInit())
        return false;
      
      var handle = SteamFriends();
      if (handle == IntPtr.Zero)
        return false;
      var cName = Encoding.UTF8.GetBytes(name + "\0");
      SteamAPI_ISteamFriends_SetPersonaName(handle, cName);
      return true;
    }

    public ulong GetUserID()
    {
      if (!EnsureInit())
        return 0;

      var handle = SteamUser();
      return handle == IntPtr.Zero ? 0 : SteamAPI_ISteamUser_GetSteamID(handle);
    }

  }
}
