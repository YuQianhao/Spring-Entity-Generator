using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Model;

namespace SpringEntityGenerator.generator
{
    public class JavaControllerGenerator : AbsEntityGenerator
    {
        public override void Generator(Project project)
        {
            var filePath = project.Path + "\\src\\main\\java\\" + project.PackageName.Replace(".", "\\") + "\\controller\\";
            var className = $"{project.Table.Name.First().ToString().ToUpper() + project.Table.Name[1..]}";
            var mapperName = $"{className}ControllerTemplate.java";
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
                package ####PACKAGE_NAME####.controller;

                import java.util.Date;
                import ####PACKAGE_NAME####.entity.####CLASS_NAME####;
                import org.springframework.web.bind.annotation.PostMapping;
                import org.springframework.web.bind.annotation.RequestBody;
                import ####PACKAGE_NAME####.service.####CLASS_NAME####ServiceTemplate;
                import com.baomidou.mybatisplus.core.metadata.IPage;
                import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
                import com.baomidou.mybatisplus.extension.plugins.pagination.Page;

                public class ####CLASS_NAME####ControllerTemplate extends BaseController {
                """.Replace("####CLASS_NAME####", className).Replace("####PACKAGE_NAME####", project.PackageName)
            );
            var classServiceFieldName = className.First().ToString().ToLower() + className[1..] + "ServiceTemplate";
            stream.Write("""
                private final ####CLASS_NAME####ServiceTemplate ####CLASS_LOWER_NAME####;

                public ####CLASS_NAME####ControllerTemplate(####CLASS_NAME####ServiceTemplate ####CLASS_LOWER_NAME####) {
                    this.####CLASS_LOWER_NAME#### = ####CLASS_LOWER_NAME####;
                }
                """.Replace("####CLASS_LOWER_NAME####", classServiceFieldName).Replace("####CLASS_NAME####", className));
            // Save类的字段
            var saveClassFields = new StringBuilder();
            // save方法的字段检查
            var saveMethodFieldCheck = new StringBuilder();
            // save方法调用服务的create方法参数列表
            var saveCallCreateBody = new StringBuilder();
            // Select类的字段
            var selectClassFields = new StringBuilder();
            // select字段拼接查询参数
            var selectBody=new StringBuilder();
            foreach (var field in project.Table.Columns)
            {
                if (field.Select)
                {
                    if (field.SelectRange)
                    {
                        selectClassFields.Append("public " + field.ToJavaType() + " " + field.Name + "Start;\n");
                        selectClassFields.Append("public " + field.ToJavaType() + " " + field.Name + "End;\n");
                    }
                    else
                    {
                        selectClassFields.Append("public " + field.ToJavaType() + " " + field.Name + ";\n");
                    }
                    if (field.SelectEqual)
                    {
                        if (field.IsTextType())
                        {
                            selectBody.Append($"if(select.{field.Name}!=null && !select.{field.Name}.isEmpty()){{\n");
                            selectBody.Append($"query.eq({className}::get{field.Name.First().ToString().ToUpper() + field.Name[1..]},select.{field.Name});\n}}\n");
                        }
                        else
                        {
                            selectBody.Append($"if(select.{field.Name}!=null){{\n");
                            selectBody.Append($"query.eq({className}::get{field.Name.First().ToString().ToUpper() + field.Name[1..]},select.{field.Name});\n}}\n");
                        }
                    }else if (field.SelectRange)
                    {
                        if (field.IsTextType())
                        {
                            selectBody.Append($"if(select.{field.Name}Start!=null && !select.{field.Name}Start.isEmpty()){{\n");
                            selectBody.Append($"query.ge({className}::get{field.Name.First().ToString().ToUpper() + field.Name[1..]},select.{field.Name}Start);\n}}\n");
                            selectBody.Append($"if(select.{field.Name}End!=null && !select.{field.Name}End.isEmpty()){{\n");
                            selectBody.Append($"query.lt({className}::get{field.Name.First().ToString().ToUpper() + field.Name[1..]},select.{field.Name}End);\n}}\n");
                        }
                        else
                        {
                            selectBody.Append($"if(select.{field.Name}Start!=null){{\n");
                            selectBody.Append($"query.ge({className}::get{field.Name.First().ToString().ToUpper() + field.Name[1..]},select.{field.Name}Start);\n}}\n");
                            selectBody.Append($"if(select.{field.Name}End!=null){{\n");
                            selectBody.Append($"query.lt({className}::get{field.Name.First().ToString().ToUpper() + field.Name[1..]},select.{field.Name}End);\n}}\n");
                        }
                        
                    }
                }
                if (field.Key || field.SaveParameter)
                {
                    saveClassFields.Append("public " + field.ToJavaType() + " " + field.Name + ";\n");
                }
                if (field is { Key: false, SaveParameter: true })
                {
                    if (field.IsTextType())
                    {
                        // 是否为空
                        if (field.AllowNull) continue;
                        saveMethodFieldCheck.Append(
                            $"if(save.{field.Name}==null || save.{field.Name}.isEmpty()){{throw new RuntimeException(\"Field '{field.Name}' cannot be empty, but it is empty here.\");}}\n");
                        if (field.MinLength == field.MaxLength)
                        {
                            saveMethodFieldCheck.Append(
                                $"else if(save.{field.Name}.length()!={field.MaxLength}){{throw new RuntimeException(\"Field '{field.Name}' requires that the length must be {field.MinLength}.\");}}\n");
                        }
                        else
                        {
                            saveMethodFieldCheck.Append(
                                $"else if(save.{field.Name}.length()>{field.MaxLength} || save.{field.Name}.length()<{field.MinLength}){{throw new RuntimeException(\"The length of field '{field.Name}' does not meet the rules, and it cannot be less than {field.MinLength} and cannot be greater than {field.MaxLength}.\");}}\n");
                        }
                    }
                    else if (field.IsNumberType())
                    {
                        // 是否为空
                        if (field.AllowNull) continue;
                        saveMethodFieldCheck.Append(
                            $"if(save.{field.Name}==null){{throw new RuntimeException(\"Field '{field.Name}' cannot be empty, but it is empty here.\");}}\n");
                        if (field.MinValue.Equals(field.MaxValue))
                        {
                            saveMethodFieldCheck.Append(
                                $"else if(save.{field.Name}!={field.MaxValue}){{throw new RuntimeException(\"Field '{field.Name}' requires that the value must be {field.MinValue}.\");}}\n");
                        }
                        else
                        {
                            saveMethodFieldCheck.Append(
                                $"else if(save.{field.Name}>{field.MaxValue} || save.{field.Name}<{field.MinValue}){{throw new RuntimeException(\"The value of field {field.Name} does not meet the rules, and it cannot be less than {field.MinValue} and cannot be greater than {field.MaxLength}.\");}}\n");
                        }
                    }
                    else
                    {
                        // 是否为空
                        if (field.AllowNull) continue;
                        saveMethodFieldCheck.Append(
                            $"if(save.{field.Name}==null){{throw new RuntimeException(\"Field '{field.Name}' cannot be empty, but it is empty here.\");}}\n");
                    }
                }
                saveCallCreateBody.Append("save.");
                if (field.Key || field.SaveParameter)
                {
                    saveCallCreateBody.Append(field.Name);
                }
                else
                {
                    saveCallCreateBody.Append("null");
                }
                saveCallCreateBody.Append(",");
            }
            if (saveCallCreateBody.Length > 0)
            {
                saveCallCreateBody.Remove(saveCallCreateBody.Length - 1, 1);
            }
            stream.Write("""
                            private static class OnlyId{
                    public Integer id;
                }
                """);
            // ============================================
            // getEntity方法
            // ============================================
            stream.Write("""
                            /* 重写这个方法可以处理查询接口查询到的对象，这个返回结果将会直接返回给发起请求的客户端。**/
                protected Object onHandleGetAfter(####CLASS_NAME#### test) {
                    return test;
                }

