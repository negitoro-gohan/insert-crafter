using System;
using System.IO;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;
using Microsoft.IdentityModel.Tokens;

namespace insert_crafter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string server = GetArgumentValue(args, "server");
            string db = GetArgumentValue(args, "db");
            string user = GetArgumentValue(args, "user");
            string pass = GetArgumentValue(args, "pass");
            string obj = GetArgumentValue(args, "table");


            if (String.IsNullOrEmpty(server))
            {
                Console.WriteLine("インスタンスを指定してください。");
                return;
            }
            if (String.IsNullOrEmpty(db))
            {
                Console.WriteLine("データベースを指定してください。");
                return;
            }
           
            try
            {
                 CreateScript(server, db, user, pass, obj);
                Console.WriteLine("出力が完了しました。");
            }
            catch (Exception ex)
            {
                Console.WriteLine("エラーが発生しました: " + ex.Message);
            }

        }

        static void WriteTextToFile(string text, string filePath)
        {

            if (String.IsNullOrEmpty(text))
            {
                return;
            }
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(text);
            }
        }
        static void CreateScript(string serverInstance, string dbName, string login, string password, string obj)
        {

            ServerConnection srvConn = new ServerConnection();
            srvConn.ServerInstance = serverInstance;   // connects to named instance
            if(login.IsNullOrEmpty() || password.IsNullOrEmpty())
            {
                srvConn.LoginSecure = true;
            }
            else
            {
                srvConn.LoginSecure = false;   // set to true for Windows Authentication  
                srvConn.Login = login;
                srvConn.Password = password;
            }
    
            Server srv = new Server(srvConn);

            // Reference the database.    
            Database db = srv.Databases[dbName];

            // Define a Scripter object and set the required scripting options.   
            Scripter scrp = new Scripter(srv);
            scrp.Options.ScriptSchema = false;
            scrp.Options.ScriptData = true; // データを出力する  
            scrp.Options.NoCommandTerminator = true; 
            scrp.Options.ToFileOnly = true;

            StringBuilder sb = new StringBuilder();

            foreach (Table dbObj in db.Tables)
            {
                if ((String.IsNullOrEmpty(obj)) || (dbObj.Name.IndexOf(obj, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    if (!dbObj.IsSystemObject)
                    {
                        scrp.Options.FileName = dbObj.Schema + "." + dbObj.Name + ".sql";
                        scrp.EnumScript(new[] { dbObj });
                    }
                }
            }
   
        }

        public static string GetArgumentValue(string[] args, string argumentName)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-" + argumentName && i + 1 < args.Length)
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
