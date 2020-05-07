
namespace ComparePgsqlTool.Services
{
    public class Owner : ItemComparable
    {
        static string SELECT_QUERY = $@"
    SELECT c.relname AS {Constants.TABLE_NAME}
    , a.rolname AS {Constants.ELEMENT_NAME}
    , CASE WHEN c.relkind = 'r' THEN 'TABLE' 
        WHEN c.relkind = 'S' THEN 'SEQUENCE' 
        WHEN c.relkind = 'v' THEN 'VIEW' 
        ELSE c.relkind::varchar END as {Constants.ELEMENT_TYPE}
FROM pg_class AS c
INNER JOIN pg_roles AS a ON (a.oid = c.relowner)
INNER JOIN pg_namespace AS n ON (n.oid = c.relnamespace)
WHERE n.nspname='{Constants.REPLACEMENT_SCHEMA}' 
AND (
c.relkind IN ('r',  'v')
or
        ( c.relkind = 'S' and  exists 
            ( --to exclude sequences from SERIAL owned by table 
                SELECT 1 FROM pg_class c2, pg_user u2
                 WHERE c2.relowner = u2.usesysid and c2.relkind = 'S'
                 and c2.relname=c.relname AND relnamespace IN ( SELECT oid FROM pg_namespace WHERE nspname = ('essai') ) 
            )
        )
   )
";
        public Owner()
        {
            this.Query = SELECT_QUERY;
        }


        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return $"ALTER {ElementType} {TableName} OWNER TO {Name}";
        }
        public override string Key()
        {
            return $"{TableName}.{Name}";
        }
        internal override string Create(InterItemCommunication communication)
        {
                return $@"{Alter(null, communication)}";
        }
        internal override string Drop(InterItemCommunication communication)
        {
            return "";
        }
    }

    /*
    */
}
