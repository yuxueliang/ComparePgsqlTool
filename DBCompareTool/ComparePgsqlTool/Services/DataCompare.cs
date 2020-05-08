using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Npgsql;

namespace ComparePgsqlTool.Services
{
    public class DataCompare
    {
        private static Database sourceDb = null;
        static Database targetDb = null;

        private static Dictionary<string, List<string>> compareTables = new Dictionary<string, List<string>>();


        static DataCompare()
        {
           var compareTable = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datacompare.js"));

           compareTables = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(compareTable);
            //compareTables.Add("billcodelibs", new List<string>() {"id"});
            //compareTables.Add("billcodes", new List<string>() { "id" });
            //compareTables.Add("buildingcategories", new List<string>() {"id"});
            //compareTables.Add("cities", new List<string>() {"id"});
            ////compareTables.Add("civilsettings", new List<string>() {"cid"});
            //compareTables.Add("civilsettings", new List<string>() { "id" });

            //compareTables.Add("countries", new List<string>() {"id"});
            //compareTables.Add("districts", new List<string>() {"id"});
            //compareTables.Add("floorsettings", new List<string>() {"id"});
            //compareTables.Add("jointmethods", new List<string>() {"id"});
            ////compareTables.Add("nodeatlases", new List<string>() {"value"});
            //compareTables.Add("nodeatlases", new List<string>() { "id" });
            //compareTables.Add("projectstages", new List<string>() {"id"});
            //compareTables.Add("propertyassociation", new List<string>() {"id"});
            //compareTables.Add("propertydefaults", new List<string>() {"id"});
            //compareTables.Add("propertytriggers", new List<string>() {"id"});
            //compareTables.Add("protectionlayersettingrules", new List<string>() {"id"});
            //compareTables.Add("provinces", new List<string>() {"id"});
            ////compareTables.Add("qspropertydefinitions", new List<string>() {"keyword"});
            //compareTables.Add("qspropertydefinitions", new List<string>() { "id" });

            ////compareTables.Add("qspropertyoptions", new List<string>() {"optionkeyword"});
            //compareTables.Add("qspropertyoptions", new List<string>() { "id" });

            //compareTables.Add("qstypebillcoderulelibs", new List<string>() {"id"});
            //compareTables.Add("qstypebillcoderules", new List<string>() {"id"});
            //compareTables.Add("qstypejoinrules", new List<string>() {"id"});
            //compareTables.Add("qstypejoints", new List<string>() {"id"});
            //compareTables.Add("qstypelibs", new List<string>() {"id"});
            //compareTables.Add("qstypemaprulelibs", new List<string>() {"id"});
            //compareTables.Add("qstypemaprules", new List<string>() {"id"});
            ////compareTables.Add("qstypeproperties", new List<string>() {"qstype"});
            //compareTables.Add("qstypeproperties", new List<string>() { "id" });

            ////compareTables.Add("qstypes", new List<string>() {"keyword"});
            //compareTables.Add("qstypes", new List<string>() { "id" });

            //compareTables.Add("rebaranchorages", new List<string>() {"id"});
            //compareTables.Add("rebarattritionrates", new List<string>() {"id"});
            //compareTables.Add("rebarcalculatesettings", new List<string>() {"id"});
            //compareTables.Add("rebarcovers", new List<string>() {"id"});
            //compareTables.Add("rebardatadefinitions", new List<string>() {"id"});
            //compareTables.Add("rebardensities", new List<string>() {"id"});
            //compareTables.Add("rebarhooks", new List<string>() {"id"});
            //compareTables.Add("rebarlapjointsettings", new List<string>() {"id"});
            //compareTables.Add("releaseversions", new List<string>() {"id"});
            //compareTables.Add("reportconfigs", new List<string>() {"id"});
            //compareTables.Add("unitprecisions", new List<string>() {"id"});
            //compareTables.Add("wallthickdefinitions", new List<string>() {"id"});


            //string s = JsonConvert.SerializeObject(compareTables);

        }




