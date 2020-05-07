using System.Text.RegularExpressions;

namespace ComparePgsqlTool.Services
{
    public class Validation
    {
        string regexNamingRule = "^[a-zA-Z0-9_]+$";

        internal bool CheckDb(Database sourceDb)
        {
            return checkNameIsValid(sourceDb.Name) && 
                checkNameIsValid(sourceDb.Schema);
        }

        private bool checkNameIsValid(string valueString)
        {
            Match match = Regex.Match(valueString, regexNamingRule, RegexOptions.IgnoreCase);

            return (!string.IsNullOrEmpty(valueString) && match.Success);
        }

        internal string Usage()
        {
            return "!! Database and Schema must be alphanumeric !! (^[a-zA-Z0-9_]+$)";
        }
    }
}
