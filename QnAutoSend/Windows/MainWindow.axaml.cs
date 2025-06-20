using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QnAutoSend.CDP;
using QnAutoSend.InjectQn;
using QnAutoSend.ViewModels;

namespace QnAutoSend.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
        DataContext = new MainWindowViewModel();
    }

    public void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (OperatingSystem.IsWindows())
        {
            InjectWindows.InjectJs();
        }

        if (OperatingSystem.IsMacOS())
        {
            InjectMacOS.InjectJs();
        }
        MyWebSocketServer.WSocketSvrInst.Start();
    }

    private void BtnSearch_OnClick(object? sender, RoutedEventArgs e)
    {
        var set = QN.QNSet;
    }
}