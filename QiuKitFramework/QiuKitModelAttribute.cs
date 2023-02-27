using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiuKitFramework
{
    public class QiuKitModelAttribute : Attribute
    {
        /// <summary>
        /// 是否为主键
        /// </summary>
        public bool IsPrimKey = false;

        /// <summary>
        /// 是否为标识序列
        /// </summary>
        public bool IsIdentity = false;

        /// <summary>
        /// 是否为字段
        /// </summary>
        public bool IsField = true;
    }
}
