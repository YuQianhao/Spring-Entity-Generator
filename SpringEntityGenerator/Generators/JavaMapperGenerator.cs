using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Models;

namespace SpringEntityGenerator.Generators
{
    public class JavaMapperGenerator :AbsEntityGenerator
    {
        public override void Generator(Project project)
        {
            var filePath = project.Path + "\\src\\main\\java\\" + project.PackageName.Replace(".", "\\") + "\\mapper\\";
            var className = $"{project.Table.Name.First().ToString().ToUpper() + project.Table.Name[1..]}";
            var mapperName = $"{className}Mapper.java";
            // 检查是否需要备份
            if (File.Exists(filePath + mapperName))
            {
                if (project.AutoBackup)
                {
                    File.Move(filePath + mapperName, filePath + $"{ToBackupName(mapperName)}");
                }
                File.Delete(filePath + mapperName);
            }
            // 检查目录是否存在
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            // 创建Mapper文件
            // 新建Java文件
            var stream = new StreamWriter(File.Create(filePath + mapperName));
            stream.Write(GetHeadStatementText());
            stream.Write("""
                package ####PACKAGE_NAME####.mapper;

                import com.baomidou.mybatisplus.core.mapper.BaseMapper;
                import ####PACKAGE_NAME####.entity.####CLASS_NAME####;

                public interface ####CLASS_NAME####Mapper extends BaseMapper<####CLASS_NAME####>{}
                """
                .Replace("####PACKAGE_NAME####", project.PackageName)
                .Replace("####CLASS_NAME####", className)
            );
            stream.Close();
        }
    }
}
