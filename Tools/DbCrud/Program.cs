using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentManagement.API.Data;
using StudentManagement.API.Models;

using System.IO;

// Resolve StudentManagement.API folder by trying several candidate paths
var candidates = new[] {
    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "StudentManagement.API")),
    Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "StudentManagement.API")),
    @"c:\\Users\\arunk\\Desktop\\Student_Management_System\\StudentManagement.API"
};

string apiPath = candidates.FirstOrDefault(Directory.Exists);
if (apiPath == null)
{
    Console.WriteLine("Could not find StudentManagement.API folder. Candidates tried:");
    foreach (var c in candidates) Console.WriteLine(c);
    return;
}

var config = new ConfigurationBuilder()
    .SetBasePath(apiPath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var conn = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(conn))
{
    Console.WriteLine("No connection string found.");
    return;
}

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql(conn)
    .Options;

using var db = new ApplicationDbContext(options);

Console.WriteLine("Creating a student (CharlieTest)...");
var student = new Student { Name = "CharlieTest", RollNumber = Guid.NewGuid().ToString().Substring(0,6), Address = "CLI Addr", Grade = "C" };
db.Students.Add(student);
db.SaveChanges();
Console.WriteLine($"Created student id={student.Id}");

Console.WriteLine("Reading students from DB:");
foreach (var s in db.Students.ToList())
{
    Console.WriteLine($"{s.Id}: {s.Name} ({s.RollNumber}) - {s.Grade}");
}

Console.WriteLine("Updating the created student grade to B...");
student.Grade = "B";
db.SaveChanges();

var fresh = db.Students.Find(student.Id);
Console.WriteLine($"After update: {fresh.Id}: {fresh.Name} - {fresh.Grade}");

Console.WriteLine("Deleting the created student...");
db.Students.Remove(fresh!);
db.SaveChanges();

Console.WriteLine("Final students list:");
foreach (var s in db.Students.ToList())
{
    Console.WriteLine($"{s.Id}: {s.Name} ({s.RollNumber}) - {s.Grade}");
}

Console.WriteLine("Done.");
