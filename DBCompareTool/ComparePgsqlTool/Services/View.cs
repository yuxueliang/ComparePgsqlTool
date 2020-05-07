using System;

namespace ComparePgsqlTool.Services
{
    public class View : ItemComparable
    {
        static string SELECT_QUERY = $@"
SELECT viewname as {Constants.ELEMENT_NAME}
,'VIEW' as {Constants.ELEMENT_TYPE}
, definition as {Constants.DEFINITION}
,array(SELECT distinct dependent_view.relname
FROM pg_depend 
JOIN pg_rewrite ON pg_depend.objid = pg_rewrite.oid 
JOIN pg_class as dependent_view ON pg_rewrite.ev_class = dependent_view.oid 
JOIN pg_class as source_table ON pg_depend.refobjid = source_table.oid 
JOIN pg_attribute ON pg_depend.refobjid = pg_attribute.attrelid 
    AND pg_depend.refobjsubid = pg_attribute.attnum 
JOIN pg_namespace dependent_ns ON dependent_ns.oid = dependent_view.relnamespace
JOIN pg_namespace source_ns ON source_ns.oid = source_table.relnamespace
WHERE 
source_ns.nspname='{Constants.REPLACEMENT_SCHEMA}' 
AND source_table.relname = viewname
AND pg_attribute.attnum > 0 
limit 1
) as {Constants.USED_BY}
FROM pg_views 
	WHERE schemaname ='{Constants.REPLACEMENT_SCHEMA}' 
	ORDER BY viewname;";
        public View()
        {
            this.Query = SELECT_QUERY;
        }
        private string UsedBy { get { return this[Constants.USED_BY]; } }
        internal override string Create(InterItemCommunication communication)
        {
            return $"CREATE VIEW {Name} AS {Definition}"; 
        }
        internal override string Drop(InterItemCommunication communication)
        {
            string drop = "";
            if (UsedBy != "null")
            {
                drop = "--!!!! ATTENTION This view is used by other views !" + Environment.NewLine;
                drop += "--" + UsedBy + Environment.NewLine;
            }
            drop +=base.Drop(communication);
            return drop;
        }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return Drop(communication) + ";" + Environment.NewLine + Create(communication);
        }

    }
}
