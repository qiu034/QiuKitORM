using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace QiuKitCore
{
    /* 
    *|--------------------------------------------------------|
    *|  这个类采用.NET Core3.1作为基础框架                    |
    *|  这个是仓储层的一个基类，自带一般规则下的增删改查方法  |
    *|  所有仓储层类都要继承这个类，T的类型为对应的实体类     |
    *|  类中需要声明常量，如table=对应的数据库表名            |
    *|--------------------------------------------------------|
    *                                                   By Qjh
    */
    public class BaseDAL<T> where T : class, new()
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
        /// 一般的查找规则，查找所有字段并返回对应实体类的List
        /// </summary>
        /// <param name="table">数据表表名</param>
        /// <param name="condition">查询条件，不带Where</param>
        /// <returns></returns>
        public List<T> Select(string table, string condition)
        {
            try
            {
                string strSql = SqlHelper.Instance.SELECT("*", table, condition);
                DataTable dt = SqlHelper.Instance.ExecuteDataset(connStr, strSql).Tables[0];

                List<T> list = new List<T>();
                //遍历DataTable
                foreach (DataRow dr in dt.Rows)
                {
                    T model = new T();
                    //利用反射机制，获取实体类的字段名和字段值
                    PropertyInfo[] properties = model.GetType().GetProperties();
                    foreach (PropertyInfo field in properties)  //遍历字段名
                    {
                        //若字段名在DataTable中可以找到相同的列，那么就给该字段赋值
                        if (dt.Columns.Contains(field.Name))
                        {
                            field.SetValue(model, dr[$"{field.Name}"]);
                        }
                    }
                    list.Add(model);
                }
                return list;
            }
            catch (Exception ex)
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
        public bool Insert(string table, T model)
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
                    values += $"'{field.GetValue(model)}',";
                }
                //去除最后一个逗号
                fields = fields.TrimEnd(',');
                values = values.TrimEnd(',');

                string strSql = SqlHelper.Instance.INSERT(table, fields, values);
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
        /// 一般删除规则
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="condition">删除条件，不带Where</param>
        /// <returns></returns>
        public bool Delete(string table, string condition)
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
        /// <param name="model">带有数据的实体类(作为修改后的值)</param>
        /// <param name="condition">新增条件，不带Where</param>
        /// <returns></returns>
        public bool Update(string table, T model, string condition)
        {
            try
            {
                string fields = "";
                //利用反射机制，获取字段名和字段值
                PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (PropertyInfo field in properties)
                {
                    fields += $"{field.Name}='{field.GetValue(model)}',";
                }
                //去除最后一个逗号
                fields = fields.TrimEnd(',');

                string strSql = SqlHelper.Instance.UPDATE(table, fields, condition);
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
