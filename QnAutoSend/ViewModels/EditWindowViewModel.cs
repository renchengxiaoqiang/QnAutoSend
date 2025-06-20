using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QnAutoSend.Db;

namespace QnAutoSend.ViewModels;

public partial class EditWindowViewModel : ObservableObject
{
    [ObservableProperty] private Item product = new();

    [RelayCommand]
    private void SaveButtonClick(Window window)
    {
        window?.Close(true);
    }
    
    [RelayCommand]
    private void CancelButtonClick(Window window) => window?.Close(false);
    
}