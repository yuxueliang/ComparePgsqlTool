using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ComparePgsqlTool.Services
{
    [DebuggerDisplay("{TableName}.{Name}")]
    public class ItemComparable
    {

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var objGoodType = obj as ItemComparable;
            if ((Object)objGoodType == null) return false;

            if (this.AllFields.Count != objGoodType.AllFields.Count) return false;

            var equals = true;

            AllFields.ToList().ForEach(x =>
            {
                if (!objGoodType.AllFields.ContainsKey(x.Key) ||
                        objGoodType.AllFields[x.Key] != x.Value)
                {
                    equals = false;
                }
            });

            return equals;
        }
        public static bool operator ==(ItemComparable a, ItemComparable b)
        {
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(ItemComparable a, ItemComparable b)
        {
            return !(a == b);
        }

        public string Query { get; set; }
        public virtual string Key()
        {
            return this.Name.ToLowerInvariant();
        }
        public string TableName => UppercaseFormattedValue(this[Constants.TABLE_NAME]);

        public string Name => UppercaseFormattedValue(this[Constants.ELEMENT_NAME]);
        public string Definition { get { return this[Constants.DEFINITION]; } }
        public string ElementType
        {
            get { return this[Constants.ELEMENT_TYPE]; }
            set { this[Constants.ELEMENT_TYPE] = value; }
        }
        public bool IfExists { get; set; }
        public bool Cascade { get; set; }

        private string UppercaseFormattedValue(string value)
        {
            return (value.ToLowerInvariant() == value) ? value : "\"" + value + "\"";
        }
        public virtual List<ItemComparable> GetList(Database sourceDb) { return null; }
        public virtual void Fill(DbDataReader reader)
        {
            AllFields = new Dictionary<string, string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string value = "null";
                if (!(reader[i] == DBNull.Value))
                {
                    value = reader[i].ToString();
                }
                if (value == "System.String[]")
                {
                    value = String.Join(",", (string[])reader[i]);
                    if (string.IsNullOrEmpty(value))
                    {
                        value = "null";
                    }
                }
                AllFields.Add(reader.GetName(i), value);

            }
        }
        public Dictionary<string, string> AllFields { get; set; }
        public string this[string index]
        {
            get
            {
                if (AllFields.ContainsKey(index))
                    return AllFields[index];
                else
                    return string.Empty;
            }
            set
            {
                if (AllFields.ContainsKey(index))
                    AllFields[index] = value;
                else
                    AllFields.Add(index, value);

            }
        }
        internal virtual string Create(InterItemCommunication communication)
        {
            string options = GenerateOptions();
            return $"CREATE {ElementType} {Name} {options}";
        }

        internal virtual string Drop(InterItemCommunication communication)
        {
            return $"DROP {ElementType} {(IfExists ? "IF EXISTS" : "")} {Name} {(Cascade ? "CASCADE" : "")}";
        }
        internal virtual string Alter(ItemComparable target, InterItemCommunication communication)
        {
            throw new NotImplementedException();
        }
        protected class OptionValue
        {
            public string OptionName { get; set; }
            public string ComparisonValue { get; set; }
            public string ValueIfEqual { get; set; }
            public string ValueIfDifferent { get; set; }
        }
        protected List<OptionValue> OptionValueList { get; set; } = new List<OptionValue>();
        internal virtual string GenerateOptions()
        {
            StringBuilder options = new StringBuilder();
            foreach (OptionValue v in OptionValueList)
            {
                if (AllFields.ContainsKey(v.OptionName))
                {
                    if (AllFields[v.OptionName] == v.ComparisonValue)
                    {
                        options.AppendFormat(" {0} ", v.ValueIfEqual);
                    }
                    else if (!string.IsNullOrEmpty(v.ValueIfDifferent))
                    {
                        options.AppendFormat(" {0} ", v.ValueIfDifferent);
                    }

                }
            }
            return options.ToString();
        }
    }
}
