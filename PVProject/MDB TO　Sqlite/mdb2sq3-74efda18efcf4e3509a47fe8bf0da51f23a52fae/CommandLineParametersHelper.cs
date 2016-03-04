using System;

namespace mdb2sq3
{
    /// <summary>
    /// This class is a simple organizer for the methods related to set specific options the user can set using the command line parameters.
    /// </summary>
    class CommandLineParametersHelper
    {
        #region Static Members
        public static string databaseSource = null;
        public static string databaseTarget = null;
        public static string databaseHost = null;
        public static bool verbose = false;
        public static bool erase = false;
        #endregion

        #region Public static Methods
        /// <summary>
        /// Static method to parse and set the options the program can have.
        /// </summary>
        /// <param name="args">Array of string parameters passed from the command line</param>
        /// <returns>True if the arguments are parsed correctly. False if the program should not continue with the given parameters</returns>
        public static bool ParseArguments(string[] args)
        {
            try
            {
                foreach (string argum in args)
                {
                    if (argum.ToUpper().StartsWith("-S:"))
                    {
                        databaseSource = argum.Substring(3);
                        continue;
                    }
                    if (argum.ToUpper().StartsWith("-T:"))
                    {
                        databaseTarget = argum.Substring(3);
                        continue;
                    }
                    if (argum.ToUpper().StartsWith("-E"))
                    {
                        erase = true;
                        continue;
                    }
                    if (argum.ToUpper().StartsWith("-V"))
                    {
                        verbose = true;
                        continue;
                    }

                    if (argum.ToUpper().StartsWith("-?"))
                    {
                        System.Console.WriteLine("mdb2sq3: Converts a simple MSAccess Database file into a SQLite database.");
                        System.Console.WriteLine("usage:");
                        System.Console.WriteLine("mdb2sq3 -s:sourcefile [-t:targetfile] [options]");
                        System.Console.WriteLine(" -s:sourcefile   Sets the source MSAccess database");
                        System.Console.WriteLine(" -t:targetfile   Sets the target SQLite database");
                        System.Console.WriteLine(" -e              Forces deletion of target file if it exists");
                        System.Console.WriteLine(" -v              Verbose mode.");
                        System.Console.WriteLine(" -?              Prints this help and exits the program.");
                        return false;
                    }
                    System.Console.WriteLine(String.Format("ERROR: Unknown parameter {0}", argum));
                    return false;
                }
            }
            catch (Exception)
            {
                System.Console.WriteLine("ERROR: Invalid parameter value.");
                return false;
            }

            // As a final step, check "must set" parameters.
            if (databaseSource == null )
            {
                System.Console.WriteLine(String.Format("ERROR: You need to set MsAccess Database source file"));
                return false;
            }
            if (!System.IO.File.Exists(databaseSource))
            {
                System.Console.WriteLine(String.Format("ERROR: source file cannot be found"));
                return false;
            }
            return true;
        }
        #endregion
    }
}
