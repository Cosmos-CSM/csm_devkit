using Microsoft.EntityFrameworkCore.Design;
using CSharp_Database_Extension.Common.Enums;
using CSharp_Database_Extension.Common.Models;

namespace DatabaseReference.PostgresSQL;

/// Auto-Generated file by CSM DevKit auto migrations generation per provider (11/2/2026 5:40:44 PM)[UTC]. 

public sealed class DesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<DatabaseReferenceContext>
{
    public DatabaseReferenceContext CreateDbContext(string[] args)
    {
        DatabaseOptions<DatabaseReferenceContext> options = new() {
            Provider = DatabaseProviders.PostgresSQL,
            Migrations = GetType().Assembly
        };

        return new DatabaseReferenceContext(options);
    }
}