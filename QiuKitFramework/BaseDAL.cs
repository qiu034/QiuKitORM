﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;

namespace QiuKitFramework
{
    /* 
    *|--------------------------------------------------------|
    *|  这个类采用.NET Framework4.6.1作为基础框架             |
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
                        if ((dr[$"{field.Name}"].Equals(DBNull.Value) == false) && dt.Columns.Contains(field.Name))
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
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// 一般的带条件查找规则，查找所有字段并返回对应实体类的List
        /// </summary>
        /// <param name="table">数据表表名</param>
        /// <param name="model">查询类</param>
        /// <param name="isFuzzy">是否模糊查询</param>
        /// <returns></returns>
        public List<T> Select(string table, T model, bool isFuzzy = false)
        {
            try
            {
                //初始化查询字段
                string condition = " 1=1 ";

                //利用反射机制，获取字段名和字段值
                PropertyInfo[] properties = model.GetType().GetProperties();

                //判断是否模糊查询
                if (isFuzzy)
                {
                    foreach (PropertyInfo field in properties)
                    {
                        //若字段值为空，则跳过
                        if (field.GetValue(model) == null)
                        {
                            continue;
                        }
                        condition += $" AND {field.Name} LIKE '%{field.GetValue(model)}%' ";
                    }
                }
                else
                {
                    foreach (PropertyInfo field in properties)
                    {
                        //若字段值为空，则跳过
                        if (field.GetValue(model) == null)
                        {
                            continue;
                        }
                        condition += $" AND {field.Name}='{field.GetValue(model)}' ";
                    }
                }
                string strSql = SqlHelper.Instance.SELECT("*", table, condition);
                DataTable dt = SqlHelper.Instance.ExecuteDataset(connStr, strSql).Tables[0];

                List<T> list = new List<T>();
                //遍历DataTable
                foreach (DataRow dr in dt.Rows)
                {
                    T resultModel = new T();
                    foreach (PropertyInfo field in properties)  //遍历字段名
                    {
                        //若字段名在DataTable中可以找到相同的列，那么就给该字段赋值
                        if ((dr[$"{field.Name}"].Equals(DBNull.Value) == false) && dt.Columns.Contains(field.Name))
                        {
                            field.SetValue(resultModel, dr[$"{field.Name}"]);
                        }
                    }
                    list.Add(resultModel);
                }
                return list;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        /// <summary>
        /// 一般添加规则，需要传一个有数据的实体类
        /// 前提：实体类中的字段名称与数据库的完全相同
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="model">带有数据的实体类</param>
        /// <param name="checkDuplicates">是否查重</param>
        /// <returns>成功返回True</returns>
        public bool Insert(string table, T model, bool checkDuplicates = false)
        {
            try
            {
                //如果要查重，首先进行查重
                if (checkDuplicates)
                {
                    List<T> duplicatesList = Select(table, model, false);
                    if (duplicatesList.Count > 0)
                    {
                        return false;
                    }
                }

                string fields = "";
                string values = "";
                //利用反射机制，获取字段名和字段值
                PropertyInfo[] properties = model.GetType().GetProperties();
                foreach (PropertyInfo field in properties)
                {
                    //若字段值为空，则跳过
                    if (field.GetValue(model) == null)
                    {
                        continue;
                    }

                    //若为自增序列，则跳过
                    QiuKitModelAttribute customAttribute = field.GetCustomAttribute(typeof(QiuKitModelAttribute)) as QiuKitModelAttribute;
                    if (customAttribute != null && customAttribute.IsIdentity == true)
                    {
                        continue;
                    }

                    fields += $"{field.Name},";
                    values += $"'{field.GetValue(model)}',";
                }

                //若所有字段均无传值，则直接返回False
                if (string.IsNullOrEmpty(fields) || string.IsNullOrEmpty(values))
                    return false;

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
                throw new Exception(ex.ToString());
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
                throw new Exception(ex.ToString());
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
                    //若字段值为空，则跳过
                    if (field.GetValue(model) == null)
                    {
                        continue;
                    }

                    //若为自增序列，则跳过
                    QiuKitModelAttribute customAttribute = field.GetCustomAttribute(typeof(QiuKitModelAttribute)) as QiuKitModelAttribute;
                    if (customAttribute != null && customAttribute.IsIdentity == true)
                    {
                        continue;
                    }

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
                throw new Exception(ex.ToString());
            }
        }

    }
}
