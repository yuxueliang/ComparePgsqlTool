using System;

namespace ComparePgsqlTool.Services
{
    public class Index : ItemComparable
    {
        private static string where = @" and c.relname not like 'proj\_%' ";
        static string SELECT_QUERY = $@"
SELECT c.relname AS {Constants.TABLE_NAME}
    , c2.relname AS {Constants.ELEMENT_NAME}
, 'INDEX'    AS {Constants.ELEMENT_TYPE}
, i.indisprimary AS {Constants.PRIMARY_KEY}
    , i.indisunique AS {Constants.UNIQUE_KEY}
    , pg_catalog.pg_get_indexdef(i.indexrelid, 0, true) AS {Constants.DEFINITION}
    , pg_catalog.pg_get_constraintdef(con.oid, true) AS {Constants.CONSTRAINT}
    , con.contype AS typ
FROM pg_catalog.pg_index AS i
JOIN pg_catalog.pg_class AS c ON (c.oid = i.indrelid)
JOIN pg_catalog.pg_class AS c2 ON (c2.oid = i.indexrelid)
LEFT JOIN pg_catalog.pg_constraint con
    ON (con.conrelid = i.indrelid AND con.conindid = i.indexrelid AND con.contype IN ('p','u','x'))
JOIN pg_catalog.pg_namespace AS n ON (c2.relnamespace = n.oid)
WHERE c.relname NOT LIKE 'pg_%' {where}
AND n.nspname='{Constants.REPLACEMENT_SCHEMA}';
";
        public Index()
        {
            this.Query = SELECT_QUERY;
            IfExists = true;
        }

        public override string Key()
        {
            return $"{this.TableName}.{this.Name}".ToLowerInvariant();
        }
        private string UniqueKey { get { return this[Constants.UNIQUE_KEY]; } }

        private string PrimaryKey { get { return this[Constants.PRIMARY_KEY]; } }

        private string Constraint { get { return this[Constants.CONSTRAINT]; } }
        internal override string Create(InterItemCommunication communication)
        {
            if (Definition == "null")
            {
                return $"--Error on index definition {Key()}";
            }

            string create = Definition;
            create += ";" + Environment.NewLine;

            if (Constraint != "null")
            {
                if (PrimaryKey != "null")
                {
                    create += $"ALTER TABLE ONLY {TableName} ADD CONSTRAINT {Name} PRIMARY KEY USING INDEX {Name}";
                }
                else if (UniqueKey == "True")
                {
                    create += $"ALTER TABLE ONLY {TableName} ADD CONSTRAINT {Name} UNIQUE USING INDEX {Name}";
                }
            }
            return create;
        }
        internal override string Drop(InterItemCommunication communication)
        {
            if (communication.TableToDeleteList.Contains(TableName))
            {
                return "";
            }

            string drop = "";
            if (Constraint != "null")
            {
                drop += "--Attention. This will delete linked foreign keys !" + Environment.NewLine;
                drop += $"ALTER TABLE ONLY {TableName} DROP CONSTRAINT IF EXISTS {Name} CASCADE;" + Environment.NewLine;
            }
            drop += base.Drop(communication);
            return drop;
        }
        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            string alter = "";
            var goodTypeTarget = target as Index;
            if (Definition == "null")
            {
                return $"--Error on index definition {Key()}";
            }
            if (goodTypeTarget.Definition == "null")
            {
                return $"--Error on index definition {goodTypeTarget.Key()}";
            }
            alter = Drop(communication);
            alter += ";" + Environment.NewLine;
            alter += Create(communication);

            return alter;
        }
    }
}
