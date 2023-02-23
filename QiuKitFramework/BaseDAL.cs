using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace QiuKitFramework
{
    public class BaseDAL<T>
    {
        private static BaseDAL<T> _Instance = null;
        public static BaseDAL<T> Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new BaseDAL<T>();
                return _Instance;
            }
        }

        private string _connStr;
        public string connStr
        {
            get { return _connStr; }
            set { _connStr = value; }
        }

        /// <summary>
        /// 一般的查找规则，查找所有字段并返回一个DataTable
        /// </summary>
        /// <param name="table">数据表表名</param>
        /// <param name="condition">查询条件，不带Where</param>
        /// <returns></returns>
        private DataTable Select(string table,string condition)
        {
            try
            {
                string strSql = SqlHelper.Instance.SELECT("*", table, condition);
                DataTable dt = SqlHelper.Instance.ExecuteDataset(connStr, strSql).Tables[0];
                return dt;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 一般添加规则，需要传一个有数据的实体类
        /// 前提：实体类中的字段名称与数据库的完全相同
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="model">带有数据的实体类</param>
        /// <returns>成功返回True</returns>
        private bool Insert(string table,T model)
        {
            try
            {
                string fields = "";
                string values = "";
                //利用反射机制，获取字段名和字段值
                PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (PropertyInfo field in properties)
                {
                    fields += $"{field.Name},";
                    values += $"'{field.GetValue(field.Name)}',";
                }
                //去除最后一个逗号
                fields.TrimEnd(',');
                values.TrimEnd(',');

                string strSql = SqlHelper.Instance.INSERT(table,fields,values);
                int result = SqlHelper.Instance.ExecuteNonQuery(connStr,strSql);
                if(result >0)
                        return true;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 一般删除规则
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="condition">删除条件，不带Where</param>
        /// <returns></returns>
        private bool Delete(string table, string condition)
        {
            try
            {
                string strSql = SqlHelper.Instance.DELETE(table, condition);
                int result = SqlHelper.Instance.ExecuteNonQuery(connStr, strSql);
                if (result > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 一般修改规则,需要传一个有数据的实体类,传进的实体类为修改后的值
        /// 前提：实体类中的字段名称与数据库的完全相同
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="model"></param>
        /// <param name="condition">新增条件，不带Where</param>
        /// <returns></returns>
        private bool Update(string table, T model,string condition)
        {
            try
            {
                string fields = "";
                //利用反射机制，获取字段名和字段值
                PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (PropertyInfo field in properties)
                {
                    fields += $"{field.Name}='{field.GetValue(field.Name)}',";
                }
                //去除最后一个逗号
                fields.TrimEnd(',');

                string strSql = SqlHelper.Instance.UPDATE(table,fields,condition);
                int result = SqlHelper.Instance.ExecuteNonQuery(connStr, strSql);
                if (result > 0)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
