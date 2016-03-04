using System;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Reflection;

namespace mdb2sq3
{
    /// <summary>
    /// MSAccess database Backend, just to work directly with this type of database.
    /// </summary>
    class MSAccessBackend: IDBBackEnd
    {
        string connectionString;
        //OleDbConnection connector;

        /// <summary>
        /// Default constructor. When used, it tries to open a db named initial.mdb located in the same folder where the assembly is.
        /// </summary>
        public MSAccessBackend()
        {
            String CallingAssembly = Assembly.GetCallingAssembly().Location;
            connectionString = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}\\initial.mdb;Persist Security Info=False;", System.IO.Path.GetDirectoryName(CallingAssembly));
        }

        /// <summary>
        /// Constructor to open up a msaccess db located in the given parameter
        /// </summary>
        /// <param name="sourceDB">Path to a file containing a MsAccess database</param>
        public MSAccessBackend(string sourceDB)
        {
            connectionString = String.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False;", sourceDB);
        }

        /// <summary>
        /// Get Metadata information about the tables in a schema in the current database
        /// </summary>
        /// <param name="schema">Name of the schema in the database.</param>
        /// <returns></returns>
        public override SchemaTablesMetaData QuerySchemaDefinition(string schema)
        {
            SchemaTablesMetaData result = new SchemaTablesMetaData();
            result.schemaName = schema;
            try
            {
                using (OleDbConnection connector = new OleDbConnection(connectionString))
                {
                    connector.Open();
                    using (DataTable dt = connector.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, schema, null, "TABLE" }))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            TableMetaData table = new TableMetaData();
                            table.tableName = row[2].ToString();
                            result.AddTable(table);
                        }
                    }
                    connector.Close();
                }
            }
            catch (OleDbException ex)
            {
                Console.Out.WriteLine("Exception fetching schema metadata: {0}", ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Get Metadata information about a table in a schema in the current database
        /// </summary>
        /// <param name="table">Name of the table to obtain the metadata (columns and primary keys)</param>
        public override void QueryTableDefinition(TableMetaData table)
        {
            try
            {
                using (OleDbConnection connector = new OleDbConnection(connectionString))
                {
                    connector.Open();
                    using (DataTable dt = connector.GetOleDbSchemaTable(OleDbSchemaGuid.Columns, new object[] { null, null, table.tableName, null }))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            ColumnMetaData metadata = new ColumnMetaData();
                            metadata.columnName = row["COLUMN_NAME"].ToString();
                            metadata.columnDescription = row["DESCRIPTION"].ToString(); ;
                            metadata.ordinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"].ToString());
                            if (row["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value)
                                metadata.maxCharSize = Convert.ToInt32(row["CHARACTER_MAXIMUM_LENGTH"].ToString());
                            if (row["NUMERIC_PRECISION"] != DBNull.Value)
                                metadata.numericPrecision = Convert.ToInt32(row["NUMERIC_PRECISION"].ToString());
                            if (row["NUMERIC_SCALE"] != DBNull.Value)
                                metadata.numericScale = Convert.ToInt32(row["NUMERIC_SCALE"].ToString());
                            if (row["DATETIME_PRECISION"] != DBNull.Value)
                                metadata.datetimePrecision = Convert.ToInt32(row["DATETIME_PRECISION"].ToString());
                            metadata.hasDefault = Convert.ToBoolean(row["COLUMN_HASDEFAULT"].ToString());
                            metadata.defaultValue = row["COLUMN_DEFAULT"].ToString(); ;
                            metadata.columnType = ((OleDbType)row["DATA_TYPE"]);
                            metadata.isNullable = Convert.ToBoolean(row["IS_NULLABLE"].ToString());

                            table.AddColumn(metadata);
                        }
                    }
                    using (DataTable dt = connector.GetOleDbSchemaTable(OleDbSchemaGuid.Primary_Keys, new object[] { null, null, table.tableName }))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            // Find Columns by name
                            string isPK = row["PK_NAME"].ToString();
                            string colPK = row["COLUMN_NAME"].ToString();
                            table.FindColumn(colPK).isPrimaryKey = true;
                        }
                    }
                    using (DataTable dt = connector.GetOleDbSchemaTable(OleDbSchemaGuid.Foreign_Keys, new object[] { null, null, null, null, null, table.tableName }))
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            // Find Columns by name
                            string localColumn = row["FK_COLUMN_NAME"].ToString();
                            ColumnMetaData column = table.FindColumn(localColumn);
                            column.hasForeignKey = true;
                            column.fkColumn = row["PK_COLUMN_NAME"].ToString();
                            column.fkTable = row["PK_TABLE_NAME"].ToString();
                        }
                    }
                    connector.Close();
                }
            }
            catch (OleDbException ex)
            {
                Console.Out.WriteLine("Exception fetching metadata for table {0} : {1}", table.tableName, ex.Message);
            }
        }

        /// <summary>
        /// Copy the table contents of a table in the current database into a table with the same name in an SQLite DB
        /// </summary>
        /// <param name="table">Metadata of the table to copy</param>
        /// <param name="target">SQLite backend that will be used to copy the contents of the table</param>
        public override void DumpTableContents(TableMetaData table, IDBBackEnd target)
        {
            try
            {
                using (OleDbConnection connector = new OleDbConnection(connectionString))
                {
                    connector.Open();
                    using (OleDbCommand cmd = connector.CreateCommand())
                    {
                        cmd.CommandText = String.Format("SELECT * FROM [{0}]", table.tableName);
                        using (OleDbDataReader db = cmd.ExecuteReader())
                        {
                            target.DumpTable(table, db);
                        }
                    }
                    connector.Close();
                }
            }
            catch (OleDbException ex)
            {
                Console.Out.WriteLine("MSAccess Exception dumping table {0} : {1}", table.tableName, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        public override void CloneSchema(SchemaTablesMetaData schema)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="reader"></param>
        public override void DumpTable(TableMetaData table, DbDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
