using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePgsqlTool.Services
{
    public class SchemaCompare
    {


        //public static string GetSchemaChanges()
        //{
       

        //    var returnSQL = new StringBuilder();


        //    Table table = new Table();
        //    List<Table> sourceTables = ReturnItemList<Table>(DBHelper.GetSourceDatabase(), table.Query);
        //    List<Table> targetTables = ReturnItemList<Table>(DBHelper.GetTargetDatabase(), table.Query);

        //    foreach (var sourceTable in sourceTables)
        //    {
                
        //    }


        //}


        private static List<T> ReturnItemList<T>(Database db, string query) where T : ItemComparable, new()
        {
            List<T> list = new List<T>();
            query = query.Replace(Constants.REPLACEMENT_SCHEMA, db.Schema);
            using (var con = new Npgsql.NpgsqlConnection(db.ConnectionString()))
            {
                using (var command = con.CreateCommand())
                {
                    command.CommandText = query;

                    con.Open();

                    DbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        T item = new T();
                        item.Fill(reader);
                        list.Add(item);
                    }
                }
            }

            return list;
        }


    }
}
