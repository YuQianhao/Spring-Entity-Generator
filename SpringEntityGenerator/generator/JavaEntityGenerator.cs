﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Model;

namespace SpringEntityGenerator.generator
{
    public class JavaEntityGenerator : AbsEntityGenerator
    {
        public override void Generator(Project project)
        {
            // 表名
            var tableName = project.Table.Prefix + project.Table.Name;
            if (project.Table.Uppercase)
            {
                tableName = tableName.ToUpper();
            }
            var filePath = project.Path + "\\src\\main\\java\\" + project.PackageName.Replace(".", "\\") + "\\entity\\";
            var className = $"{project.Table.Name.First().ToString().ToUpper() + project.Table.Name[1..]}";
            var entityName = $"{className}.java";
            // 检查是否需要备份
            if (File.Exists(filePath + entityName))
            {
                if (project.AutoBackup)
                {
                    File.Move(filePath + entityName, filePath + $"{ToBackupName(entityName)}");
                }
                File.Delete(filePath + entityName);
            }
            // 检查目录是否存在
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            // 新建Java文件
            var stream = new StreamWriter(File.Create(filePath + entityName));
            stream.Write(GetHeadStatementText());
            stream.Write("""
                package ####PACKAGE_NAME####.entity;

                import com.baomidou.mybatisplus.annotation.IdType;
                import com.baomidou.mybatisplus.annotation.TableField;
                import com.baomidou.mybatisplus.annotation.TableId;
                import com.baomidou.mybatisplus.annotation.TableName;
                import java.lang.reflect.Field;
                import java.util.Arrays;
                import java.util.Date;
                import java.util.List;

                /**
                  * ####CN_NAME####<br>####TABLE_COMMENT####
                  */
                @TableName("####TABLE_NAME####")
                public class ####CLASS_NAME####{
                """.Replace("####PACKAGE_NAME####", project.PackageName).
                Replace("####CN_NAME####", project.Table.CnName).
                Replace("####TABLE_NAME####", tableName).
                Replace("####CLASS_NAME####", className).
                Replace("####TABLE_COMMENT####", project.Table.Comment));
            foreach (var field in project.Table.Columns)
            {
                // 字段在数据库中的名称
                var fieldFormatName = project.Table.Prefix + field.Name;
                if (project.Table.Uppercase)
                {
                    fieldFormatName = fieldFormatName.ToUpper();
                    // 写入注释
                    stream.Write("\n/**" + field.CnName + "<br>" + field.Comment + "*/\n");
                    // 写入注解
                    if (field.Key)
                    {
                        stream.Write(field.AutoIncrease ? $"@TableId(value = \"{fieldFormatName}\",type = IdType.AUTO)\n" : $"@TableId(value = \"{fieldFormatName}\")\n");
                    }
                    else
                    {
                        stream.Write($"@TableField(\"{fieldFormatName}\")\n");
                    }
                    // 写入字段
                    if (field.Key)
                    {
                        stream.Write("public " + field.ToJavaType() + " " + field.Name + ";\n");
                    }
                    else
                    {
                        stream.Write("private " + field.ToJavaType() + " " + field.Name + ";\n");
                    }
                    // 写入get set
                    stream.Write("public void set####FIELD_NOUN_NAME####(####TYPE#### value){ this.####FIELD_NAME#### = value; }\n"
                        .Replace("####TYPE####", field.ToJavaType())
                        .Replace("####FIELD_NOUN_NAME####", field.Name.First().ToString().ToUpper() + field.Name[1..])
                        .Replace("####FIELD_NAME####", field.Name)
                    );
                    stream.Write("public ####TYPE#### get####FIELD_NOUN_NAME####(){ return this.####FIELD_NAME####; }\n"
                        .Replace("####TYPE####", field.ToJavaType())
                        .Replace("####FIELD_NOUN_NAME####", field.Name.First().ToString().ToUpper() + field.Name[1..])
                        .Replace("####FIELD_NAME####", field.Name)
                    );
                }
            }

            // =========================================
            //             写入 create 静态方法
            // =========================================
            stream.Write("""

                            /**
                 * 从另一个对象中复制相同字段的值
                 */
                public static ####CLASS_NAME#### create(Object object) {
                    ####CLASS_NAME#### targetObject = new ####CLASS_NAME####();
                    try {
                        Class<?> targetClass = targetObject.getClass();
                        Field[] targetFields = targetClass.getDeclaredFields();
                        Class<?> paramsClass = object.getClass();
                        Field[] paramsFields = paramsClass.getDeclaredFields();
                        for (Field paramsField : paramsFields) {
                            List<Field> targetValueFields = Arrays.stream(targetFields).filter(item -> item.getName().equals(paramsField.getName())).toList();
                            for (Field targetField : targetValueFields) {
                                targetField.setAccessible(true);
                                if(targetField.getGenericType().equals(paramsField.getGenericType())){
                                    targetField.set(targetObject, paramsField.get(object));
                                }

                            }
                        }
                    } catch (Exception e) {
                        throw new RuntimeException(e.getMessage());
                    }
                    return targetObject;
                }

                """.Replace("####CLASS_NAME####", className));

            stream.Write("}");
            stream.Close();
        }
    }
}
