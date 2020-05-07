
namespace ComparePgsqlTool.Services
{
    public class Trigger : ItemComparable
    {
        static string SELECT_QUERY = $@"
 SELECT tbl.relname as {Constants.TABLE_NAME}
,'TRIGGER' as {Constants.ELEMENT_TYPE}
, t.tgname AS  {Constants.ELEMENT_NAME}
, pg_catalog.pg_get_triggerdef(t.oid, true) AS {Constants.DEFINITION}
    FROM pg_catalog.pg_trigger t
    INNER JOIN (
    	SELECT c.oid, c.relname
    	FROM pg_catalog.pg_class c
    	JOIN pg_catalog.pg_namespace n ON (n.oid = c.relnamespace AND n.nspname='{Constants.REPLACEMENT_SCHEMA}')) AS tbl
    	ON (tbl.oid = t.tgrelid)
    AND NOT t.tgisinternal
    ORDER BY 1;
";
        public Trigger()
        {
            this.Query = SELECT_QUERY;
            Cascade = true;
        }
        internal override string Create(InterItemCommunication communication)
        {
            return Definition;
        }
        internal override string Drop(InterItemCommunication communication)
        {

            return $"DROP TRIGGER {Name} ON {TableName}";
        }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return Create(communication);
        }

    }
}
