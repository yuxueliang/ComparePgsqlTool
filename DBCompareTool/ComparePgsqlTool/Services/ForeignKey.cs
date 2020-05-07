using System;

namespace ComparePgsqlTool.Services
{
    public class ForeignKey : ItemComparable
    {
        static string SELECT_QUERY = $@"
SELECT c.conname AS {Constants.ELEMENT_NAME}
	, cl.relname AS {Constants.TABLE_NAME}
    , 'FOREIGN KEY' AS {Constants.ELEMENT_TYPE}
	, pg_catalog.pg_get_constraintdef(c.oid, true) as {Constants.DEFINITION}
FROM pg_catalog.pg_constraint c
INNER JOIN pg_class AS cl ON (c.conrelid = cl.oid)
inner join pg_namespace nsp on c.connamespace=nsp.oid
WHERE c.contype = 'f' 
AND nsp.nspname='{Constants.REPLACEMENT_SCHEMA}';
";
        public ForeignKey()
        {
            this.Query = SELECT_QUERY;
        }

        public override string Key()
        {
            return $"{this.TableName}.{this.Name}".ToLowerInvariant();
        }
        internal override string Create(InterItemCommunication communication)
        {
            if (Definition == "null")
            {
                return $"--Erreur sur définition de la clé étrangère {Key()}";
            }
            return $"ALTER TABLE {TableName} ADD CONSTRAINT {Name} {Definition}";
        }
        internal override string Drop(InterItemCommunication communication)
        {
            if(communication.TableToDeleteList.Contains(TableName))
            {
                return "";
            }
            return $"ALTER TABLE {TableName} DROP CONSTRAINT {Name}";
        }

        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            return Drop(communication) + ";"+Environment.NewLine + Create(communication);
        }
    }
}
