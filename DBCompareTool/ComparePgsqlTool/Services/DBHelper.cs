using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparePgsqlTool.Services
{
    public class DBHelper
    {
        public static Database GetSourceDatabase()
        {
            return new Database(ConfigurationManager.ConnectionStrings["sourceDB"].ToString());
        }

        public static Database GetTargetDatabase()
        {
            return new Database(ConfigurationManager.ConnectionStrings["targetDB"].ToString());
        }
    }
}
