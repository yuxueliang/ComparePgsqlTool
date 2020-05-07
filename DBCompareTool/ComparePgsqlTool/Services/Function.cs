
namespace ComparePgsqlTool.Services
{
    public class Function : ItemComparable
    {
        static string SELECT_QUERY = $@"
SELECT (p.oid::regprocedure)::text      AS {Constants.ELEMENT_NAME}
,'FUNCTION' as {Constants.ELEMENT_TYPE}
, t.typname                 AS return_type
, pg_get_functiondef(p.oid) AS {Constants.DEFINITION}
FROM pg_proc AS p
JOIN pg_type t ON (p.prorettype = t.oid)
JOIN pg_namespace n ON (n.oid = p.pronamespace)
JOIN pg_language l ON (p.prolang = l.oid AND l.lanname IN ('c','plpgsql', 'sql'))
WHERE n.nspname='{Constants.REPLACEMENT_SCHEMA}';
";
        public Function()
        {
            this.Query = SELECT_QUERY;
            Cascade = true;
        }
        private string ReturnType { get { return this["return_type"]; } }
        internal override string Create(InterItemCommunication communication)
        {
            return Definition; 
        }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return Create(communication);
        }

    }

}
