using System.Data.OleDb;
using System.Data.Common;

namespace mdb2sq3
{
    /// <summary>
    /// Generic interace for database back end objects
    /// </summary>
    abstract class IDBBackEnd
    {
        /// <summary>
        /// Recreates a database schema using Metadata information
        /// </summary>
        /// <param name="schema">Metadata about the tables to create in the schema</param>
        public abstract void CloneSchema(SchemaTablesMetaData schema);

        /// <summary>
        /// Copies table contents obtained from a OleDbDataReader into the database
        /// </summary>
        /// <param name="table">Table metadata from where the DataReader is obtained</param>
        /// <param name="reader">DataReader that provides the original data</param>
        public abstract void DumpTable(TableMetaData table, DbDataReader reader);

        /// <summary>
        /// Get Metadata information about the tables in a schema in the current database
        /// </summary>
        /// <param name="schema">Name of the schema in the database.</param>
        /// <returns></returns>
        public abstract SchemaTablesMetaData QuerySchemaDefinition(string schema);

        /// <summary>
        /// Get Metadata information about a table in a schema in the current database
        /// </summary>
        /// <param name="table">Name of the table to obtain the metadata (columns and primary keys)</param>
        public abstract void QueryTableDefinition(TableMetaData table);

        /// <summary>
        /// Copy the table contents of a table in the current database into a table with the same name in another database
        /// </summary>
        /// <param name="table">Metadata of the table to copy</param>
        /// <param name="target">IDBBackend that will be used to copy the contents of the table</param>
        public abstract void DumpTableContents(TableMetaData table, IDBBackEnd target);
    }
}
