using System;
using System.Collections.Generic;

namespace ComparePgsqlTool.Services
{
    public class Grant : ItemComparable
    {
        static string SELECT_QUERY = $@"
SELECT 'SCHEMA' as {Constants.ELEMENT_TYPE}
,nspname AS {Constants.TABLE_NAME}
,'SCHEMA' as {Constants.TARGET}
,null::text as {Constants.ELEMENT_NAME}
,unnest(nspacl)::text  AS acl
from pg_catalog.pg_namespace
where nspname='{Constants.REPLACEMENT_SCHEMA}' 
union
SELECT
 CASE c.relkind
    WHEN 'r' THEN 'TABLE'
    WHEN 'v' THEN 'VIEW'
    WHEN 'S' THEN 'SEQUENCE'
    WHEN 'f' THEN 'FOREIGN TABLE'
    END as {Constants.ELEMENT_TYPE}

 ,  c.relname AS {Constants.TABLE_NAME}
 ,CASE c.relkind
    WHEN 'r' THEN 'TABLE'
    WHEN 'v' THEN 'VIEW'
    WHEN 'S' THEN 'SEQUENCE'
    WHEN 'f' THEN 'FOREIGN TABLE'
    END as {Constants.TARGET}
,null::text as {Constants.ELEMENT_NAME}
  , unnest(c.relacl)::text AS acl
FROM pg_catalog.pg_class c
LEFT JOIN pg_catalog.pg_namespace n ON (n.oid = c.relnamespace)
WHERE c.relkind IN ('r', 'v', 'S', 'f')
 AND  n.nspname='{Constants.REPLACEMENT_SCHEMA}'
union
 SELECT
'{Constants.COLUMN}' as {Constants.ELEMENT_TYPE}
   ,c.relname AS {Constants.TABLE_NAME}
   ,CASE c.relkind
    WHEN 'r' THEN 'TABLE'
    WHEN 'v' THEN 'VIEW'
    WHEN 'f' THEN 'FOREIGN TABLE'
    END as {Constants.TARGET}
  , a.attname AS {Constants.ELEMENT_NAME}
  , a.attacl::text AS acl

FROM pg_catalog.pg_class c
LEFT JOIN pg_catalog.pg_namespace n ON (n.oid = c.relnamespace)
INNER JOIN (SELECT attname, unnest(attacl) AS attacl, attrelid
           FROM pg_catalog.pg_attribute
           WHERE NOT attisdropped AND attacl IS NOT NULL)
      AS a ON (a.attrelid = c.oid)
WHERE c.relkind IN ('r', 'v', 'f')
AND  n.nspname='{Constants.REPLACEMENT_SCHEMA}'

ORDER BY 1,2";
        Dictionary<string, string> rightList = new Dictionary<string, string>();

        public Grant()
        {
            this.Query = SELECT_QUERY;
            InitRightList();
        }

        private void InitRightList()
        {
            rightList.Add("a", "INSERT");
            rightList.Add("r", "SELECT");
            rightList.Add("w", "UPDATE");
            rightList.Add("d", "DELETE");
            rightList.Add("D", "TRUNCATE");
            rightList.Add("x", "REFERENCES");
            rightList.Add("t", "TRIGGER");
            rightList.Add("X", "EXECUTE");
            rightList.Add("U", "USAGE");
            rightList.Add("C", "CREATE");
            rightList.Add("c", "CONNECT");
            rightList.Add("T", "TEMPORARY");

        }

        private string Target { get { return this[Constants.TARGET]; } }
        private string ACL { get { return this["acl"]; } }
        private int EqualPosition { get { return ACL.IndexOf("="); } }
        private int SlashPosition { get { return ACL.IndexOf("/"); } }
        private string AccessList
        {
            get
            {
                List<string> rights = new List<string>();
                string sectionAcl = ACL.Substring(EqualPosition + 1, SlashPosition - EqualPosition - 1);
                for (int i = 0; i < sectionAcl.Length; i++)
                {
                    if (rightList.ContainsKey(sectionAcl[i].ToString()))
                    {
                        rights.Add(rightList[sectionAcl[i].ToString()]);
                    }
                }

                string rightString = string.Join(", ", rights);
                rightString = ReplaceAll(rightString);
                return rightString;
            }
        }

        private string ReplaceAll(string rightString)
        {
            if (Target == "TABLE" && rightString == "INSERT, SELECT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER")
                return "ALL";
            if (Target == "VIEW" && rightString == "INSERT, SELECT, UPDATE, DELETE, TRUNCATE, REFERENCES, TRIGGER")
                return "ALL";
            if (Target == "VIEW" && rightString == "INSERT, SELECT, UPDATE, DELETE, REFERENCES, TRIGGER")
                return "ALL";
            if (Target == "SEQUENCE" && rightString == "SELECT, UPDATE, USAGE")
                return "ALL";
            if (Target == "SCHEMA" && rightString == "USAGE, CREATE")
                return "ALL";

            return rightString;

        }

        private string User
        {
            get
            {
                var userString = ACL.Substring(0, EqualPosition); ;
                if (string.IsNullOrEmpty(userString))
                {
                    return "public";
                }
                else
                {
                    return userString;
                }
            }
        }
        public override string Key()
        {
            if (ElementType == Constants.COLUMN)
            {
                return $"{TableName}.{Name}.{User}";
            }
            else
            {
                return $"{TableName}.{User}";
            }
        }
        internal override string Create(InterItemCommunication communication)
        {
            if (ElementType == Constants.COLUMN)
            {
                return $"GRANT {AccessList} ({Name}) ON {Target} {TableName} TO {User}";
            }
            else
            {
                return $"GRANT {AccessList} ON {Target} {TableName} TO {User}";
            }
        }
        internal override string Drop(InterItemCommunication communication)
        {
            if (ElementType == Constants.COLUMN)
            {
                return $"REVOKE {AccessList} ({Name}) ON {Target} {TableName} FROM {User}";
            }
            else
            {
                return $"REVOKE {AccessList} ON {Target} {TableName} FROM {User}";
            }
        }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return target.Drop(communication) + ";" + Environment.NewLine + Create(communication);
        }



    }
}
