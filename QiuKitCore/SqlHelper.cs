using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace QiuKitCore
{
    public class SqlHelper
    {
        private static SqlHelper _Instance = null;
        public static SqlHelper Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SqlHelper();
                return _Instance;
            }
        }

        #region ExecuteDataset

        public DataSet ExecuteDataset(string connectionString, string commandText)
        {
            return ExecuteDataset(connectionString, CommandType.Text, commandText);
        }

        public DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteDataset(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public DataSet ExecuteDataset(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return ExecuteDataset(connection, commandType, commandText, commandParameters);
            }
        }

        public DataSet ExecuteDataset(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            // 预处理  
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行并返回DataSet
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                DataSet ds = new DataSet();
                da.Fill(ds);
                cmd.Parameters.Clear();
                if (mustCloseConnection)
                    connection.Close();
                return ds;
            }
        }

        #endregion ExecuteDataset

        #region ExecuteNonQuery

        public int ExecuteNonQuery(string connectionString, string commandText)
        {
            return ExecuteNonQuery(connectionString, CommandType.Text, commandText);
        }

        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            return ExecuteNonQuery(connectionString, commandType, commandText, (SqlParameter[])null);
        }

        public int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connectionString == null || connectionString.Length == 0) throw new ArgumentNullException("connectionString");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                return ExecuteNonQuery(connection, commandType, commandText, commandParameters);
            }
        }

        public int ExecuteNonQuery(SqlConnection connection, CommandType commandType, string commandText, params SqlParameter[] commandParameters)
        {
            if (connection == null) throw new ArgumentNullException("connection");
            // 创建SqlCommand命令,并进行预处理  
            SqlCommand cmd = new SqlCommand();
            bool mustCloseConnection = false;
            PrepareCommand(cmd, connection, (SqlTransaction)null, commandType, commandText, commandParameters, out mustCloseConnection);

            // 执行cmd.ExecuteNonQuery
            int retval = cmd.ExecuteNonQuery();

            // 清除参数,以便再次使用.  
            cmd.Parameters.Clear();
            if (mustCloseConnection)
                connection.Close();
            return retval;
        }

        #endregion

        #region ConnTest

        /// <summary>
        /// 测试服务器连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public bool ConnTest(string host, string user, string pwd)
        {
            return ConnTest(host, "master", user, pwd);
        }

        /// <summary>
        /// 测试库连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="master"></param>
        /// <param name="user"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public bool ConnTest(string host, string master, string user, string pwd)
        {
            bool result = false;
            string sqlConn = string.Format("Data Source={0};Initial Catalog={1};User ID={2};password={3}", host, master, user, pwd);
            using (SqlConnection connection = new SqlConnection(sqlConn))
            {
                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }
                    result = true;
                }
                catch (Exception ex)
                {

                    throw new Exception("连接失败！" + ex.Message);
                }

            }
            return result;
        }

        #endregion

        #region 定制方法

        /// <summary>
        /// 获取字段和类型并且转化为C#中的类型
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetModelStr(string tableName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(" select  ");
            stringBuilder.Append(" col.name ColumnName, column_id ColumnId, is_identity,  ");
            stringBuilder.Append(" column_id ColumnId, ");
            stringBuilder.Append(" case typ.name  when 'bigint' then 'long'  when 'binary' then 'byte[]'  when 'bit' then 'bool' " +
                " \r\n when 'char' then 'string'  when 'date' then 'DateTime'  when 'datetime' then 'DateTime'  " +
                " \r\n when 'datetime2' then 'DateTime'  when 'datetimeoffset' then 'DateTimeOffset'  when 'decimal' then 'decimal?' " +
                " \r\n when 'float' then 'float?'  when 'image' then 'byte[]'  when 'int' then 'int?'  when 'money' then 'decimal?' " +
                " \r\n when 'nchar' then 'string'  when 'ntext' then 'string'  when 'numeric' then 'decimal?'  when 'nvarchar' then 'string'" +
                " \r\n when 'real' then 'double?'  when 'smalldatetime' then 'DateTime'  when 'smallint' then 'short?'  " +
                " \r\n when 'smallmoney' then 'decimal'  when 'text' then 'string'  when 'time' then 'TimeSpan'  " +
                " \r\n when 'timestamp' then 'DateTime'  when 'tinyint' then 'byte'  when 'uniqueidentifier' then 'Guid'  " +
                " \r\n when 'varbinary' then 'byte[]'  when 'varchar' then 'string'  else 'UNKNOWN_' + typ.name end ColumnType");
            stringBuilder.Append(" from sys.columns col,sys.types typ ");
            stringBuilder.Append(" where");
            stringBuilder.Append(" col.system_type_id = typ.system_type_id ");
            stringBuilder.Append(" AND col.user_type_id = typ.user_type_id ");
            stringBuilder.Append($" AND object_id = object_id('{tableName}')");
            stringBuilder.Append("  ");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// SELECT {fields} FROM {table} WHERE {condition}
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string SELECT(string fields, string table, string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return $"SELECT {fields} FROM {table}";
            return $"SELECT {fields} FROM {table} WHERE {condition} ";
        }

        /// <summary>
        /// INSERT INTO {table}({fields}) VALUES({values}
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public string INSERT(string table, string fields, string values)
        {
            return $"INSERT INTO {table}({fields}) VALUES({values})";
        }

        /// <summary>
        /// DELETE FROM {table} WHERE{condition}
        /// </summary>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string DELETE(string table, string condition)
        {
            return $"DELETE FROM {table} WHERE {condition}";
        }

        /// <summary>
        /// UPDATE {table} SET {fields} WHERE {condition}
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fields"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public string UPDATE(string table, string fields, string condition)
        {
            return $"UPDATE {table} SET {fields} WHERE {condition}";
        }



        #endregion

        #region 私有方法

        /// <summary>
        /// 预处理Sqlcommand
        /// </summary>
        /// <param name="command">Sql命令</param>
        /// <param name="connection">Sql连接</param>
        /// <param name="transaction">Sql事务</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandText">命令，Sql语句</param>
        /// <param name="commandParameters">参数数组</param>
        /// <param name="mustCloseConnection">是否需要关闭</param>
        private void PrepareCommand(SqlCommand command, SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, out bool mustCloseConnection)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandText == null || commandText.Length == 0) throw new ArgumentNullException("commandText");
            if (connection.State != ConnectionState.Open)
            {
                mustCloseConnection = true;
                connection.Open();
            }
            else
            {
                mustCloseConnection = false;
            }
            // 给命令分配一个数据库连接.  
            command.Connection = connection;
            // 设置命令文本(存储过程名或SQL语句)  
            command.CommandText = commandText;
            // 分配事务  
            if (transaction != null)
            {
                if (transaction.Connection == null) throw new ArgumentException("The transaction was rollbacked or commited, please provide an open transaction.", "transaction");
                command.Transaction = transaction;
            }
            // 设置命令类型 
            command.CommandType = commandType;
            // 分配参数  
            if (commandParameters != null)
            {
                AttachParameters(command, commandParameters);
            }
            return;
        }

        /// <summary>
        /// 将SqlParameter参数数组分配给SqlCommand
        /// </summary>
        /// <param name="command"></param>
        /// <param name="commandParameters"></param>
        private void AttachParameters(SqlCommand command, SqlParameter[] commandParameters)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (commandParameters != null)
            {
                foreach (SqlParameter p in commandParameters)
                {
                    if (p != null)
                    {
                        if ((p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Input) && p.Value == null)
                        {
                            p.Value = DBNull.Value;
                        }
                        command.Parameters.Add(p);
                    }
                }
            }
        }

        #endregion 
    }
}
