
namespace ComparePgsqlTool.Services
{
    public class Comment : ItemComparable
    {
        static string SELECT_QUERY = $@"
   SELECT 
n.nspname||'.'||c.relname As {Constants.ELEMENT_NAME}
,CASE WHEN c.relkind = 'v' THEN 'view' ELSE 'TABLE' END as {Constants.ELEMENT_TYPE}
,d.description as {Constants.DEFINITION}
FROM pg_class As c
LEFT JOIN pg_namespace n ON n.oid = c.relnamespace
LEFT JOIN pg_tablespace t ON t.oid = c.reltablespace
LEFT JOIN pg_description As d ON (d.objoid = c.oid AND d.objsubid = 0)
AND  n.nspname ='{Constants.REPLACEMENT_SCHEMA}'
WHERE c.relkind IN('r', 'v') AND d.description > ''
union
SELECT n.nspname||'.'||relname||'.'||a.attname as {Constants.ELEMENT_NAME}
,'COLUMN' as {Constants.ELEMENT_TYPE}
,d.description as {Constants.DEFINITION}
FROM pg_class As c
INNER JOIN pg_attribute As a ON c.oid = a.attrelid
LEFT JOIN pg_namespace n ON n.oid = c.relnamespace
LEFT JOIN pg_tablespace t ON t.oid = c.reltablespace
inner JOIN pg_description As d ON (d.objoid = c.oid AND d.objsubid = a.attnum)
WHERE  c.relkind IN('r', 'v') 
AND  n.nspname ='{Constants.REPLACEMENT_SCHEMA}'
ORDER BY 1 ;";
        public Comment()
        {
            this.Query = SELECT_QUERY;
        }
        internal override string Create(InterItemCommunication communication)
        {
            return $"COMMENT ON {ElementType} {Name} IS '{Definition.Replace("'","''")}'";
        }
        internal override string Drop(InterItemCommunication communication)
        {
            return $"COMMENT ON {ElementType} {Name} IS ''";
        }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return Create(communication);
        }

    }
 
}

