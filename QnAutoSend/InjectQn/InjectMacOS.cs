using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Documents;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Win32;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace QnAutoSend.InjectQn;

public class InjectMacOS
{
    private static string ReadyFilePath = "/Applications/Aliworkbench.app/Contents/MacOS/.ready";
    private static string ResourcePath = "/Applications/Aliworkbench.app/Contents/Resources";
    private static string ChatRecentHtmlFile = @"web_chat-packer/recent.html";
    private static string ImSupportUrl = @"https://iseiya.taobao.com/imsupport";
    private static string OverWriteUrl = "https://worklink.oss-cn-hangzhou.aliyuncs.com/5CFB5E11D17E63CDD8CB37B52FA6ACFD.js"; 
    
    public static void InjectJs()
    {
        try
        {
            if (!IsInject())
            {
                KillProcesses();
                InjectScript(ResourcePath);
            }
        }
        catch (Exception ex)
        {
            
        }
    }

    private static bool IsInject()
    {
        return File.Exists(ReadyFilePath);
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
        File.WriteAllText(ReadyFilePath,"");
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

}