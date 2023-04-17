using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpringEntityGenerator.Model
{
    /// <summary>
    /// 支持的索引类型。
    /// </summary>
    public enum IndexTypes
    {

        /// <summary>
        /// 无索引
        /// </summary>
        None,
        /// <summary>
        /// 默认索引类型
        /// </summary>
        Index,
        /// <summary>
        /// 唯一索引
        /// </summary>
        Unique

    }
}
