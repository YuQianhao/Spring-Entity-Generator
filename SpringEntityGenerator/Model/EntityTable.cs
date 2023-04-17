using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringEntityGenerator.Model
{
    /// <summary>
    /// 数据库表
    /// </summary>
    public class EntityTable:INotifyPropertyChanged
    {

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 中文名
        /// </summary>
        public string CnName { get; set; } = "";

        /// <summary>
        /// 说明，默认使用中文名
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// 表结构中通用的前缀，这个前缀会增加到表名和字段名中。可空
        /// </summary>
        public string Prefix { get; set; } = "";

        /// <summary>
        /// 表名和字段名是否大写
        /// </summary>
        public bool Uppercase { get; set; } = true;

        private List<Column> _columns = new(0);

        /// <summary>
        /// 字段列表
        /// </summary>
        public List<Column> Columns
        {
            get=> _columns;
            set { _columns = value;OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
