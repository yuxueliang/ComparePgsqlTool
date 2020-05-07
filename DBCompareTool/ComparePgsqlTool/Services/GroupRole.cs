using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComparePgsqlTool.Services
{
    public class GroupRole : ItemComparable
    {

        static string SELECT_QUERY = $@"
SELECT r.rolname as {Constants.ELEMENT_NAME}
,'ROLE' as {Constants.ELEMENT_TYPE}
, r.rolsuper
    , r.rolinherit
    , r.rolcreaterole
    , r.rolcreatedb
    , r.rolcanlogin
    , r.rolconnlimit
    , case r.rolvaliduntil when 'infinity'::timestamp then null else  r.rolvaliduntil  end
    , r.rolreplication
	, ARRAY(SELECT b.rolname 
	        FROM pg_catalog.pg_auth_members m  
			JOIN pg_catalog.pg_roles b ON (m.roleid = b.oid)  
	        WHERE m.member = r.oid) as memberof
FROM pg_catalog.pg_roles AS r"
//It should work for every login but we limit on group logins
//remove the following line to use it for every login
+@"
where rolcanlogin= false
ORDER BY r.rolname;";
        public GroupRole()
        {
            this.Query = SELECT_QUERY;
            PreparerOptions();
        }

        private void PreparerOptions()
        {
            OptionValueList.Add(new OptionValue
            {
                OptionName = "rolcanlogin",
                ComparisonValue = "True",
                ValueIfDifferent = "NOLOGIN",
                ValueIfEqual = "WITH PASSWORD 'changeme' LOGIN"
            });
            OptionValueList.Add(new OptionValue
            {
                OptionName = "rolsuper",
                ComparisonValue = "True",
                ValueIfEqual = "SUPERUSER"
            });
            OptionValueList.Add(new OptionValue
            {
                OptionName = "rolcreatedb",
                ComparisonValue = "True",
                ValueIfEqual = "CREATEDB"
            });
            OptionValueList.Add(new OptionValue
            {
                OptionName = "rolcreaterole",
                ComparisonValue = "True",
                ValueIfEqual = "CREATEROLE"
            });
            OptionValueList.Add(new OptionValue
            {
                OptionName = "rolinherit",
                ComparisonValue = "True",
                ValueIfDifferent = "NOINHERIT",
                ValueIfEqual = "INHERIT"
            });
            OptionValueList.Add(new OptionValue
            {
                OptionName = "rolreplication",
                ComparisonValue = "True",
                ValueIfDifferent = "NOREPLICATION",
                ValueIfEqual = "REPLICATION"
            });



        }
        internal override string GenerateOptions()
        {
            string options = base.GenerateOptions();
            if (this["rolconnlimit"] != "-1" && (this["rolconnlimit"].Length) > 0)
            {
                options += " CONNECTION LIMIT " + this["rolconnlimit"];
            }
            if (this["rolvaliduntil"] != "null")
            {
                options += $" VALID UNTIL '{this["rolvaliduntil"]}'";
            }

            return options;
        }
       

        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            string alter = string.Empty;
            
            var optionsTarget = target.GenerateOptions();
            var options = GenerateOptions();

            if (options != optionsTarget)
            {
                alter = $"CREATE ROLE {Name} {options};";
            }

            if (this["memberof"] != target["memberof"])
            {
                alter += GenerateMemberOfDifference(target);
            }
            return alter;
        }

        private string GenerateMemberOfDifference(ItemComparable target)
        {
            StringBuilder difference = new StringBuilder();
            List<string> sourceList = GetRightsList(this["memberof"]);
            List<string> targetList = GetRightsList(target["memberof"]);
            sourceList
                .Where(x => !targetList.Any(y => x == y))
                .ToList()
                .ForEach(x =>difference.AppendLine($"REVOKE {x} FROM {Name};"));

            targetList
                .Where(x => !sourceList.Any(y => x == y))
                .ToList()
                .ForEach(x => difference.AppendLine($"GRANT {x} TO {Name};"));

            return difference.ToString();
        }

        private List<string> GetRightsList(string valeur)
        {
            var memberOfRevoke = valeur.Replace("{", "").Replace("}", "");
            return memberOfRevoke.Split(',').ToList();

        }
    }
}
