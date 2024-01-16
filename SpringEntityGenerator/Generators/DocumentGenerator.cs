using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.XSSF.UserModel;
using SpringEntityGenerator.Models;

namespace SpringEntityGenerator.Generators
{
    public class DocumentGenerator : AbsEntityGenerator
    {
        public override void Generator(Project project)
        {
            // 获取文档名称
            var documentName = project.Table.CnName + $"({project.Table.Name}).xlsx";
            // 文档存储路径
            var documentPath = project.DocumentPath + "//" + documentName;
            // 实际否需要备份
            if (project.AutoBackup && File.Exists(documentPath))
            {
                File.Move(documentPath, project.DocumentPath + "//" + ToBackupName(documentName));
            }

            var workbook = new XSSFWorkbook();

            // 生成完整版的表格
            var sheet = workbook.CreateSheet("完整");
            {
                var tableNameRow = sheet.CreateRow(0);
                tableNameRow.CreateCell(0).SetCellValue("表名：");
                tableNameRow.CreateCell(1).SetCellValue(project.Table.Name);
                var tableCnNameRow = sheet.CreateRow(1);
                tableCnNameRow.CreateCell(0).SetCellValue("CnName：");
                tableCnNameRow.CreateCell(1).SetCellValue(project.Table.Name);
                var describeRow = sheet.CreateRow(2);
                describeRow.CreateCell(0).SetCellValue("describe：");
                describeRow.CreateCell(1).SetCellValue(project.Table.Comment);
                var dateRow = sheet.CreateRow(3);
                dateRow.CreateCell(0).SetCellValue("创建日期：");
                dateRow.CreateCell(1).SetCellValue(DateTime.UtcNow.ToString("u"));
                // 写入列头
                var header = sheet.CreateRow(4);
                header.CreateCell(0).SetCellValue("序号");
                header.CreateCell(1).SetCellValue("字段");
                header.CreateCell(2).SetCellValue("cnName");
                header.CreateCell(3).SetCellValue("类型");
                header.CreateCell(4).SetCellValue("长度");
                header.CreateCell(5).SetCellValue("主键");
                header.CreateCell(6).SetCellValue("可空");
                header.CreateCell(7).SetCellValue("索引");
                header.CreateCell(8).SetCellValue("最小长度");
                header.CreateCell(9).SetCellValue("最大长度");
                header.CreateCell(10).SetCellValue("最小值");
                header.CreateCell(11).SetCellValue("最大值");
                header.CreateCell(12).SetCellValue("select接口");
                header.CreateCell(13).SetCellValue("select相等匹配");
                header.CreateCell(14).SetCellValue("select范围匹配");
                header.CreateCell(14).SetCellValue("select文本模糊匹配");
                header.CreateCell(15).SetCellValue("save接口支持");
                header.CreateCell(16).SetCellValue("说明");
                var index = 0;
                foreach (var tableColumn in project.Table.Columns)
                {
                    index++;
                    var columnRow = sheet.CreateRow(4 + index + 1);
                    columnRow.CreateCell(0).SetCellValue(index);
                    columnRow.CreateCell(1).SetCellValue(tableColumn.Name);
                    columnRow.CreateCell(2).SetCellValue(tableColumn.CnName);
                    columnRow.CreateCell(3).SetCellValue(tableColumn.Type.ToString());
                    columnRow.CreateCell(4).SetCellValue(tableColumn.IsTextType() ? tableColumn.Length.ToString() : "无效");
                    columnRow.CreateCell(5).SetCellValue(tableColumn.Key ? "√" : "");
                    columnRow.CreateCell(6).SetCellValue(tableColumn.AllowNull ? "√" : "");
                    columnRow.CreateCell(7).SetCellValue(tableColumn.IndexType != IndexTypes.None ? tableColumn.IndexType.ToString() : "");
                    columnRow.CreateCell(8).SetCellValue(tableColumn.IsTextType() ? tableColumn.MinLength.ToString() : "无效");
                    columnRow.CreateCell(9).SetCellValue(tableColumn.IsTextType() ? tableColumn.MaxLength.ToString() : "无效");
                    columnRow.CreateCell(10).SetCellValue(tableColumn.IsNumberType() ? tableColumn.MinValue.ToString(CultureInfo.InvariantCulture) : "无效");
                    columnRow.CreateCell(11).SetCellValue(tableColumn.IsNumberType() ? tableColumn.MaxValue.ToString(CultureInfo.InvariantCulture) : "无效");
                    columnRow.CreateCell(12).SetCellValue(tableColumn.Select ? "√" : "");
                    columnRow.CreateCell(13).SetCellValue(tableColumn.SelectEqual ? "√" : "");
                    columnRow.CreateCell(14).SetCellValue(tableColumn.SelectRange ? "√" : "");
                    columnRow.CreateCell(14).SetCellValue(tableColumn.SelectTextLike ? "√" : "");
                    columnRow.CreateCell(15).SetCellValue(tableColumn.SaveParameter ? "√" : "");
                    columnRow.CreateCell(16).SetCellValue(string.IsNullOrEmpty(tableColumn.Comment) ? tableColumn.CnName : tableColumn.Comment);
                }
            }

            // 生成简易版的表格
            sheet = workbook.CreateSheet("建议");
            {
                var tableNameRow = sheet.CreateRow(0);
                tableNameRow.CreateCell(0).SetCellValue("表名：");
                tableNameRow.CreateCell(1).SetCellValue(project.Table.Name);
                var tableCnNameRow = sheet.CreateRow(1);
                tableCnNameRow.CreateCell(0).SetCellValue("CnName：");
                tableCnNameRow.CreateCell(1).SetCellValue(project.Table.Name);
                var describeRow = sheet.CreateRow(2);
                describeRow.CreateCell(0).SetCellValue("describe：");
                describeRow.CreateCell(1).SetCellValue(project.Table.Comment);
                var dateRow = sheet.CreateRow(3);
                dateRow.CreateCell(0).SetCellValue("创建日期：");
                dateRow.CreateCell(1).SetCellValue(DateTime.UtcNow.ToString("u"));
                // 写入列头
                var header = sheet.CreateRow(4);
                header.CreateCell(0).SetCellValue("序号");
                header.CreateCell(1).SetCellValue("字段");
                header.CreateCell(2).SetCellValue("类型");
                header.CreateCell(3).SetCellValue("可空");
                header.CreateCell(4).SetCellValue("最小长度");
                header.CreateCell(5).SetCellValue("最大长度");
                header.CreateCell(6).SetCellValue("最小值");
                header.CreateCell(7).SetCellValue("最大值");
                header.CreateCell(8).SetCellValue("说明");
                var index = 0;
                foreach (var tableColumn in project.Table.Columns)
                {
                    index++;
                    var columnRow = sheet.CreateRow(4 + index + 1);
                    columnRow.CreateCell(0).SetCellValue(index);
                    columnRow.CreateCell(1).SetCellValue(tableColumn.Name);
                    columnRow.CreateCell(2).SetCellValue(tableColumn.Type.ToString());
                    columnRow.CreateCell(3).SetCellValue(tableColumn.AllowNull ? "√" : "");
                    columnRow.CreateCell(4).SetCellValue(tableColumn.IsTextType() ? tableColumn.MinLength.ToString() : "-");
                    columnRow.CreateCell(5).SetCellValue(tableColumn.IsTextType() ? tableColumn.MaxLength.ToString() : "-");
                    columnRow.CreateCell(6).SetCellValue(tableColumn.IsNumberType() ? tableColumn.MinValue.ToString(CultureInfo.InvariantCulture) : "-");
                    columnRow.CreateCell(7).SetCellValue(tableColumn.IsNumberType() ? tableColumn.MaxValue.ToString(CultureInfo.InvariantCulture) : "-");
                    columnRow.CreateCell(8).SetCellValue(string.IsNullOrEmpty(tableColumn.Comment) ? tableColumn.CnName : tableColumn.Comment);
                }
            }

            workbook.Write(new FileStream(documentPath, FileMode.Create));
            workbook.Close();
        }
    }
}
