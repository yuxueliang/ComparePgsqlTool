using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ComparePgsqlTool.Services;

namespace ComparePgsqlTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private static InterItemCommunication Communication;

        
        private void button1_Click(object sender, EventArgs e)
        {
            Communication  = new InterItemCommunication();

            var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");

            string sql = CompareSchema(DBHelper.GetSourceDatabase(), DBHelper.GetTargetDatabase());

            string tempPath = $@"D:\sql\temp\updateschema{fileName}.sql";
            SaveTxtFile(tempPath, sql);


            List<string> sqlLines = File.ReadAllLines(tempPath).Where(o => string.IsNullOrWhiteSpace(o) == false).ToList();


            Table table = new Table();
            List<Table> sourceList = ReturnItemList<Table>(DBHelper.GetSourceDatabase(), table.Query);

            List<string> orderSqls = new List<string>();

            var i = 0;
            foreach (var sourceTable in sourceList)
            {
                i++;
                var sqls = sqlLines.Where(o => o.Contains(sourceTable.Name)).ToList();
                if (sqls.Any())
                {
                    orderSqls.AddRange(sqls);
                    orderSqls.Add(i.ToString());
                }
            }

            orderSqls = orderSqls.Distinct().ToList();

            string path = $@"D:\sql\updateschema{fileName}.sql";
            foreach (var orderSql in orderSqls)
            {
                if (int.TryParse(orderSql, out i))
                {
                    SaveTxtFile(path, "");
                }
                else
                {
                    SaveTxtFile(path, orderSql);

                }
            }



            if (string.IsNullOrWhiteSpace(sql))
            {
                MessageBox.Show("没有要修改的sql脚本");
            }
            else
            {
                MessageBox.Show($"脚本路径{path}");
            }
        }



        private string CompareSchema(Database sourceDb, Database targetDb)
        {
            StringBuilder sb = new StringBuilder();
            //var sql = Compare<Schema>(sourceDb, targetDb);
            //if (string.IsNullOrWhiteSpace(sql)==false)
            //{
            //    sb.AppendLine(sql);
            //}
          
            var sql = Compare<Table>(sourceDb, targetDb);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }
            sql = Compare<Column>(sourceDb, targetDb);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }
            sql = Compare<Index>(sourceDb, targetDb);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            } 
            sql = Compare<Sequence>(sourceDb, targetDb);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }
            sql = Compare<Services.View>(sourceDb, targetDb);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }
            return sb.ToString();
        }

        private string Compare<T>(Database sourceDb, Database targetDb) where T : ItemComparable, new()
        {
            StringBuilder sb = new StringBuilder();


            T item = new T();

            List<T> sourceList = ReturnItemList<T>(sourceDb, item.Query);
            List<T> targetList = ReturnItemList<T>(targetDb, item.Query);

            var sql = CompareItemToCreate(sourceList, targetList);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }
            sql = CompareItemToDrop(sourceList, targetList);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }
            sql = CompareItemToAlter(sourceList, targetList);
            if (string.IsNullOrWhiteSpace(sql) == false)
            {
                sb.AppendLine(sql);
            }

            return sb.ToString();

        }

        private string CompareItemToAlter<T>(List<T> sourceList, List<T> targetList) where T : ItemComparable, new()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var t in sourceList)
            {
                var tCible = targetList.FirstOrDefault(y => y.Key() == t.Key());
                if (tCible != null && t != tCible)
                {
                    var instruction = t.Alter(tCible, Communication);
                    if (!string.IsNullOrEmpty(instruction))
                    {
                        if (!instruction.EndsWith(";") && !instruction.EndsWith(";\r\n"))
                            instruction += ";";
                        sb.AppendLine(instruction);
                    }

                }
            }
            return sb.ToString();
        }

        private string CompareItemToDrop<T>(List<T> sourceList, List<T> targetList) where T : ItemComparable, new()
        {
            StringBuilder sb = new StringBuilder();

            GetMissingItem(targetList, sourceList)
                .ForEach(x =>
                {
                    var instruction = x.Drop(Communication);
                    if (!string.IsNullOrEmpty(instruction))
                    {
                        if (!instruction.EndsWith(";") && !instruction.EndsWith(";\r\n"))
                            instruction += ";";
                        sb.AppendLine(instruction);
                    }
                    AddDropCommunication(x);
                });
            return sb.ToString();
        }
        private List<T> GetMissingItem<T>(List<T> itemList, List<T> listToSearch) where T : ItemComparable, new()
        {
            return itemList
                .Where(x => MissingItem(x, listToSearch))
                .ToList();


        }
        private  bool MissingItem<T>(ItemComparable x, List<T> list) where T : ItemComparable, new()
        {
            return list.All(y => y.Key() != x.Key());
        }

        private string CompareItemToCreate<T>(List<T> sourceList, List<T> targetList) where T : ItemComparable, new()
        {
            StringBuilder sb = new StringBuilder();

            GetMissingItem(sourceList, targetList)
                .ForEach(x =>
                {
                    var instruction = x.Create(Communication);
                    if (!string.IsNullOrEmpty(instruction))
                    {
                        if (!instruction.EndsWith(";") && !instruction.EndsWith(";\r\n"))
                            instruction += ";";
                        sb.AppendLine(instruction);
                    }

                    AddCreateCommunication(x);
                });
            return sb.ToString();
        }

        private  void AddCreateCommunication<T>(T addedItem) where T : ItemComparable, new()
        {
            if (addedItem is Table || addedItem is Services.View)
            {
                Communication.TableToDeleteList.Add(addedItem.Name);
            }
        }

        private static void AddDropCommunication<T>(T droppedItem) where T : ItemComparable, new()
        {
            if (droppedItem is Table || droppedItem is Services.View)
            {
                Communication.TableToDeleteList.Add(droppedItem.Name);
            }
        }


        /// <summary>
        /// C# 用 Environment.NewLine 换行
        /// </summary>
        /// <param name="filePath">文本文件路径</param>
        public static void SaveTxtFile(string filePath, string content)
        {
            var dirPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(content);
                sw.Flush();
                sw.Close();
                fs.Close();
            }

        }



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

        private void button3_Click(object sender, EventArgs e)
        {
            var fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
            string sql = DataCompare.GetDataChanges(DBHelper.GetSourceDatabase(), DBHelper.GetTargetDatabase());
            string path = $@"D:\sql\updatedata{fileName}.sql";
            SaveTxtFile(path, sql);
            if (string.IsNullOrWhiteSpace(sql))
            {
                MessageBox.Show("没有要修改的sql脚本");
            }
            else
            {
                MessageBox.Show($"脚本路径{path}");
            }
        }



    }

    
}
