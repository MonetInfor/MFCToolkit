using System;

namespace mdb2sq3
{
    class Program
    {
        static int Main(string[] args)
        {
            if (!CommandLineParametersHelper.ParseArguments(args))
            {
                return 1;
            }

            MSAccessBackend msbackend = new MSAccessBackend(CommandLineParametersHelper.databaseSource);
            string TargetFile = CommandLineParametersHelper.databaseTarget;
            if (null == TargetFile)
                TargetFile = System.IO.Path.ChangeExtension(CommandLineParametersHelper.databaseSource, "db");

            if (System.IO.File.Exists(TargetFile) && CommandLineParametersHelper.erase)
                System.IO.File.Delete(TargetFile);

            SQLiteBackend backend = new SQLiteBackend(TargetFile);

            // Explore the source database to get metadata.
            SchemaTablesMetaData schemaInfo = msbackend.QuerySchemaDefinition(null);
            foreach (TableMetaData t in schemaInfo.tables)
            {
                msbackend.QueryTableDefinition(t);
            }
            if (CommandLineParametersHelper.verbose)
            {
                Console.Out.WriteLine(schemaInfo.ToString());
                Console.Out.WriteLine("There are {0} tables to convert.", schemaInfo.tables.Count);
            }

            schemaInfo.SortTablesByDependencies();

            if (CommandLineParametersHelper.verbose)
            {
                Console.Out.WriteLine("Tables will be created in this order:");
                foreach (TableMetaData t in schemaInfo.tables)
                {
                    Console.Out.Write(t.tableName + " , ");
                }
                Console.Out.WriteLine();
            }

            // Clone source db schema into target db schema (no data)
            backend.CloneSchema(schemaInfo);

            // Copy data from source db to target db
            DateTime dt1 = DateTime.Now;
            foreach (TableMetaData table in schemaInfo.tables)
            {
                Console.Out.Write("Dumping table {0}", table.tableName);
                msbackend.DumpTableContents(table, backend);
            }
            TimeSpan sp = DateTime.Now - dt1;

            Console.Out.WriteLine("Complete dump performed in {0} ", sp);

            return 0;
        }
    }
}
