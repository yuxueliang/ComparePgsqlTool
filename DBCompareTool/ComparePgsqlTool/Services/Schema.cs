
namespace ComparePgsqlTool.Services
{
    public class Schema : ItemComparable
    {
        static string SELECT_QUERY = $@"
select schema_name as {Constants.ELEMENT_NAME}
,'SCHEMA' AS {Constants.ELEMENT_TYPE}
,schema_owner as {Constants.OWNER}
from information_schema.schemata
where schema_name='{Constants.REPLACEMENT_SCHEMA}'";
        public Schema()
        {
            this.Query = SELECT_QUERY;
        }
        private string Owner { get { return this[Constants.OWNER]; } }
        internal override string Create(InterItemCommunication communication)
        {
            return $"CREATE SCHEMA {Name} AUTHORIZATION {this.Owner};";
        }

        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return string.Empty;
        }
    }
}
