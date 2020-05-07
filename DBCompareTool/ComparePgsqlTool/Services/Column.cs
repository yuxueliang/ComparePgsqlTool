using System.Text;
using ComparePgsqlTool.Services;

namespace ComparePgsqlTool.Services
{
    public class Column : ItemComparable
    {
        private static string where = @" and table_name not like 'proj\_%' ";

        static string SELECT_QUERY = $@"
SELECT table_name as {Constants.TABLE_NAME}
    , column_name as {Constants.ELEMENT_NAME}
    , data_type as {Constants.ELEMENT_TYPE}
    , is_nullable
    , column_default
    , character_maximum_length
    ,identity_generation
FROM information_schema.columns 
WHERE table_schema = '{Constants.REPLACEMENT_SCHEMA}'
and table_name not in 
    (select table_name 
    from information_schema.tables 
    where table_schema='{Constants.REPLACEMENT_SCHEMA}'
    and table_type='VIEW') {@where}
AND is_updatable = 'YES'
ORDER BY table_name, column_name;";
        public Column()
        {
            this.Query = SELECT_QUERY;
            PrepareOptions();
        }

        public override string Key()
        {
            return $"{this.TableName}.{this.Name}".ToLowerInvariant();
        }

        private void PrepareOptions()
        {
            OptionValueList.Add(new OptionValue
            {
                OptionName = "is_nullable",
                ComparisonValue = "NO",
                ValueIfEqual = "NOT NULL"
            });

        }
        internal override string Create(InterItemCommunication communication)
        {
            string create = "";

            if (ElementType == "ARRAY")
            {
                create = "--!!! ARRAY : Double check !!!";
            }
            if (DefautValue.EndsWith($".{TableName}_{Name}_seq'::regclass)")){
                ElementType = "SERIAL";
            }

            create += $"ALTER TABLE {TableName} ADD COLUMN {Name} {TypeStringFromElementType} {GenerateOptions()}";

            return create;
        }
        internal override string GenerateOptions()
        {
            string options = base.GenerateOptions();
            if (DefautValue != "null" && ElementType!="SERIAL")
            {
                options += $" DEFAULT {DefautValue}";
            }

            if (IdentityGeneration != "null")
            {
                options += $"GENERATED {IdentityGeneration} AS IDENTITY";
            }

            return options;
        }
        internal override string Drop(InterItemCommunication communication)
        {
            if (!communication.TableToDeleteList.Contains(TableName))
            {
                return $"ALTER TABLE {TableName} DROP COLUMN IF EXISTS {Name}";
            }
            //else : no need to remove a column from a table that will be deleted!
            return string.Empty;
        }
        private string DefautValue => this["column_default"];

        private string IdentityGeneration => this["identity_generation"];

        private bool IsNullable => this["is_nullable"] =="YES";

        private int MaximumLength
        {
            get
            {
                if(int.TryParse(this["character_maximum_length"],out int maximumLength))
                {
                    return maximumLength;
                }

                return 0;
            }
        }

        internal override string Alter(ItemComparable target, InterItemCommunication communication)
        {
            StringBuilder alter = new StringBuilder();
            AddTypeModification(alter, target);
            AddNullableModification(alter, target);
            AddDefaultModification(alter, target);
            AddIdentityGenerationModification(alter, target);
            return alter.ToString();
        }

        private void AddTypeModification(StringBuilder alter, ItemComparable target)
        {
            var targetGoodType = target as Column;
            if (ElementType == target.ElementType)
            {
                if (ElementType == "character varying" && MaximumLength != targetGoodType.MaximumLength)
                {
                    if (MaximumLength < targetGoodType.MaximumLength)
                    {
                        alter.AppendLine("--!!! Truncation will occur !!!");
                    }
                    alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} TYPE {TypeStringFromElementType};");
                }
                //ELSE ???
            }
            else
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} TYPE {TypeStringFromElementType};");
            }
        }

        private void AddNullableModification(StringBuilder alter, ItemComparable target)
        {
            var targetGoodType = target as Column;
            if (IsNullable != targetGoodType.IsNullable)
            {
                if (IsNullable)
                {
                    alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} DROP NOT NULL;");
                }
                else
                {
                    alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} SET NOT NULL;");
                }

            }

        }


        private void AddDefaultModification(StringBuilder alter, ItemComparable target)
        {
            var targetGoodType = target as Column;
            if (targetGoodType.DefautValue != "null" && DefautValue == "null")
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} DROP DEFAULT;");
            }
            else if (targetGoodType.DefautValue != DefautValue)
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} SET DEFAULT {DefautValue};");
            }
            

        }

        private void AddIdentityGenerationModification(StringBuilder alter, ItemComparable target)
        {
            var targetGoodType = target as Column;
            if (targetGoodType.IdentityGeneration == "null" && IdentityGeneration != "null")
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} ADD  GENERATED  {IdentityGeneration} AS IDENTITY;");

            }
            else if (targetGoodType.IdentityGeneration != IdentityGeneration)
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} SET GENERATED  {IdentityGeneration};");
            }
            else if (targetGoodType.IdentityGeneration != "null" && IdentityGeneration == "null")
            {
                alter.AppendLine($"ALTER TABLE {TableName} ALTER COLUMN {Name} DROP IDENTITY;");
            }
        }


        
        private string TypeStringFromElementType
        {
            get
            {
                if (ElementType == "character varying")
                {
                    if (MaximumLength == 0)
                    {
                        return $"{ElementType}";
                    }
                    return $"{ElementType} ({MaximumLength})";
                }
                else
                {
                    return $"{ElementType}";
                }
            }
        }
    }
}
