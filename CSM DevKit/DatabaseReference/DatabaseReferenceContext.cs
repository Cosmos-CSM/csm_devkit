using CSharp_Database_Extension;
using CSharp_Database_Extension.Common.Models;

namespace DatabaseReference;

public class DatabaseReferenceContext : DatabaseBase<DatabaseReferenceContext> {

    public override string Signature => "DBREF";

    public DatabaseReferenceContext(DatabaseOptions<DatabaseReferenceContext> databaseOptions)
        : base(databaseOptions){

    }
}
