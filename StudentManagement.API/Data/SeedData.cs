using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace StudentManagement.API.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(ApplicationDbContext db, IConfiguration config)
    {
        if (await db.Students.AnyAsync() || await db.Users.AnyAsync()) return;

        db.Students.AddRange(
            new Models.Student { Name = "Alice", RollNumber = "R001", Address = "111 Main St", Grade = "A" },
            new Models.Student { Name = "Bob", RollNumber = "R002", Address = "222 Oak Ave", Grade = "B" }
        );

        // Add a default dev user if configured
        var addDevUser = config.GetValue<bool>("Seed:CreateDevUser", true);
        if (addDevUser)
        {
            var pw = BCrypt.Net.BCrypt.HashPassword("devpass");
            db.Users.Add(new Models.User { Username = "devuser", PasswordHash = pw });
        }

        await db.SaveChangesAsync();
    }
}
