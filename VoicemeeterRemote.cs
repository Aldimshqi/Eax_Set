using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

public static class VoicemeeterRemote
{
    private const string DLL_NAME = "VoicemeeterRemote.dll";

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool SetDllDirectory(string lpPathName);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
    private static extern int VBVMR_Login();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
    private static extern int VBVMR_Logout();

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private static extern int VBVMR_SetParameterFloat(string szParamName, float value);

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    private static extern int VBVMR_GetParameterFloat(string szParamName, ref float value);

    private static bool isConnected = false;
    private static bool dllDirectorySet = false;

    public static int LastLoginResult { get; private set; } = -99;
    public static string LastError { get; private set; } = "";
    public static string FoundVoicemeeterPath { get; private set; } = "";

    private static string FindVoicemeeterFolder()
    {
        string[] registryPaths = new string[]
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\VB:Voicemeeter {17359A74-1236-5467}"
        };

        foreach (string regPath in registryPaths)
        {
            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath))
                {
                    if (key != null)
                    {
                        object uninstallString = key.GetValue("UninstallString");
                        if (uninstallString != null)
                        {
                            string exePath = uninstallString.ToString().Trim('"');
                            string folder = Path.GetDirectoryName(exePath);
                            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
                            {
                                return folder;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        string[] commonPaths = new string[]
        {
            @"C:\Program Files (x86)\VB\Voicemeeter",
            @"C:\Program Files\VB\Voicemeeter"
        };

        foreach (string p in commonPaths)
        {
            if (Directory.Exists(p) && File.Exists(Path.Combine(p, DLL_NAME)))
            {
                return p;
            }
        }

        return null;
    }

    public static bool Connect()
    {
        try
        {
            if (!dllDirectorySet)
            {
                string folder = FindVoicemeeterFolder();
                FoundVoicemeeterPath = folder ?? "(not found)";

                if (folder != null)
                {
                    SetDllDirectory(folder);
                }
                dllDirectorySet = true;
            }

            int result = VBVMR_Login();
            LastLoginResult = result;
            isConnected = (result == 0 || result == 1);
            return isConnected;
        }
        catch (Exception ex)
        {
            LastError = ex.GetType().Name + ": " + ex.Message;
            isConnected = false;
            return false;
        }
    }

    public static void Disconnect()
    {
        if (isConnected)
        {
            VBVMR_Logout();
            isConnected = false;
        }
    }

    public static int LastSetResult { get; private set; } = -99;

    public static bool SetReverb(int stripIndex, float reverbAmount)
    {
        if (!isConnected) return false;

        try
        {
            string paramName = $"Strip[{stripIndex}].Reverb";
            int result = VBVMR_SetParameterFloat(paramName, reverbAmount);
            LastSetResult = result;
            return result == 0;
        }
        catch (Exception ex)
        {
            LastError = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    public static bool IsConnected => isConnected;
}