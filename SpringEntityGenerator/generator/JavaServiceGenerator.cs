using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Model;

namespace SpringEntityGenerator.generator
{
    public class JavaServiceGenerator : AbsEntityGenerator
    {
        public override void Generator(Project project)
        {
            var filePath = project.Path + "\\src\\main\\java\\" + project.PackageName.Replace(".", "\\") + "\\service\\";
            var className = $"{project.Table.Name.First().ToString().ToUpper() + project.Table.Name[1..]}";
            var mapperName = $"{className}ServiceTemplate.java";
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
                package ####PACKAGE_NAME####.service;

                import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
                import ####PACKAGE_NAME####.entity.####CLASS_NAME####;
                import ####PACKAGE_NAME####.mapper.####CLASS_NAME####Mapper;
                import org.jetbrains.annotations.NotNull;
                import org.springframework.stereotype.Service;
                import java.util.Date;

                @Service
                public class ####CLASS_NAME####ServiceTemplate extends ServiceImpl<####CLASS_NAME####Mapper, ####CLASS_NAME####> {
                """.Replace("####CLASS_NAME####", className).Replace("####PACKAGE_NAME####", project.PackageName)
            );
            // ===================================
            // 增加Create方法
            // ===================================
            stream.Write($"\npublic {className} create(");
            // 写入参数列表
            var createMethodParamsText = new StringBuilder();
            // 方法体的修改字段部分
            var methodBodySetText = new StringBuilder();
            // saveV1版本方法的参数列表
            var saveMultipleParamsText = new StringBuilder();
            // saveV2版本的方法体
            var saveSingleBodyText = new StringBuilder();
            foreach (var field in project.Table.Columns)
            {
                createMethodParamsText.Append($"{field.ToJavaType()} {field.Name} , ");
                methodBodySetText.Append($"object.set{field.Name.First().ToString().ToUpper() + field.Name[1..]}({field.Name});\n");
                if (field is { AllowNull: false, AutoIncrease: false })
                {
                    saveMultipleParamsText.Append($"@NotNull {field.ToJavaType()} {field.Name} , ");
                }
                else
                {
                    saveMultipleParamsText.Append($"{field.ToJavaType()} {field.Name} , ");
                }
                saveSingleBodyText.Append($"object.get{field.Name.First().ToString().ToUpper() + field.Name[1..]}(),");
            }
            if (createMethodParamsText.Length > 0)
            {
                createMethodParamsText.Remove(createMethodParamsText.Length - 2, 2);
            }
            if (saveMultipleParamsText.Length > 0)
            {
                saveMultipleParamsText.Remove(saveMultipleParamsText.Length - 2, 2);
            }
            if (saveSingleBodyText.Length > 0)
            {
                saveSingleBodyText.Remove(saveSingleBodyText.Length - 1, 1);
            }
            stream.Write(createMethodParamsText.ToString());
            stream.Write("){\n");
            // 创建方法体
            stream.Write($"{className} object = new {className}();\n");
            stream.Write(methodBodySetText);
            stream.Write("return object;\n}");
            // ===================================
            // 增加Save方法
            // ===================================
            stream.Write($"\npublic {className} save({saveMultipleParamsText})" + "{\n");
            stream.Write($$"""
                {className} object = getById({project.Table.Columns.Find(item => item.Key)?.Name});
                if(id!=null && object==null){
                    throw new RuntimeException("要修改的数据不存在。");
                }
                boolean needInsert=false;
                if(object == null) {"{"} 
                object=new {className}();
                needInsert=true;
                {"}"}
                {methodBodySetText}
                if(needInsert){"{"}
                getBaseMapper().insert(object);
                {"}else{"}
                getBaseMapper().updateById(object);
                {"}"}
                return object;
                {"}"}
                """);
            // ===================================
            // 增加Save V2方法
            // ===================================
            stream.Write($"\npublic {className} saveEntity(@NotNull {className} object)" + "{\n");
            stream.Write($"return this.save({saveSingleBodyText});\n}}\n");
            // ===================================
            // 增加get方法
            // ===================================
            stream.Write($"public {className} getEntityById(@NotNull " + project.Table.Columns.Find(item => item.Key)?.ToJavaType() + " selectKey){\n");
            stream.Write("return getById(selectKey);\n}\n");
            // ===================================
            // 增加remove方法
            // ===================================
            stream.Write($"public void removeById(@NotNull " + project.Table.Columns.Find(item => item.Key)?.ToJavaType() + " removeKey){\n");
            stream.Write("getBaseMapper().deleteById(removeKey);\n}\n");
            // ===================================
            // 增加removeEntity方法
            // ===================================
            var keyField = project.Table.Columns.Find(item => item.Key);
            stream.Write($"public void removeEntity(@NotNull {className} entity){{");
            stream.Write($"getBaseMapper().deleteById(entity.get{keyField?.Name.First().ToString().ToUpper()+ keyField?.Name[1..]}());\n}}");
            stream.Write("}");
            stream.Close();
        }
    }
}