                @PostMapping("getEntity")
                public Object getEntity(@RequestBody OnlyId onlyId) {
                    if (onlyId.id == null) {
                        throw new RuntimeException("The object 'id' to be queried cannot be empty.");
                    }
                    return this.onHandleGetAfter(####SERVICE_FIELD_NAME####.getById(onlyId.id));
                }
                """.Replace("####CLASS_NAME####",className).Replace("####SERVICE_FIELD_NAME####", classServiceFieldName));
            // ============================================
            // remove方法
            // ============================================
            stream.Write("""
                            /* 重写这个方法可以处理remove接口在查询到要删除的对象，这个返回结果将会被传入数据库的删除方法。**/
                protected ####CLASS_NAME#### onHandleRemoveBefore(####CLASS_NAME#### object) {
                    return object;
                }

                /* 重写这个方法可以处理remove接口在删除之后的处理业务，这个返回结果将会直接返回给发起请求的客户端。**/
                protected Object onHandleRemoveAfter() {
                    return new Object();
                }
                """.Replace("####CLASS_NAME####",className));
            stream.Write("""
                            @PostMapping("remove")
                public Object remove(@RequestBody OnlyId onlyId){
                    if(onlyId.id==null){
                        throw new RuntimeException("The object 'id' to be deleted cannot be empty.");
                    }
                    var object=####SERVICE_NAME####.getById(onlyId.id);
                    ####SERVICE_NAME####.removeById(this.onHandleRemoveBefore(object).getId());
                    return this.onHandleRemoveAfter();
                }
                """.Replace("####SERVICE_NAME####",classServiceFieldName));
            // =============================================
            // select方法
            // =============================================
            if (project.Table.Columns.Find(item => item.Select) != null)
            {
                selectClassFields.Append("public Integer page;\npublic Integer pageSize;\n");
                stream.Write($"\nprivate static class Select {{\n{selectClassFields} \n}}\n");
                stream.Write("""
                    /* 重写这个方法可以处理select接口在创建完查询条件之后的回调，这个返回结果将会被传入分页查询的方法。**/
                    protected LambdaQueryWrapper<####CLASS_NAME####> onHandleSelectBefore(LambdaQueryWrapper<####CLASS_NAME####> queryWrapper){
                     return queryWrapper;
                    }
                    /* 重写这个方法可以处理select接口分页查询之后的结果，这个返回结果将会直接返回给发起请求的客户端。**/
                    protected Object onHandleSelectAfter(Page<####CLASS_NAME####> result){
                     return result;
                    }
                    """.Replace("####CLASS_NAME####", className));

                stream.Write("\n@PostMapping(\"select\")\n    public Object select(@RequestBody Select select){\n");
                stream.Write("""
                                if(select.page==null || select.page<1){
                        throw new RuntimeException("The 'page' field in a paginated query cannot be empty and cannot be less than 0.");
                    }
                    if(select.pageSize==null || select.pageSize<1 || select.pageSize>20){
                        throw new RuntimeException("The 'pageSize' field in a paginated query cannot be empty, less than 0 and greater than 20.");
                    }
                    var query=new LambdaQueryWrapper<####CLASS_NAME####>();
                    """.Replace("####CLASS_NAME####",className));
                stream.Write(selectBody);
                stream.Write("return onHandleSelectAfter(####SERVICE_FIELD_NAME####.page(new Page<>(select.page,select.pageSize),this.onHandleSelectBefore(query)));\n}\n".Replace("####SERVICE_FIELD_NAME####", classServiceFieldName));
            }
            // =============================================
            // save方法
            // =============================================
            stream.Write("\nprivate static class Save {\n" + saveClassFields + "\n}\n");
            stream.Write($"/* 重写这个方法可以处理save接口在调用数据库save方法之前的回调，这个返回结果将会被传入数据库save方法。**/\nprotected {className} onHandleSaveBefore({className} entity){{\n return entity;\n}}\n");
            stream.Write($"/* 重写这个方法可以处理save接口在调用数据库save方法之后的回调，这个返回结果将会直接返回给发起请求的客户端。**/\nprotected Object onHandleSaveAfter({className} entity){{\n return entity;\n}}\n");
            stream.Write("\n@PostMapping(\"save\")\n public Object save(@RequestBody Save save){\n");
            stream.Write(saveMethodFieldCheck);
            stream.Write($"var object={classServiceFieldName}.create({saveCallCreateBody});\n");
            stream.Write($"return this.onHandleSaveAfter(this.{classServiceFieldName}.saveEntity(this.onHandleSaveBefore(object)));\n");
            stream.Write("\n}");
            stream.Write("\n}");
            stream.Close();
        }
    }
}
