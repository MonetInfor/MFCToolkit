using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;

namespace mdb2sq3
{
    /// <summary>
    /// This class is a container for the metadata information to describe a very simple database structure.
    /// </summary>
    class SchemaTablesMetaData
    {
        public string schemaName;
        public List<TableMetaData> tables = new List<TableMetaData>();

        /// <summary>
        /// Adds a table to the schema collection.
        /// </summary>
        /// <param name="table">Table Metadata info to add</param>
        public void AddTable(TableMetaData table)
        {
            tables.Add(table);
        }

        /// <summary>
        /// Finds a table by name
        /// </summary>
        /// <param name="aTableName">name of the table</param>
        /// <returns></returns>
        public TableMetaData FindTable(string aTableName)
        {
            return FindTable(aTableName, tables);
        }

        /// <summary>
        /// Finds a table by name in the indicated table list
        /// </summary>
        /// <param name="aTableName">Name of the table</param>
        /// <param name="aTableList">List of table metadata objects where to look for</param>
        /// <returns></returns>
        private TableMetaData FindTable(string aTableName, List<TableMetaData> aTableList)
        {
            return aTableList.Find(delegate(TableMetaData candidate)
                {
                    return candidate.tableName == aTableName;
                }
            );
        }

        /// <summary>
        /// Helper to encase identifiers between single quote characters
        /// </summary>
        /// <param name="identifier">string to escape with quotes</param>
        /// <returns>Escaped (quoted) identifier</returns>
        public static String EscapeIdentifier(String identifier)
        {
            return "'" + identifier.Replace("'", "''") + "'";
        }

        /// <summary>
        /// Helper to avoid identifiers that contains invalid characters.
        /// </summary>
        /// <param name="identifier">string to escape removing non alphanumeric characters (whitespaces, signs...) </param>
        /// <returns>Escaped identifier (hexadecimal sequence)</returns>
        public static String HexString(String identifier)
        {
            string hexConversion = string.Empty;
            foreach (ushort u in identifier)
                hexConversion = String.Format("{1}{0:x4}", u,hexConversion);
            return hexConversion;
        }

        /// <summary>
        /// Sorts the table list by dependencies (FK)
        /// </summary>
        public void SortTablesByDependencies()
        {
            List<TableMetaData> tmpSortList = new List<TableMetaData>();

            int actualSortedElemements = 0;
            int previousSortedElements = -1;
            while ((actualSortedElemements != previousSortedElements) && (tmpSortList.Count != tables.Count))
            {
                previousSortedElements = actualSortedElemements;
                foreach (TableMetaData candidateTable in tables)
                {
                    if (tmpSortList.Contains(candidateTable))
                        continue;
                    bool validPrecedence = true;
                    foreach (ColumnMetaData inspectColumn in candidateTable.columns)
                    {
                        if (inspectColumn.hasForeignKey)
                        {
                            if (FindTable(inspectColumn.fkTable, tmpSortList) == null)
                            {
                                validPrecedence = false;
                                break;
                            }
                        }
                    }
                    if (validPrecedence)
                    {
                        tmpSortList.Add(candidateTable);
                    }
                }
                actualSortedElemements = tmpSortList.Count;
                System.Console.Out.WriteLine("Sort loop: candidates added: {0}", actualSortedElemements - previousSortedElements);
            }
            tables = tmpSortList;
        }

        /// <summary>
        /// Returns information about the schema
        /// </summary>
        /// <returns>A string that contains summarized information about the schema</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine(String.Format("Schema: {0}", schemaName));
            str.AppendLine("-----------------------------------");
            foreach (TableMetaData table in tables)
            {
                str.AppendLine(table.ToString());
            }
            return str.ToString();
        }
    }

    /// <summary>
    /// This class is a container for the metadata information to describe a very simple database table.
    /// </summary>
    class TableMetaData
    {
        /// <summary>
        /// 
        /// </summary>
        public string tableName;

        /// <summary>
        /// 
        /// </summary>
        public List<ColumnMetaData> columns = new List<ColumnMetaData>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="col"></param>
        public void AddColumn(ColumnMetaData col)
        {
            columns.Add(col);
            columns.Sort(ColumnMetaData.CompareColumnOrder);
        }

        /// <summary>
        /// Finds a ColumnMetaData by its name
        /// </summary>
        /// <param name="aColumnName">Name of the column to search for</param>
        /// <returns>A ColumnMetaData object named as the parameter</returns>
        public ColumnMetaData FindColumn(string aColumnName)
        {
            return columns.Find(delegate(ColumnMetaData candidate)
                {
                    return candidate.columnName == aColumnName;
                }
            );
        }

        /// <summary>
        /// Returns information about the table
        /// </summary>
        /// <returns>A String that contains information about the table</returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine(String.Format("Table: {0}",tableName));
            str.AppendLine("===================================");
            foreach (ColumnMetaData column in columns)
            {
                str.AppendLine(column.ToString());
            }
            return str.ToString();
        }

    }

    /// <summary>
    /// This class is a container for the metadata information to describe a very simple table column.
    /// </summary>
    class ColumnMetaData
    {
        public string columnName;
        public string columnDescription;
        public int ordinalPosition;
        public int maxCharSize;
        public int numericPrecision;
        public int numericScale;
        public int datetimePrecision;
        public bool hasDefault;
        public bool isNullable=true;
        public string defaultValue;
        public OleDbType columnType;
        public bool isPrimaryKey=false;
        public bool hasForeignKey = false;
        public string fkTable;
        public string fkColumn;

        /// <summary>
        /// Comparer function for ColumnMetadata Objects
        /// </summary>
        /// <param name="x">left object to compare</param>
        /// <param name="y">right object to compare</param>
        /// <returns></returns>
        public static int CompareColumnOrder(ColumnMetaData x, ColumnMetaData y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    int retval = x.ordinalPosition.CompareTo(y.ordinalPosition);
                    return retval;
                }
            }
        }

        /// <summary>
        /// Returns information about the column
        /// </summary>
        /// <returns>A String that contains information about the column</returns>
        public override string ToString()
        {
            string fieldSize = String.Format("{0}",maxCharSize);
            switch (columnType)
            {
                case OleDbType.Decimal:
                case OleDbType.Double:
                case OleDbType.Integer:
                case OleDbType.Numeric:
                case OleDbType.Single:
                case OleDbType.SmallInt:
                case OleDbType.TinyInt:
                case OleDbType.UnsignedBigInt:
                case OleDbType.UnsignedInt:
                case OleDbType.UnsignedSmallInt:
                case OleDbType.UnsignedTinyInt:
                case OleDbType.VarNumeric:
                case OleDbType.BigInt:
                case OleDbType.Currency:
                    fieldSize = String.Format("{0},{1}",numericPrecision, numericScale);
                    break;
            }
            return String.Format("{1}){5}\"{0}\":\t{2}({4})\t [{3}]\t {6} {7}", columnName, ordinalPosition, columnType, columnDescription, fieldSize,(isPrimaryKey?"*":""),(hasDefault? "{"+defaultValue+"}":""),(hasForeignKey? "\n\tFK: "+fkTable+"."+fkColumn:""));
        }
    }

}
