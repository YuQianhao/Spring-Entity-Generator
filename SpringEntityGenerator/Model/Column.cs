using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpringEntityGenerator.Model
{

    /// <summary>
    /// 字段
    /// </summary>
    public class Column : INotifyPropertyChanged
    {

        /// <summary>
        /// 字段名。
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// 字段中文名。
        /// </summary>
        public string CnName { get; set; } = "";

        /// <summary>
        /// 字段类型。取值为<see cref="ColumnTypes"/>
        /// </summary>
        public ColumnTypes Type { get; set; } = ColumnTypes.Int;

        /// <summary>
        /// 是否是主键。默认为false
        /// </summary>
        public bool Key { get; set; } = false;

        /// <summary>
        /// 字段内容长度。只有在char,varchar和text类型才会生效。默认为0
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// 是否自动递增。只有在数据类型是int的情况下才生效，默认为false
        /// </summary>
        public bool AutoIncrease { get; set; } = false;

        /// <summary>
        /// 是否允许null。默认为false。
        /// </summary>
        public bool AllowNull { get; set; } = false;

        /// <summary>
        /// 索引类型。详见<see cref="IndexTypes"/>
        /// </summary>
        public IndexTypes IndexType { get; set; }

        /// <summary>
        /// 最小文本长度。只有对于char，varchar和text类型，并且<see cref="SaveParameter"/>设置为true时才会生效。
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// 最大文本长度。只有对于char，varchar和text类型，并且<see cref="SaveParameter"/>设置为true时才会生效。
        /// </summary>
        public int MaxLength { get; set; }

        /// <summary>
        /// 最小值。只有对于int和double类型，并且<see cref="SaveParameter"/>设置为true时才会生效。
        /// </summary>
        public double MinValue { get; set; }

        /// <summary>
        /// 最大值。只有对于int和double类型，并且<see cref="SaveParameter"/>设置为true时才会生效。
        /// </summary>
        public double MaxValue { get; set; }

        /// <summary>
        /// 是否用于查询条件。默认为false。
        /// </summary>
        public bool Select { get; set; } = false;

        /// <summary>
        /// 用于查询条件时，是否使用“相等”匹配。
        /// </summary>
        public bool SelectEqual { get; set; } = true;

        /// <summary>
        /// 用于查询条件时，是否使用“区间”匹配。
        /// </summary>
        public bool SelectRange { get; set; } = false;

        /// <summary>
        /// 是否添加到save方法中。
        /// </summary>
        public bool SaveParameter { get; set; } = true;

        /// <summary>
        /// 是否允许单独修改字段
        /// </summary>
        public bool AllowSetField { get; set; } = true;


        /// <summary>
        /// 字段注释。可空，默认为字段中文名
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// 获取Java对应的类型
        /// </summary>
        public string ToJavaType()
        {
            if (IsTextType())
            {
                return "String";
            }
            else
                return Type switch
                {
                    ColumnTypes.Int => "Integer",
                    ColumnTypes.Double => "Double",
                    ColumnTypes.Bool => "Boolean",
                    ColumnTypes.DateTime => "Date",
                    _ => throw new Exception("非法的数据类型" + Type)
                };
        }

        /// <summary>
        /// 是否是文本类型
        /// </summary>
        public bool IsTextType()
        {
            return Type is ColumnTypes.Text or ColumnTypes.Varchar or ColumnTypes.Char;
        }

        /// <summary>
        /// 是否是数字类型
        /// </summary>
        public bool IsNumberType()
        {
            return Type is ColumnTypes.Int or ColumnTypes.Double;
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
