using System.Windows;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ensure database is created
            using (var context = new LibraryDbContext())
            {
                context.Database.EnsureCreated();
            }
        }
    }
}
