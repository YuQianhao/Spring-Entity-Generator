using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Model;

namespace SpringEntityGenerator.generator
{
    /// <summary>
    /// 生成器接口
    /// </summary>
    public abstract class AbsEntityGenerator
    {

        /// <summary>
        /// 生成方法。
        /// </summary>
        public abstract void Generator(Project project);


        /// <summary>
        /// 获取一个文件的备份文件名
        /// </summary>
        /// <param name="fileName">文件原始名称</param>
        /// <returns>备份文件名</returns>
        protected string ToBackupName(string fileName)
        {
            return fileName + "_" + new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }

        /// <summary>
        /// 获取自动生成文件的声明开头
        /// </summary>
        protected string GetHeadStatementText()
        {
            return "// 这个文件是自动生成的，不要修改内容，下次自动生成的时候会覆盖这个文件。\n";
        }
    }
}
