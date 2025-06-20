using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace QnAutoSend.Db;


/*
    dotnet add package Microsoft.EntityFrameworkCore
    dotnet add package Microsoft.EntityFrameworkCore.Design
    dotnet ef migrations add "initial migration"
    dotnet ef database update
*/

class AppDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }
    public DbSet<Record> Records { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=dat.db;Cache=Shared");
    }
}


public class Item
{    
    public int Id { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string ActivateCodes { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}

public class Record
{    
    public int Id { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string SellerNick { get; set; } = string.Empty;
    public string BuyerNick { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
