using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QnAutoSend.Db;
using QnAutoSend.Helpers;
using QnAutoSend.Windows;

namespace QnAutoSend.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private List<Item> products = new();
    [ObservableProperty] private Item selectedProduct;
    [ObservableProperty] private string keyword;

    [RelayCommand]
    private void SearchButtonClick()
    {
        using var db = new AppDbContext();
        var products = db.Items.AsQueryable();
        if (!string.IsNullOrEmpty(keyword))
        {
            products = products.Where(p=>p.ProductName.Contains(Keyword) 
                                         || p.ShopName.Contains(Keyword) 
                                         || p.ProductId.Contains(Keyword));
        }
        this.Products = products.ToList();
    }
    
    [RelayCommand]
    private async void AddButtonClick(Window window)
    {
        var vm = new EditWindowViewModel();
        var wnd = new EditWindow { DataContext = vm };
        
        if (await wnd.ShowDialog<bool>(window))
        {
            using var db = new AppDbContext();
            db.Items.Add(vm.Product);
            await db.SaveChangesAsync();
            this.Products = db.Items.ToList();
        }
    }
    
    [RelayCommand]
    private async void EditButtonClick(Window window)
    {
        using var db = new AppDbContext();
        if (selectedProduct == null) return;
        var product = db.Items.FirstOrDefault(k=>k.Id == SelectedProduct.Id);
        var vm = new EditWindowViewModel();
        vm.Product = product;
        var wnd = new EditWindow { DataContext = vm };
        
        if (await wnd.ShowDialog<bool>(window))
        {
            await db.SaveChangesAsync();
            this.Products = db.Items.ToList();
        }
    }
    
    [RelayCommand]
    private async void DeleteButtonClick()
    {
        using var db = new AppDbContext();
        var product = db.Items.FirstOrDefault(k=>k.Id == SelectedProduct.Id);
        db.Items.Remove(product);
        await db.SaveChangesAsync();
        
        this.Products = db.Items.ToList();
    }
    
    [RelayCommand]
    private async void ImportButtonClick(Window window)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择文件",
            Filters = new() { new FileDialogFilter(){ Name = "csv文件", Extensions = new List<string> { "csv"}}},
        };

        var files = await dialog.ShowAsync(window);
        var file = files.FirstOrDefault();
        if (string.IsNullOrEmpty(file)) return;
        var dats = CsvFileHelper.ReadCsvFile(file);
        if (IsFileFormatOk(dats))
        {
            var importProducts = ReadProductsFromFile(dats);
            using var db = new AppDbContext();
            var products = db.Items.ToList();
            var delProds = this.products.Where(p=>importProducts.Any(k=>k.ProductId == p.ProductId));
            foreach (var product in delProds)
            {
                db.Items.Remove(product);
            }
            db.Items.AddRange(importProducts);
            await db.SaveChangesAsync();
            Products = db.Items.ToList();
        }
    }
    
    private bool IsFileFormatOk(List<List<string>> tmplist)
    {
        bool isOk = false;
        try
        {
            var fisrtline = tmplist[0][0];
            var headerline = xToFlatString(tmplist[1]);
            isOk = (fisrtline == "最初2行的内容请勿改动（网盘发货使用下载链接，卡密字段换行导入多个）！！！" &&  headerline== "店铺名,商品名,商品ID,下载链接,卡密");
        }
        catch (Exception e)
        {
        }
        return isOk;
    }
    
    public string xToFlatString<T>(IEnumerable<T> list, string seperator = ",", bool printNull = true)
    {
        if (list == null) return string.Empty;

        var sb = new StringBuilder();
        foreach (T t in list)
        {
            var text = (t != null) ? t.ToString() : null;
            sb.Append(text ?? (printNull ? "null" : ""));
            sb.Append(seperator);
        }
        int length = seperator.Length;
        if (sb.Length >= length)
        {
            sb.Remove(sb.Length - length, length);
        }
        return sb.ToString();
    }

    protected List<Item> ReadProductsFromFile(List<List<string>> dlist)
    {
        var products = new List<Item>();
        for (int i = 2; i < dlist.Count; i++)
        {
            while (dlist[i].Count < 5)
            {
                dlist[i].Add("");
            }

            products.Add(new Item
            {
                ShopName = dlist[i][0],
                ProductName = dlist[i][1],
                ProductId = dlist[i][2],
                DownloadUrl = dlist[i][3],
                ActivateCodes = ((dlist[i].Count < 5 || string.IsNullOrEmpty(dlist[i][4])) ? null : dlist[i][4]),
            });
        }

        return products;
    }
}