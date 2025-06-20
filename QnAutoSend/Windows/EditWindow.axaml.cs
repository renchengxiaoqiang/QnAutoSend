using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using QnAutoSend.CDP;
using QnAutoSend.InjectQn;
using QnAutoSend.ViewModels;

namespace QnAutoSend.Windows;

public partial class EditWindow : Window
{
    public  EditWindow()
    {
        InitializeComponent();
        Loaded += EditWindow_Loaded;
        DataContext = new EditWindowViewModel();
    }

    public void EditWindow_Loaded(object? sender, RoutedEventArgs e)
    {
    }
}