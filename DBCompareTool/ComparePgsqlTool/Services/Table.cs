
namespace ComparePgsqlTool.Services
{
    public class Table : ItemComparable
    {
        private static string where = @" and table_name not like 'proj\_%' ";

        static string SELECT_QUERY = $@"
SELECT table_name as {Constants.ELEMENT_NAME}
    , CASE table_type WHEN 'BASE TABLE' THEN 'TABLE' ELSE table_type END AS {Constants.ELEMENT_TYPE}
    , is_insertable_into
FROM information_schema.tables 
WHERE table_schema ='{Constants.REPLACEMENT_SCHEMA}' 
AND table_type = 'BASE TABLE' {where}
ORDER BY table_name;";
        public Table()
        {
            this.Query = SELECT_QUERY;
        }

        internal override string Create(InterItemCommunication communication)
        {
            return base.Create(communication) + "()";
        }

    }
}
