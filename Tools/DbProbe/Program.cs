using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StudentManagement.API.Data;
using StudentManagement.API.Models;

var argsList = args.ToList();

// Resolve API folder
var apiPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "StudentManagement.API"));
if (!Directory.Exists(apiPath))
{
    Console.WriteLine("Could not find StudentManagement.API folder.");
    return;
}

var config = new ConfigurationBuilder()
    .SetBasePath(apiPath)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var conn = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(conn))
{
    Console.WriteLine("No DefaultConnection found in appsettings.json");
    return;
}

var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(conn).Options;

using var db = new ApplicationDbContext(options);

if (argsList.Count == 0 || argsList[0] == "help")
{
    Console.WriteLine("Usage: DbProbe <command> [args]\nCommands:\n  list-students\n  list-users\n  create-user <username> <password>\n  update-user <id> <username> <password>\n  delete-user <id>\n  create-student <name> <roll> <address> <grade>\n  update-student <id> <name> <roll> <address> <grade>\n  delete-student <id>");
    return;
}

string cmd = argsList[0];
try
{
    switch (cmd)
    {
        case "list-students":
            foreach (var s in db.Students.ToList()) Console.WriteLine($"{s.Id}: {s.Name} ({s.RollNumber}) - {s.Grade}");
            break;
        case "list-users":
            foreach (var u in db.Users.ToList()) Console.WriteLine($"{u.Id}: {u.Username}");
            break;
        case "create-user":
            {
                var username = argsList[1]; var password = argsList[2];
                var user = new User { Username = username, PasswordHash = BCrypt.Net.BCrypt.HashPassword(password) };
                db.Users.Add(user); db.SaveChanges(); Console.WriteLine($"Created user {user.Id}");
            }
            break;
        case "update-user":
            {
                var id = int.Parse(argsList[1]); var username = argsList[2]; var password = argsList[3];
                var u = db.Users.Find(id); if (u == null) { Console.WriteLine("User not found"); break; }
                u.Username = username; u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password); db.SaveChanges(); Console.WriteLine("Updated");
            }
            break;
        case "delete-user":
            {
                var id = int.Parse(argsList[1]); var u = db.Users.Find(id); if (u == null) { Console.WriteLine("User not found"); break; }
                db.Users.Remove(u); db.SaveChanges(); Console.WriteLine("Deleted");
            }
            break;
        case "create-student":
            {
                var name = argsList[1]; var roll = argsList[2]; var addr = argsList[3]; var grade = argsList[4];
                var s = new Student { Name = name, RollNumber = roll, Address = addr, Grade = grade };
                db.Students.Add(s); db.SaveChanges(); Console.WriteLine($"Created student {s.Id}");
            }
            break;
        case "update-student":
            {
                var id = int.Parse(argsList[1]); var name = argsList[2]; var roll = argsList[3]; var addr = argsList[4]; var grade = argsList[5];
                var s = db.Students.Find(id); if (s == null) { Console.WriteLine("Student not found"); break; }
                s.Name = name; s.RollNumber = roll; s.Address = addr; s.Grade = grade; db.SaveChanges(); Console.WriteLine("Updated");
            }
            break;
        case "delete-student":
            {
                var id = int.Parse(argsList[1]); var s = db.Students.Find(id); if (s == null) { Console.WriteLine("Student not found"); break; }
                db.Students.Remove(s); db.SaveChanges(); Console.WriteLine("Deleted");
            }
            break;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
