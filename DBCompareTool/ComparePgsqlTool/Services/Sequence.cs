
namespace ComparePgsqlTool.Services
{
    public class Sequence : ItemComparable
    {
        private static string where = @" and sequencename not like 'proj\_%' ";

        //We select all sequences except the one from SERIAL

        private static string SELECT_QUERY = $@"SELECT 
sequencename as {Constants.ELEMENT_NAME}
,'SEQUENCE' as {Constants.ELEMENT_TYPE}
, data_type
, start_value
, min_value
, max_value
, increment_by
, cycle 
FROM pg_sequences
WHERE schemaname ='{Constants.REPLACEMENT_SCHEMA}'  {@where} 
ORDER BY sequencename;";

//        static string SELECT_QUERY = $@"SELECT 
//sequence_name as {Constants.ELEMENT_NAME}
//,'SEQUENCE' as {Constants.ELEMENT_TYPE}
//, data_type
//, start_value
//, minimum_value
//, maximum_value
//, increment
//, cycle_option 
//FROM information_schema.sequences
//WHERE sequence_schema ='{Constants.REPLACEMENT_SCHEMA}' {@where}
//ORDER BY sequence_name;";
        public Sequence()
        {
            this.Query = SELECT_QUERY;
            this.IfExists = true;
        }
        internal override string Create(InterItemCommunication communication)
        {
            return $"CREATE {ElementType} IF NOT EXISTS {Name} INCREMENT {this["increment_by"]} MINVALUE {this["min_value"]} MAXVALUE {this["max_value"]} START {this["start_value"]};";
        }

        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return string.Empty;
        }

        internal override string Drop(InterItemCommunication communication)
        {
            return string.Empty;//TODO:这里不需要生成删除序列的
        }
    }
}
