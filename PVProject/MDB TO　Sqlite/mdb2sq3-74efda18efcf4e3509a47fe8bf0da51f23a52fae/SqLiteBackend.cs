using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Reflection;
using System.Text;

namespace mdb2sq3
{
    /// <summary>
    /// SQLite database Backend, just to work directly with this type of database.
    /// </summary>
    class SQLiteBackend: IDBBackEnd
    {
        private string connectionString = null;

        /// <summary>
        /// Default constructor. When used, it will try to open a db file named "converted.db" in the folder where the assembly is located.
        /// </summary>
        public SQLiteBackend()
        {
            String CallingAssembly = Assembly.GetCallingAssembly().Location;
            connectionString = String.Format("Data Source={0}/converted.db", System.IO.Path.GetDirectoryName(CallingAssembly));
        }

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="sourceDB">File path indicating where the SQLite database is stored</param>
        public SQLiteBackend(string sourceDB)
        {
            connectionString = String.Format("Data Source={0}", sourceDB);
        }

        /// <summary>
        /// Recreates a database schema using Metadata information
        /// </summary>
        /// <param name="schema">Metadata about the tables to create in the schema</param>
        public override void CloneSchema(SchemaTablesMetaData schema)
        {
            try
            {
                using (SQLiteConnection connector = new SQLiteConnection(connectionString))
                {
                    connector.Open();
                    foreach (TableMetaData table in schema.tables)
                    {
                        using (SQLiteCommand Cmd = new SQLiteCommand())
                        {
                            Cmd.CommandText = CreateTableDML(table);
                            if (CommandLineParametersHelper.verbose)
                            {
                                Console.Out.WriteLine("{0}", Cmd.CommandText);
                            }
                            Cmd.Connection = connector;
                            Cmd.ExecuteNonQuery();
                        }
                    }
                    connector.Close();
                }
            }
            catch (SQLiteException ex)
            {
                Console.Out.WriteLine("Exception cloning schema: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Method used to find out if a column contains data that is susceptible of being encased into quotes (string literals).
        /// </summary>
        /// <param name="column">Column metadata information</param>
        /// <returns>true if the column metadata type indicates is one of a literal kind</returns>
        private bool IsLiteralType(ColumnMetaData column)
        {
            switch (column.columnType)
            {
                case OleDbType.Boolean: 	//A Boolean value (DBTYPE_BOOL). This maps to Boolean.
                case OleDbType.Date: //	Date data, stored as a double (DBTYPE_DATE). The whole portion is the number of days since December 30, 1899, and the fractional portion is a fraction of a day. This maps to DateTime.
                case OleDbType.DBDate://	Date data in the format yyyymmdd (DBTYPE_DBDATE). This maps to DateTime.
                case OleDbType.DBTime://	Time data in the format hhmmss (DBTYPE_DBTIME). This maps to TimeSpan.
                case OleDbType.DBTimeStamp://	Data and time data in the format yyyymmddhhmmss (DBTYPE_DBTIMESTAMP). This maps to DateTime.

                case OleDbType.Binary: 	//A stream of binary data (DBTYPE_BYTES). This maps to an Array of type Byte.
                case OleDbType.LongVarBinary://	A long binary value (OleDbParameter only). This maps to an Array of type Byte.
                case OleDbType.VarBinary://	A variable-length stream of binary data (OleDbParameter only). This maps to an Array of type Byte.

                case OleDbType.BSTR: 	//A null-terminated character string of Unicode characters (DBTYPE_BSTR). This maps to String.
                case OleDbType.Char: 	//A character string (DBTYPE_STR). This maps to String.
                case OleDbType.LongVarChar://	A long string value (OleDbParameter only). This maps to String.
                case OleDbType.LongVarWChar://	A long null-terminated Unicode string value (OleDbParameter only). This maps to String.
                case OleDbType.VarChar://	A variable-length stream of non-Unicode characters (OleDbParameter only). This maps to String.
                case OleDbType.VarWChar://	A variable-length, null-terminated stream of Unicode characters (OleDbParameter only). This maps to String.
                case OleDbType.WChar: //                
                case OleDbType.Guid://	A globally unique identifier (or GUID) (DBTYPE_GUID). This maps to Guid.
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Conversion function, gets a table column metadata, returns a string indicating the SQLite type for the Column Data Type
        /// </summary>
        /// <param name="column">A Table Column Metadata Info</param>
        /// <returns>String indicating the data type for the same column in SQLite</returns>
        private string ConvertDbTypeToSQLiteType(ColumnMetaData column)
        {
            switch (column.columnType)
            {
                case OleDbType.Boolean: 	//A Boolean value (DBTYPE_BOOL). This maps to Boolean.
                    return "BOOLEAN";

                case OleDbType.BigInt: 	//A 64-bit signed integer (DBTYPE_I8). This maps to Int64.
                case OleDbType.Filetime ://	A 64-bit unsigned integer representing the number of 100-nanosecond intervals since January 1, 1601 (DBTYPE_FILETIME). This maps to DateTime.
                case OleDbType.Integer ://	A 32-bit signed integer (DBTYPE_I4). This maps to Int32.
                case OleDbType.SmallInt://	A 16-bit signed integer (DBTYPE_I2). This maps to Int16.
                case OleDbType.TinyInt://	A 8-bit signed integer (DBTYPE_I1). This maps to SByte.
                case OleDbType.UnsignedBigInt://	A 64-bit unsigned integer (DBTYPE_UI8). This maps to UInt64.
                case OleDbType.UnsignedInt://	A 32-bit unsigned integer (DBTYPE_UI4). This maps to UInt32.
                case OleDbType.UnsignedSmallInt://	A 16-bit unsigned integer (DBTYPE_UI2). This maps to UInt16.
                case OleDbType.UnsignedTinyInt://	A 8-bit unsigned integer (DBTYPE_UI1). This maps to Byte.
                    return "INTEGER";

                case OleDbType.Currency: 	//A currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63 -1 (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of a currency unit (DBTYPE_CY). This maps to Decimal.
                    // Dudo si currency deberia ser text...
                case OleDbType.Decimal://	A fixed precision and scale numeric value between -10 38 -1 and 10 38 -1 (DBTYPE_DECIMAL). This maps to Decimal.
                case OleDbType.Double://	A floating-point number within the range of -1.79E +308 through 1.79E +308 (DBTYPE_R8). This maps to Double.
                case OleDbType.Numeric://	An exact numeric value with a fixed precision and scale (DBTYPE_NUMERIC). This maps to Decimal.
                case OleDbType.Single://	A floating-point number within the range of -3.40E +38 through 3.40E +38 (DBTYPE_R4). This maps to Single.
                case OleDbType.VarNumeric://	A variable-length numeric value (OleDbParameter only). This maps to Decimal.
                    return "DOUBLE";

                case OleDbType.Binary: 	//A stream of binary data (DBTYPE_BYTES). This maps to an Array of type Byte.
                case OleDbType.LongVarBinary://	A long binary value (OleDbParameter only). This maps to an Array of type Byte.
                case OleDbType.VarBinary://	A variable-length stream of binary data (OleDbParameter only). This maps to an Array of type Byte.
                    return "BLOB";

                case OleDbType.BSTR: 	//A null-terminated character string of Unicode characters (DBTYPE_BSTR). This maps to String.
                case OleDbType.Char: 	//A character string (DBTYPE_STR). This maps to String.
                case OleDbType.LongVarChar ://	A long string value (OleDbParameter only). This maps to String.
                case OleDbType.LongVarWChar ://	A long null-terminated Unicode string value (OleDbParameter only). This maps to String.
                case OleDbType.VarChar://	A variable-length stream of non-Unicode characters (OleDbParameter only). This maps to String.
                case OleDbType.VarWChar://	A variable-length, null-terminated stream of Unicode characters (OleDbParameter only). This maps to String.
                case OleDbType.WChar: //                
                case OleDbType.Guid://	A globally unique identifier (or GUID) (DBTYPE_GUID). This maps to Guid.
                    return "TEXT";

                case OleDbType.Date: //	Date data, stored as a double (DBTYPE_DATE). The whole portion is the number of days since December 30, 1899, and the fractional portion is a fraction of a day. This maps to DateTime.
                case OleDbType.DBDate ://	Date data in the format yyyymmdd (DBTYPE_DBDATE). This maps to DateTime.
                case OleDbType.DBTime ://	Time data in the format hhmmss (DBTYPE_DBTIME). This maps to TimeSpan.
                case OleDbType.DBTimeStamp ://	Data and time data in the format yyyymmddhhmmss (DBTYPE_DBTIMESTAMP). This maps to DateTime.
                    return "DATETIME";

                default:
                    throw new Exception("Unhandled MS Acess datatype: " + column.columnType);
            }
        }

        /// <summary>
        /// Creates the text of the SQL command to create a table in SQLite
        /// </summary>
        /// <param name="table">Table Metadata to create</param>
        /// <returns>The DML sentence (SQL) to create the given table in a SQLite schema</returns>
        private string CreateTableDML (TableMetaData table) 
        {
            StringBuilder stmtBuilder = new StringBuilder();
            string primary_keys_string = string.Empty;

            stmtBuilder.Append("CREATE TABLE " + SchemaTablesMetaData.EscapeIdentifier(table.tableName) + " (");
        
            for (int i = 0 ; i < table.columns.Count; i++)
            {
                ColumnMetaData column = table.columns[i];
                stmtBuilder.Append(SchemaTablesMetaData.EscapeIdentifier(column.columnName));
                stmtBuilder.Append(" ");
                stmtBuilder.Append(ConvertDbTypeToSQLiteType(column));
                if (!column.isNullable)
                {
                    stmtBuilder.Append(" NOT NULL");
                }
                if (column.hasDefault)
                {
                    stmtBuilder.Append(" DEFAULT ");
                    if (IsLiteralType(column))
                    {
                        stmtBuilder.Append(SchemaTablesMetaData.EscapeIdentifier(column.defaultValue));
                    }
                    else
                    {
                        stmtBuilder.Append(column.defaultValue);
                    }
                }
                if (column.hasForeignKey)
                {
                    stmtBuilder.Append(String.Format(" REFERENCES {0} ({1})", SchemaTablesMetaData.EscapeIdentifier(column.fkTable), SchemaTablesMetaData.EscapeIdentifier(column.fkColumn)));
                }
                if (column.isPrimaryKey)
                {
                    if (string.IsNullOrEmpty(primary_keys_string))
                        primary_keys_string = SchemaTablesMetaData.EscapeIdentifier(column.columnName);
                    else primary_keys_string = String.Format("{0},{1}", primary_keys_string , SchemaTablesMetaData.EscapeIdentifier(column.columnName));
                }
                if (i + 1 < table.columns.Count || !string.IsNullOrEmpty(primary_keys_string))
                    stmtBuilder.Append(", ");
            }
            if (!string.IsNullOrEmpty(primary_keys_string))
                stmtBuilder.Append(String.Format("PRIMARY KEY({0})", primary_keys_string));
            stmtBuilder.Append(")");
        
            return stmtBuilder.ToString();
        }

        /// <summary>
        /// Populates a SQLiteCommand parameter list with parameter names based on the column names of a table
        /// </summary>
        /// <param name="table">Table metadata to obtain column names</param>
        /// <param name="cmd">SQLite command whose parameter List will be filled</param>
        private void CreateSqlCommandParameters(TableMetaData table, SQLiteCommand cmd)
        {
            cmd.Parameters.Clear();
            foreach (ColumnMetaData column in table.columns)
            {
                cmd.Parameters.Add(new SQLiteParameter(String.Format("@{0}", SchemaTablesMetaData.HexString(column.columnName))));
            }
        }

        /// <summary>
        /// Sets the parameter value for a SQLite query
        /// </summary>
        /// <param name="column">Column metadata info. The parameter will be named after the column name</param>
        /// <param name="cmd">SQLite command where to set the parameter</param>
        /// <param name="value">Value to set the parameter will be obtained from a row in this OleDbDataReader whose column has the column name</param>
        private void SetSqlCommandParameterValueForColumn(ColumnMetaData column, SQLiteCommand cmd, DbDataReader value)
        {
            string parameterName = String.Format("@{0}", SchemaTablesMetaData.HexString(column.columnName));
            cmd.Parameters[parameterName].Value = value[column.columnName];
        }

        /// <summary>
        /// Creates a SQL INSERT sentence to insert a row of data to a table in the target database.
        /// </summary>
        /// <param name="table">Table metadata from the table to copy</param>
        /// <returns>SQL Sentence of the insertion in the target database</returns>
        private string CreateInsertQuery(TableMetaData table)
        {
            string csvColumList = string.Empty;
            string csvParamsList = string.Empty;

            foreach (ColumnMetaData column in table.columns)
            {
                csvColumList = String.Format("{0}{1},",csvColumList , SchemaTablesMetaData.EscapeIdentifier(column.columnName));
                csvParamsList = String.Format("{0}@{1},", csvParamsList, SchemaTablesMetaData.HexString(column.columnName));
            }
            string query = String.Format("INSERT INTO {0} ({1}) VALUES ({2})", SchemaTablesMetaData.EscapeIdentifier(table.tableName), csvColumList.Remove(csvColumList.Length - 1), csvParamsList.Remove(csvParamsList.Length - 1));
            return query;
        }

        /// <summary>
        /// Dumps table contents obtained from a OleDbDataReader into the target database
        /// </summary>
        /// <param name="table">Table metadata from where the DataReader is obtained</param>
        /// <param name="reader">DataReader that provides the original data</param>
        public override void DumpTable(TableMetaData table, DbDataReader reader)
        {
            SQLiteTransaction transaction = null;
            string currentQuery = string.Empty;
            try
            {
                using (SQLiteConnection connector = new SQLiteConnection(connectionString))
                {
                    connector.Open();
                    using (SQLiteCommand Cmd = new SQLiteCommand())
                    {
                        Cmd.CommandText = CreateInsertQuery(table);
                        Cmd.Connection = connector;
                        CreateSqlCommandParameters(table, Cmd);

                        int countRecords = 0;
                        transaction = connector.BeginTransaction();
                        while (reader.Read())
                        {
                            foreach (ColumnMetaData colum in table.columns)
                            {
                                SetSqlCommandParameterValueForColumn(colum, Cmd, reader);
                            }
                            currentQuery = Cmd.CommandText;
                            Cmd.ExecuteNonQuery();
                            if (countRecords++ % 100 == 0) System.Console.Out.Write(".");
                        }
                        transaction.Commit();
                        System.Console.Out.WriteLine("done");
                        System.Console.Out.WriteLine("Table dump complete, {0} records copied.", countRecords);
                    }
                    connector.Close();
                }
            }
            catch (SQLiteException ex)
            {
                if (null != transaction)
                    transaction.Rollback();
                Console.Out.WriteLine("SQLite Exception dumping table {0} : {1}\n{2}", table.tableName, ex.Message, currentQuery);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public override SchemaTablesMetaData QuerySchemaDefinition(string schema)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        public override void QueryTableDefinition(TableMetaData table)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="target"></param>
        public override void DumpTableContents(TableMetaData table, IDBBackEnd target)
        {
            throw new NotImplementedException();
        }
    }
}