        public static string GetDataChanges(Database sourceDatabase, Database targetDatabase)
        {
            sourceDb = sourceDatabase;
            targetDb = targetDatabase;

            var returnSQL = new StringBuilder();
          
            foreach (var tableName in compareTables.Keys)
            {
                var result = CompareTable(tableName);
                if (result.Length > 0)
                {
                    returnSQL.AppendLine(result);
                }
            }

            return returnSQL.ToString();
        }

       

        private static string CompareTable(string tableName)
        {

            string sql = $"select * from {tableName}";

            var sourceData = GetDataTable(sourceDb, sql);
            var targetData = GetDataTable(targetDb, sql);
            return CompareDataTables(sourceData, targetData, tableName);

        }

        private static string CompareDataTables(
            List<DataRow> sourceDataTable,
            List<DataRow> targetDataTable,
            string tableName
        )
        {
            var returnSQL = new StringBuilder();
            bool hasInsert = false;

            var pkColumns = compareTables.First(o => o.Key == tableName).Value;

            foreach (DataRow targetRow in targetDataTable)
            {
                Dictionary<string,object> values = new Dictionary<string, object>();
                foreach (var pkColumn in pkColumns)
                {
                    values.Add(pkColumn, targetRow.Columns.First(o => o.Name == pkColumn).Value);
                }

                DataRow sourceRow = null;
                foreach (var row in sourceDataTable)
                {
                    var i = 0;
                    foreach (var value in values)
                    {
                        if (row.Columns.First(o => o.Name == value.Key).Value.ToString() != value.Value.ToString())
                        {
                            i = 1;
                            break;
                        }
                    }

                    if (i == 0)
                    {
                        sourceRow = row;
                        break;
                    }
                }

                if (sourceRow == null)
                {
                    returnSQL.AppendLine(ScriptDelete(targetRow, tableName));
                }
            }

            foreach (var sourceRow in sourceDataTable)
            {
                Dictionary<string, object> values = new Dictionary<string, object>();
                foreach (var pkColumn in pkColumns)
                {
                    values.Add(pkColumn, sourceRow.Columns.First(o => o.Name == pkColumn).Value);
                }


                DataRow targetRow = null;
                foreach (var row in targetDataTable)
                {
                    var i = 0;
                    foreach (var value in values)
                    {
                        if (row.Columns.First(o => o.Name == value.Key).Value.ToString() != value.Value.ToString())
                        {
                            i = 1;
                            break;
                        }
                    }

                    if (i == 0)
                    {
                        targetRow = row;
                        break;
                    }

                }

                if (targetRow == null)
                {
                    returnSQL.AppendLine(ScriptInsert(sourceRow, tableName));
                    hasInsert = true;
                }
                else
                {
                    string updateSQL = ScriptUpdate(sourceRow, targetRow, tableName);
                    if (updateSQL.Length > 0)
                    {
                        returnSQL.AppendLine(updateSQL);
                    }
                }
            }

            

            //if (hasInsert && DatabaseCommon.DatabaseSchema.HasIdentityColumn(sourceTable))
            //{
            //    string identityClause = "SET IDENTITY_INSERT " + sourceTable.Schema + "." + sourceTable.Name;
            //    returnSQL.Insert(0, identityClause + " ON;" + Environment.NewLine + Environment.NewLine);
            //    returnSQL.AppendLine(identityClause + " OFF;");
            //}
            return returnSQL.ToString();
        }

        private static List<string> GetPkColumns(string tableName)
        {
            return compareTables.FirstOrDefault(o => o.Key == tableName).Value ?? new List<string>() {"id"};
        }







