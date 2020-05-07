using System.Collections.Generic;

namespace ComparePgsqlTool.Services
{
    public class Parameter
    {

        internal const string DB = "BASE";
        internal const string SERVER = "SRVR";
        internal const string PORT = "PORT";
        internal const string SCHEMA = "SCHM";
        internal const string LOGIN = "LGN";
        internal const string PASSWORD = "PWD";
        const string TARGET_SUFFIX = "_T";
        const string SOURCE_SUFFIX = "_S";

        internal Dictionary<string, string> Source { get; private set; } = new Dictionary<string, string>();
        internal Dictionary<string, string> Target { get; private set; } = new Dictionary<string, string>();
        internal Parameter(string[] args)
        {
            try
            {
                for (int i = 0; i < args.Length; i++)
                {
                    string argument = args[i];
                    string[] argumentSplit = argument.Split('=');
                    string key = argumentSplit[0];
                    string value = argumentSplit.Length > 1 ? argumentSplit[1] : string.Empty;
                    AddKeyValue(key, value);
                }
            }
            catch 
            {
                Source = new Dictionary<string, string>();
                Target = new Dictionary<string, string>();
            }
        }

        private void AddKeyValue(string key, string value)
        {
            string keyWithoutSuffix = string.Empty;
            string suffix = string.Empty;
            Dictionary<string, string> dicToAdd = null;

            if (IsSource(key))
            {
                suffix = SOURCE_SUFFIX;
                dicToAdd = Source;
            }
            else if (IsTarget(key))
            {
                suffix = TARGET_SUFFIX;
                dicToAdd = Target;
            }

            if (dicToAdd != null)
            {
                keyWithoutSuffix = key.Substring(0, key.Length - suffix.Length);
                dicToAdd.Add(keyWithoutSuffix.ToUpperInvariant(), value);
            }
            else
            {
                ApplyItemSelection(key);

            }
        }

        private void ApplyItemSelection(string key)
        {
            ItemGroupRole = key.Contains("g");
            ItemSchema = key.Contains("s");
            ItemSequence = key.Contains("S");
            ItemTable = key.Contains("t");
            ItemColumn = key.Contains("c");
            ItemIndex = key.Contains("i");
            ItemView = key.Contains("v");
            ItemComment = key.Contains("C");
            ItemFunction = key.Contains("f");
            ItemForeignKey = key.Contains("F");
            ItemOwner = key.Contains("o");
            ItemGrant = key.Contains("G");
            ItemTrigger = key.Contains("T");
        }

        private bool IsTarget(string key)
        {
            return key.EndsWith(TARGET_SUFFIX);
        }

        private bool IsSource(string key)
        {
            return key.EndsWith(SOURCE_SUFFIX);
        }

        internal bool Complete { get { return Target.Count == 6 && Source.Count == 6; } }

        public bool ItemGroupRole { get; private set; } = true;
        public bool ItemSchema { get; private set; } = true;
        public bool ItemSequence { get; private set; } = true;
        public bool ItemTable { get; private set; } = true;
        public bool ItemColumn { get; private set; } = true;
        public bool ItemIndex { get; private set; } = true;
        public bool ItemView { get; private set; } = true;
        public bool ItemComment { get; private set; } = true;
        public bool ItemFunction { get; private set; } = true;
        public bool ItemForeignKey { get; private set; } = true;
        public bool ItemOwner { get; private set; } = true;
        public bool ItemGrant { get; private set; } = true;
        public bool ItemTrigger { get; private set; } = true;

        internal string Usage()
        {
            return $@"
Usage :
=======
{System.AppDomain.CurrentDomain.FriendlyName} {{parameter}}=[value]... [Items]
SOURCE : 
       {SERVER}{SOURCE_SUFFIX} : server
       {DB}{SOURCE_SUFFIX} : database
       {PORT}{SOURCE_SUFFIX} : port 
       {SCHEMA}{SOURCE_SUFFIX} : schema
       {LOGIN}{SOURCE_SUFFIX} : login
       {PASSWORD}{SOURCE_SUFFIX} : password 
TARGET : 
       {SERVER}{TARGET_SUFFIX} : server
       {DB}{TARGET_SUFFIX} : database
       {PORT}{TARGET_SUFFIX} : port 
       {SCHEMA}{TARGET_SUFFIX} : schema
       {LOGIN}{TARGET_SUFFIX} : login
       {PASSWORD}{TARGET_SUFFIX} : password
Items 
Any of the caracter representing an item comparison. If none given ALL will be assumed.
        g : group roles
        s : schema
        S : sequence
        t : table
        c : column
        i : index
        v : view
        C : comment
        f : functions
        F : foreign key
        o : owner
        G : grant
        T : trigger
ex : 
{System.AppDomain.CurrentDomain.FriendlyName} {SERVER}{SOURCE_SUFFIX}=localhost {DB}{SOURCE_SUFFIX}=bd1 {PORT}{SOURCE_SUFFIX}=5432 {SCHEMA}{SOURCE_SUFFIX}=public {LOGIN}{SOURCE_SUFFIX}=user1 {PASSWORD}{SOURCE_SUFFIX}=***** {SERVER}{TARGET_SUFFIX}=server {DB}{TARGET_SUFFIX}=bd1 {PORT}{TARGET_SUFFIX}=5433 {SCHEMA}{TARGET_SUFFIX}=public {LOGIN}{TARGET_SUFFIX}=user1 {PASSWORD}{TARGET_SUFFIX}=*****";
        }
    }
}
