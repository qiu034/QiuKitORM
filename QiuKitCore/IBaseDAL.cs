using System;
using System.Collections.Generic;
using System.Text;

namespace QiuKitCore
{
    public interface IBaseDAL<T>
    {
        /// <summary>
        /// 一般的查找规则，查找所有字段并返回对应实体类的List
        /// </summary>
        /// <param name="table">数据表表名</param>
        /// <param name="condition">查询条件，不带Where</param>
        /// <returns></returns>
        public List<T> Select(string table, string condition);


        /// <summary>
        /// 一般添加规则，需要传一个有数据的实体类
        /// 前提：实体类中的字段名称与数据库的完全相同
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="model">带有数据的实体类</param>
        /// <returns>成功返回True</returns>
        public bool Insert(string table, T model);


        /// <summary>
        /// 一般删除规则
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="condition">删除条件，不带Where</param>
        /// <returns></returns>
        public bool Delete(string table, string condition);


        /// <summary>
        /// 一般修改规则,需要传一个有数据的实体类,传进的实体类为修改后的值
        /// 前提：实体类中的字段名称与数据库的完全相同
        /// </summary>
        /// <param name="table">数据库表名</param>
        /// <param name="model">带有数据的实体类(作为修改后的值)</param>
        /// <param name="condition">新增条件，不带Where</param>
        /// <returns></returns>
        public bool Update(string table, T model, string condition);
    }
}
