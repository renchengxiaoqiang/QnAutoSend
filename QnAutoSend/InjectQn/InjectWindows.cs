using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace QnAutoSend.InjectQn;

public class InjectWindows
{
    private static string ChatRecentHtmlFile = @"web_chat-packer/recent.html";
    private static string ImSupportUrl = @"https://iseiya.taobao.com/imsupport";
    private static string OverWriteUrl = "https://worklink.oss-cn-hangzhou.aliyuncs.com/5CFB5E11D17E63CDD8CB37B52FA6ACFD.js"; 
    
    public static void InjectJs()
    {
        try
        {
            if (!IsInject())
            {
                var resourcePath = GetResourcePath();
                if (string.IsNullOrEmpty(resourcePath)) return;
                KillProcesses();
                InjectScript(resourcePath);
            }
        }
        catch (Exception ex)
        {
            
        }
    }
    
    private static void KillProcesses()
    {
        try
        {
            var processes = Process.GetProcessesByName("AliWorkbench");
            foreach (var process in processes)
            {
                process.Kill();
            }
        }
        catch (Exception e)
        {
        }
    }
    
    private static bool IsInject()
    {
        var readyFilePath = Path.Combine(InstallPath, ".ready");
        return File.Exists(readyFilePath);
    }

    private static string installPath;
    private static string InstallPath
    {
        get
        {
            try
            {
                if (string.IsNullOrEmpty(installPath))
                {
                    var registryKey = Registry.ClassesRoot.OpenSubKey("aliim");
                    registryKey = registryKey.OpenSubKey("Shell");
                    registryKey = registryKey.OpenSubKey("Open");
                    registryKey = registryKey.OpenSubKey("Command");
                    installPath = registryKey.GetValue("").ToString();
                    var idx = installPath.IndexOf("wwcmd.exe");
                    installPath = installPath.Substring(1, idx + 8);
                    installPath = Directory.GetParent(installPath).Parent.FullName;
                }
            }
            catch (Exception e)
            {
            }

            return installPath;
        }
    }

    private static string GetResourcePath()
    {
        if(string.IsNullOrEmpty(InstallPath)) return String.Empty;
        var configPath = Path.Combine(InstallPath, "AliWorkbench.ini");
        if (!File.Exists(configPath)) return string.Empty;
        var version = ReadIniFile(configPath);
        return Path.Combine(InstallPath, version, "Resources");
    }
    
    
    private static void InjectScript(string resourcePath)
    {
        var webuiResPath = Path.Combine(resourcePath,"newWebui","webui.zip");
        var signPath = Path.Combine(resourcePath,"newWebui", "sign.json");
        using var zipFile = new ZipFile(webuiResPath);
        var entry = zipFile.GetEntry(ChatRecentHtmlFile);
        using var inputStream = zipFile.GetInputStream(entry);
        using var streamReader = new StreamReader(inputStream);
        var chatRecentHtmlContent = streamReader.ReadToEnd();
        if (!chatRecentHtmlContent.Contains(ImSupportUrl)) return;
        chatRecentHtmlContent = chatRecentHtmlContent.Replace(ImSupportUrl, OverWriteUrl);
        zipFile.BeginUpdate();
        zipFile.Add(new ZipStaticDataSource(chatRecentHtmlContent), ChatRecentHtmlFile);
        zipFile.CommitUpdate();
        var readyFilePath = Path.Combine(InstallPath, ".ready");
        File.WriteAllText(readyFilePath,"");
        if (File.Exists(signPath))
        {
            var signFi = new FileInfo(signPath);
            if (signFi.Length > 0)
            {
                signFi.Delete();
                File.Create(signPath);
            }
        }
    }
    
    private static string ReadIniFile(string path)
    {
        var ini = new IniParser(path)
        {
            Encoding = Encoding.UTF8,
            PreserveFormatting = true // 保留注释和格式
        };
        var version = ini.GetValue("Common", "Version");
        return version;
    }
}