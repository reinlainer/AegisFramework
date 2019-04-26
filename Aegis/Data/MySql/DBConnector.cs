using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Aegis.Calculate;



namespace Aegis.Data.MySQL
{
    internal sealed class DBConnector
    {
        public MySqlConnection Connection { get; private set; }
        public string ConnectionString { get { return Connection.ConnectionString; } }
        public IntervalCounter QPS { get; private set; }





        internal DBConnector()
        {
        }


        public void Connect(string hostIp, int hostPortNo, string charSet, string dbName, string user, string pwd, int commandTimeoutSec = 60)
        {
            if (Connection != null)
                return;


            string connStr = $"Server={hostIp};Port={hostPortNo};Uid={user};Pwd={pwd};";
            if (charSet != null)
                connStr += $"CharSet={charSet};";
            if (dbName != null)
                connStr += $"Database={dbName};";


            QPS = new IntervalCounter(1000);
            Connection = new MySqlConnection(connStr);
            Connection.Open();


            /*using (DBCommand cmd = DBCommand.NewCommand(MySql))
            {
                cmd.QueryNoReader("set transaction isolation level read uncommitted;");
                cmd.QueryNoReader("set session wait_timeout=604800;set session interactive_timeout=604800;");
            }*/
        }


        public void Close()
        {
            if (Connection != null)
            {
                Connection.Close();
                Connection = null;
            }
        }


        public void Ping()
        {
            if (Connection != null)
                Connection.Ping();
        }
    }
}
