using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SpringEntityGenerator.Model;

namespace SpringEntityGenerator.generator
{
    /// <summary>
    /// MySql生成器
    /// </summary>
    public class MySqlGenerator : AbsEntityGenerator
    {

        private Project? _project;

        private MySqlConnection? _mySqlConnection;

        public override void Generator(Project project)
        {
            this._project = project;
            // 测试MySql连接
            _mySqlConnection = new MySqlConnection(
                $"server = {project.MySql.Host}; port = {project.MySql.Port}; database = {project.MySql.Databases}; user = {project.MySql.User}; password = {project.MySql.Password}");
            _mySqlConnection.Open();
            if (_project == null)
            {
                _mySqlConnection.Close();
                throw new Exception("项目对象引用是空的。");
            }
            // 表名
            var tableName = project.Table.Prefix + project.Table.Name;
            if (project.Table.Uppercase)
            {
                tableName = tableName.ToUpper();
            }
            // 检查表是否已经存在
            var tableValid = ExecSingleCommand($"SHOW TABLES LIKE '{tableName}';");
            if (tableValid != null)
            {
                // 表已经存在，将表重新命名
                ExecSingleCommand($"RENAME TABLE `{tableName}` TO `{ToBackupName(tableName)}`;");
            }
            // ------------------------------
            // 新建数据库表
            // ------------------------------
            // 字段创建语句
            var columnSql = new StringBuilder();
            // 主键名称
            var keySql = new StringBuilder();
            // 索引创建语句
            var indexSql = new StringBuilder();
            foreach (var column in project.Table.Columns)
            {
                // 字段名格式化
                var columnName = project.Table.Prefix + column.Name;
                if (project.Table.Uppercase)
                {
                    columnName = columnName.ToUpper();
                }
                var lineSql = $" `{columnName}` {column.Type.ToString().ToUpper()}";
                // 拼接文本类型的长度
                if (column.IsTextType())
                {
                    lineSql += $"({column.Length})";
                }
                // 是否自增
                if (column.AutoIncrease)
                {
                    lineSql += " AUTO_INCREMENT";
                }
                // 是否不允许null
                if (!column.AllowNull)
                {
                    lineSql += " NOT NULL";
                }
                // 添加注释
                lineSql += $" COMMENT '{column.Comment}' ,";
                columnSql.Append(lineSql);
                // 添加主键
                if (column.Key)
                {
                    keySql.Append($"`{columnName}`,");
                }
                // 添加索引
                switch (column.IndexType)
                {
                    case IndexTypes.Unique:
                        indexSql.Append($"ALTER TABLE {tableName} ADD UNIQUE {tableName.ToLower()}_unique_{columnName} (`{columnName}`);");
                        break;
                    case IndexTypes.Index:
                        indexSql.Append($"ALTER TABLE {tableName} ADD INDEX {tableName.ToLower()}_index_{columnName} (`{columnName}`);");
                        break;
                }
            }
            if (columnSql.Length > 0 && keySql.Length == 0)
            {
                columnSql.Remove(columnSql.Length - 1, 1);
            }
            if (keySql.Length > 0)
            {
                keySql.Remove(keySql.Length - 1, 1);
                keySql.Insert(0, "PRIMARY KEY (");
                keySql.Append(")");
            }
            // 执行创建表的SQL语句
            ExecSingleCommand($"CREATE TABLE `{tableName}` ({columnSql} {keySql}) ENGINE=InnoDB DEFAULT CHARSET=utf8;");
            // 执行创建索引的语句
            ExecSingleCommand(indexSql.ToString());
            _mySqlConnection.Close();
        }

        /// <summary>
        /// 执行Sql并获取单行返回值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>执行后的返回值</returns>
        private object? ExecSingleCommand(string sql)
        {
            return new MySqlCommand(sql, _mySqlConnection).ExecuteScalar();
        }
    }
}
