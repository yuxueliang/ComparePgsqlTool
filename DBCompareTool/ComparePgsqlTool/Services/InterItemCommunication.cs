using System.Collections.Generic;

namespace ComparePgsqlTool.Services
{
    public class InterItemCommunication
    {
        public List<string> TableToDeleteList { get; } = new List<string>();
        public List<string> TableToAddList{ get; } = new List<string>();
    }
}
