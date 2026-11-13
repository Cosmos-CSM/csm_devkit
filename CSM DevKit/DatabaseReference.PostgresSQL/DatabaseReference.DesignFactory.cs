using CSharp_Database_Extension.Abstractions.Bases;
using CSharp_Database_Extension.Common.Enums;
using CSharp_Database_Extension.Common.Models;

namespace DatabaseReference.PostgresSQL;

/// Auto-Generated file by CSM DevKit auto migrations generation per provider (11/13/2026 1:41:11 AM)[UTC]. 

public sealed class DesignTimeDbContextFactory
    : DatabaseDesignerBase<DatabaseReferenceContext> {

    public DesignTimeDbContextFactory() {
    }

    override public DatabaseReferenceContext CreateDbContext(string[] args) {
        DatabaseOptions<DatabaseReferenceContext> options = new() {
            Provider = DatabaseProviders.PostgresSQL,
            Migrations = GetType().Assembly
        };

        return new DatabaseReferenceContext(options);
    }
}