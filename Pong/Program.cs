using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

List<string> files = new List<string>();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/ping", async() => "pong!");

async Task UploadFile(HttpContext context)
{
    var file = context.Request.Form["file"];
    files.Add(file);
}

app.MapGet("/download", async(context) =>
{
    string name = context.Request.Query["FileName"];
    if (!System.IO.File.Exists(@$".\Repository\{name}"))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync($"File \"{name}\" not found");
    }
    else
    {
        context.Response.Headers.ContentDisposition = $"attachment; filename={name}";
        await context.Response.SendFileAsync(@$".\Repository\{name}");
    }
});

app.MapGet("/files", async(context) =>
{
    string[] temp = Directory.GetFiles(@".\Repository");
    string[] allfiles = new string[temp.Count() + files.Count];
    int i = 0;
    foreach(string file in temp)
    {
        allfiles[i] = file;
        i++;
    }
    foreach (string file in files)
    {
        allfiles[i] = file;
        i++;
    }
    foreach (string filename in allfiles)
    {
        await context.Response.WriteAsync(Path.GetFileName(filename) + "\n");
    }
});

app.MapPost("/rename", async (context) =>
{
    string oldName = context.Request.Form["oldName"];
    Console.WriteLine("!!!FILE RENAMED!!!");
    Console.WriteLine("old name:" + oldName);
    string newName = context.Request.Form["newName"];
    Console.WriteLine("new name:" + newName);
    try
    {
        System.IO.File.Move(@$".\Repository\" + oldName, @$".\Repository\" + newName);
    }catch(Exception e)
    {
        await context.Response.WriteAsync(e.Message);
    }
});
app.MapPost("/upload", UploadFile);
app.Run();