        public static List<DataRow> GetDataTable(Database db, string sql)
        {
            var rows = new List<DataRow>();

            using (var con = new NpgsqlConnection(db.ConnectionString()))
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText = sql;

                    con.Open();

                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        var row = new DataRow();
                        var columns = reader.GetColumnSchema();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Columns.Add(new DataColumn()
                            {
                                Name = columns[i].ColumnName,
                                Value = reader[i]
                            });

                        }

                        rows.Add(row);
                    }

                }
            }

            return rows;
        }



        private static string ScriptInsert(
            DataRow dataRow,
            string tableName
        )
        {
            var returnSQL = new StringBuilder();
            var valuesList = new StringBuilder();

            returnSQL.Append("INSERT INTO ");
            //returnSQL.Append("[" + table.Schema + "].[" + table.Name + "](");
            returnSQL.Append($"{tableName}(");

            bool firstColumn = true;
            foreach (DataColumn column in dataRow.Columns)
            {
                if (column.Value is DBNull)
                {
                    continue;
                }

                if (!firstColumn)
                {
                    returnSQL.Append(", ");
                    valuesList.Append(", ");
                }


                returnSQL.Append(column.Name);
                valuesList.Append(HandleValue(column.Value));


                firstColumn = false;
            }

            returnSQL.Append(") ");
            returnSQL.AppendLine("VALUES (" + valuesList + ");");

            Console.WriteLine(returnSQL.ToString());
            return returnSQL.ToString();
        }


        private static string ScriptUpdate(
            DataRow sourceRow,
            DataRow targetRow,
            string tableName
        )
        {
            var updateSQL = new StringBuilder();

            bool firstColumn = true;

            var pkColumns = compareTables.First(o => o.Key == tableName).Value;

            foreach (DataColumn column in sourceRow.Columns)
            {
                if (column.Name.ToLower() == "id") continue;
                if (pkColumns.Contains(column.Name)) continue;

                if (column.Value.ToString() != targetRow.Columns.First(o => o.Name == column.Name).Value.ToString())
                {
                    if (!firstColumn)
                    {
                        updateSQL.Append(", ");
                    }

                    string updateValue = HandleValue(column.Value);

                    updateSQL.Append($"{column.Name}={updateValue}");

                    firstColumn = false;
                }
            }

            if (updateSQL.ToString().Length == 0)
            {
                return string.Empty;
            }

            var returnSQL = new StringBuilder();
            returnSQL.Append($"UPDATE {tableName} SET ");
            returnSQL.Append(updateSQL);
            returnSQL.AppendLine(" WHERE " + GetWhere(sourceRow, tableName) + ";");

            Console.WriteLine(returnSQL.ToString());

            return returnSQL.ToString();
        }

        private static string ScriptDelete(
            DataRow targetRow,
            string tableName
        )
        {
            var returnSQL = new StringBuilder();
            returnSQL.Append($"DELETE FROM {tableName}");
            returnSQL.AppendLine(" WHERE " + GetWhere(targetRow, tableName) + ";");

            Console.WriteLine(returnSQL.ToString());
            return returnSQL.ToString();
        }


        private static string GetWhere(DataRow dataRow, string tableName)
        {
            var pkColumns = compareTables.First(o => o.Key == tableName).Value;
            
            string where = "";
            foreach (string columnName in pkColumns)
            {
                if (where.Length > 0)
                {
                    where += " and ";
                }

                var pkValue = dataRow.Columns.First(o => o.Name == columnName).Value;



                where += columnName + " = " + HandleValue(pkValue);
            }
            return where;
        }



        private static string HandleValue(object value)
        {
            string result;
            if (value is DBNull)
            {
                result = "NULL";
            }
            else if (value is string || value is DateTime)
            {
                result = $"'{value.ToString().Replace("'", "''")}'";
            }
            else
            {
                result = $"{value}";

            }

            return result;
        }

    }

    

    public class DataRow
    {
        public List<DataColumn> Columns { get; set; }=new List<DataColumn>();
    }

    public class DataColumn
    {
        public string Name { get; set; }

        public object Value { get; set; }
    }
}
