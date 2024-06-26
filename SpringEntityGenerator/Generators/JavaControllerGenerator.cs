﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Models;

namespace SpringEntityGenerator.Generators
{
    public class JavaControllerGenerator : AbsEntityGenerator
    {
        public override void Generator(Project project)
        {
            // controller文件的写出路径
            var filePath = project.Path + "\\src\\main\\java\\" + project.PackageName.Replace(".", "\\") +
                           "\\controller\\template\\";
            // 类名
            var className = $"{project.Table.Name.First().ToString().ToUpper() + project.Table.Name[1..]}";
            // 使用的mapper名称
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
                package ##PACKAGE_NAME##.controller.template;

                import java.util.*;
                import java.util.Date;
                import org.springframework.transaction.annotation.Transactional;
                import ##PACKAGE_NAME##.entity.##CLASS_NAME##;
                import org.springframework.web.bind.annotation.PostMapping;
                import org.springframework.web.bind.annotation.RequestBody;
                import ##PACKAGE_NAME##.service.template.##CLASS_NAME##ServiceTemplate;
                import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
                import com.baomidou.mybatisplus.extension.plugins.pagination.Page;

                public class ##CLASS_NAME##ControllerTemplate extends ##CLASS_NAME##ServiceTemplate {


                """.Replace("##CLASS_NAME##", className).Replace("##PACKAGE_NAME##", project.PackageName)
            );

            // Save类的字段
            var saveClassFields = new StringBuilder();

            // save方法的字段检查
            var saveMethodFieldCheck = new StringBuilder();

            // save方法调用服务的create方法参数列表
            var saveCallCreateBody = new StringBuilder();

            // Select类的字段
            var selectClassFields = new StringBuilder();

            // 在select方法中，每个查询条件的Handle方法
            var selectParamsHandleMethod = new StringBuilder();

            // select字段拼接查询参数
            var selectBody = new StringBuilder();

            foreach (var field in project.Table.Columns)
            {
                // 字段首字母大写的名称
                var fieldBigName = field.Name.First().ToString().ToUpper() + field.Name[1..];

                saveCallCreateBody.Append("");
                if (field.Key || field.SaveParameter)
                {
                    saveCallCreateBody.Append("save." + field.Name);
                }
                else
                {
                    saveCallCreateBody.Append("null");
                }

                saveCallCreateBody.Append(",");
                if (field.Select)
                {
                    // 查询条件Handle方法名称
                    var selectParamsHandleName =
                        "onHandleSelectParams##FIELD_BIG_NAME##".Replace("##FIELD_BIG_NAME##", fieldBigName);

                    // 先为select方法的查询条件生成Handle方法体
                    if (field.SelectRange)
                    {
                        // 字段范围查询
                        selectParamsHandleMethod.Append("""

                            // 重写这个方法，可以修改查询参数"##FIELD_NAME##"的查询步骤
                            protected void ##METHOD_NAME##(##CLASS_NAME##_Select select,##FIELD_JAVA_TYPE## ##FIELD_NAME##Start,##FIELD_JAVA_TYPE## ##FIELD_NAME##End,LambdaQueryWrapper<##CLASS_NAME##> query){
                                ##SELECT_LOGIC##
                            }

                            """.Replace("##FIELD_BIG_NAME##", fieldBigName)
                            .Replace("##CLASS_NAME##", className)
                            .Replace("##FIELD_JAVA_TYPE##", field.ToJavaType())
                            .Replace("##FIELD_NAME##", field.Name)
                            .Replace("##METHOD_NAME##", selectParamsHandleName)
                        );
                    }
                    else
                    {
                        // 字段非范围查询
                        selectParamsHandleMethod.Append("""

                            // 重写这个方法，可以修改查询参数"##FIELD_NAME##"的查询步骤
                            protected void ##METHOD_NAME##(##CLASS_NAME##_Select select,##FIELD_JAVA_TYPE## ##FIELD_NAME##,LambdaQueryWrapper<##CLASS_NAME##> query){
                                ##SELECT_LOGIC##
                            }

                            """.Replace("##FIELD_BIG_NAME##", fieldBigName)
                            .Replace("##CLASS_NAME##", className)
                            .Replace("##FIELD_JAVA_TYPE##", field.ToJavaType())
                            .Replace("##FIELD_NAME##", field.Name)
                            .Replace("##METHOD_NAME##", selectParamsHandleName)
                        );
                    }

                    // 参与查询的字段
                    string selectParamsName;

                    // 具体的查询业务
                    var selectLogic = new StringBuilder();

                    // 生成查询字段
                    if (field.SelectRange)
                    {
                        selectClassFields.Append("public " + field.ToJavaType() + " " + field.Name + "Start;\n");
                        selectClassFields.Append("public " + field.ToJavaType() + " " + field.Name + "End;\n");
                        selectParamsName = $"select.{field.Name}Start,select.{field.Name}End";
                    }
                    else
                    {
                        selectClassFields.Append("public " + field.ToJavaType() + " " + field.Name + ";\n");
                        selectParamsName = $"select.{field.Name}";
                    }

                    // 生成查询业务
                    if (field.SelectEqual || field.SelectTextLike)
                    {
                        if (field.IsTextType())
                        {
                            selectLogic.Append($"if({field.Name}!=null && !{field.Name}.isEmpty()){{\n");
                            if (field.SelectTextLike)
                            {
                                selectLogic.Append($"query.like({className}::get{fieldBigName},{field.Name});\n}}\n");
                            }
                            else
                            {
                                selectLogic.Append($"query.eq({className}::get{fieldBigName},{field.Name});\n}}\n");
                            }
                        }
                        else
                        {
                            selectLogic.Append($"if({field.Name}!=null){{\n");
                            selectLogic.Append($"query.eq({className}::get{fieldBigName},{field.Name});\n}}\n");
                        }
                    }
                    else if (field.SelectRange)
                    {
                        if (field.IsTextType())
                        {
                            selectLogic.Append($"if({field.Name}Start!=null && !{field.Name}Start.isEmpty()){{\n");
                            selectLogic.Append($"query.ge({className}::get{fieldBigName},{field.Name}Start);\n}}\n");
                            selectLogic.Append($"if({field.Name}End!=null && !{field.Name}End.isEmpty()){{\n");
                            selectLogic.Append($"query.lt({className}::get{fieldBigName},{field.Name}End);\n}}\n");
                        }
                        else
                        {
                            selectLogic.Append($"if({field.Name}Start!=null){{\n");
                            selectLogic.Append($"query.ge({className}::get{fieldBigName},{field.Name}Start);\n}}\n");
                            selectLogic.Append($"if({field.Name}End!=null){{\n");
                            selectLogic.Append($"query.lt({className}::get{fieldBigName},{field.Name}End);\n}}\n");
                        }
                    }

                    // 将查询业务替换到查询参数Handle方法中
                    selectParamsHandleMethod.Replace("##SELECT_LOGIC##", selectLogic.ToString());

                    // 查询方法体中调用这个这个Handle方法
                    selectBody.Append($"\n{selectParamsHandleName}(select,{selectParamsName},query);\n");
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
                            $"if({field.Name}==null || {field.Name}.isEmpty()){{throw new RuntimeException(\"字段 '{field.Name}' 不能是空的，但是在这里的参数却是空的。\");}}\n");
                        if (field.MinLength == field.MaxLength)
                        {
                            // 字段大小长度相等，并且不是0，表示这个长度是固定值
                            if (field.MaxLength != 0)
                            {
                                saveMethodFieldCheck.Append(
                                    $"else if({field.Name}.length()!={field.MaxLength}){{throw new RuntimeException(\"字段 '{field.Name}' 的最大长度不能和最小长度一致。这次请求参数中最小长度和最大长度都是 {field.MinLength}.\");}}\n");
                            }
                        }
                        else
                        {
                            saveMethodFieldCheck.Append(
                                $"else if({field.Name}.length()>{field.MaxLength} || {field.Name}.length()<{field.MinLength}){{throw new RuntimeException(\"字段 '{field.Name}' 的值不符合规则, 文本的最小长度不能小于 {field.MinLength} ，最大长度不能大于 {field.MaxLength}.\");}}\n");
                        }
                    }
                    else if (field.IsNumberType())
                    {
                        // 是否为空
                        if (field.AllowNull) continue;
                        saveMethodFieldCheck.Append(
                            $"if({field.Name}==null){{throw new RuntimeException(\"字段 '{field.Name}' 不能是空的，但是在这里的请求参数却是空的.\");}}\n");

                        if (field.MinValue.Equals(field.MaxValue))
                        {
                            // 字段最小值和最大值相等，并且不是0，表示这个值是固定值
                            if (field.MaxValue != 0)
                            {
                                saveMethodFieldCheck.Append(
                                    $"else if({field.Name}!={field.MaxValue}){{throw new RuntimeException(\"字段 '{field.Name}' 得最大值和最小值结果不能是一样的，当前的最大值和最小值都是 {field.MinValue}.\");}}\n");
                            }
                        }
                        else
                        {
                            saveMethodFieldCheck.Append(
                                $"else if({field.Name}>{field.MaxValue} || {field.Name}<{field.MinValue}){{throw new RuntimeException(\"字段 {field.Name} 的值不符合规则, 这个字段的不能小于 {field.MinValue} 也不能大于 {field.MaxValue}.\");}}\n");
                        }
                    }
                    else
                    {
                        // 是否为空
                        if (field.AllowNull) continue;
                        saveMethodFieldCheck.Append(
                            $"if({field.Name}==null){{throw new RuntimeException(\"字段 '{field.Name}' 的规则要求不能是空的，但是在这里收到的参数是空的。\");}}\n");
                    }
                }
            }

            if (saveCallCreateBody.Length > 0)
            {
                saveCallCreateBody.Remove(saveCallCreateBody.Length - 1, 1);
            }


            stream.Write("""
                                     protected static class ##CLASS_NAME##_OnlyId{
                             public Integer id;
                         }
                         """.Replace("##CLASS_NAME##", className));

            // ============================================
            // getEntity方法
            // ============================================
            stream.Write("""
                                     /* 重写这个方法可以处理查询接口查询到的对象，这个返回结果将会直接返回给发起请求的客户端。**/
                         protected Object onHandleGetAfter(##CLASS_NAME##Dynamic object) {
                             return object;
                         }

                         @PostMapping("template/getEntity")
                         public Object getEntity(@RequestBody ##CLASS_NAME##_OnlyId onlyId) {
                             if (onlyId.id == null) {
                                 throw new RuntimeException("要查询的对象'id'格式不正确，id通常为Integer类型的数据，并且不能是空的。");
                             }
                             return this.onHandleGetAfter(new ##CLASS_NAME##Dynamic(getById(onlyId.id)));
                         }
                         """.Replace("##CLASS_NAME##", className));

            // ============================================
            // remove方法
            // ============================================
            stream.Write("""
                                     /* 重写这个方法可以处理remove接口在查询到要删除的对象，这个返回结果将会被传入数据库的删除方法。**/
                         protected ##CLASS_NAME## onHandleRemoveBefore(##CLASS_NAME## object) {
                             if(object==null){
                                 throw new RuntimeException("要删除的##CN_CLASS_NAME##对象不存在。");
                             }
                             return object;
                         }

                         /* 重写这个方法可以处理remove接口在删除之后的处理业务，这个返回结果将会直接返回给发起请求的客户端。**/
                         protected Object onHandleRemoveAfter(##CLASS_NAME## object) {
                             throw new RuntimeException("没有找到“onHandleRemoveAfter”方法的实现。");
                         }
                         """.Replace("##CLASS_NAME##", className).Replace("##CN_CLASS_NAME##", project.Table.CnName));
            stream.Write("""
                                     @Transactional
                                     @PostMapping("template/remove")
                         public Object remove(@RequestBody ##CLASS_NAME##_OnlyId onlyId){
                             if(onlyId.id==null){
                                 throw new RuntimeException("The object 'id' to be deleted cannot be empty.");
                             }
                             var object=getById(onlyId.id);
                             removeById(this.onHandleRemoveBefore(object).getId());
                             object.id=null;
                             return this.onHandleRemoveAfter(object);
                         }
                         """.Replace("##CLASS_NAME##", className));


            // =============================================
            // select方法
            // =============================================

            // 写入每个参数的Handle方法
            stream.Write(selectParamsHandleMethod);

            selectClassFields.Append(
                $"public Integer {project.PageFieldName};\npublic Integer {project.PageSizeFieldName};\n");

            // 生成Select格式化后的查询参数类
            stream.Write("""

                protected static class ##CLASS_NAME##_Select{
                
                    ##SELECT_CLASS_FIELDS##
                
                
                    public ##CLASS_NAME##_Select(Map<String,Object> params){
                    
                        var selectClass=getClass();
                        var fields=selectClass.getDeclaredFields();
                        for(var field : fields){
                            if(params.containsKey(field.getName())){
                                field.setAccessible(true);
                                try {
                                    if (field.getType().equals(Date.class) && params.get(field.getName())!=null) {
                                        if (params.get(field.getName()).getClass().equals(Long.class)) {
                                            field.set(this, new Date((Long) params.get(field.getName())));
                                        } else {
                                            throw new RuntimeException("无法将ResponseBody转换为目标查询参数，字段" + field.getName() + "无法按照预期转换。目标字段类型为java.lang.Date。");
                                        }
                                    } else {
                                        field.set(this, params.get(field.getName()));
                                    }
                                } catch (IllegalAccessException e) {
                                    throw new RuntimeException("无法将ResponseBody转换为目标查询参数，字段"+field.getName()+"无法按照预期转换。"+e.getMessage());
                                }
                            }
                        }
                        
                    }

                }

                """.Replace("##SELECT_CLASS_FIELDS##", selectClassFields.ToString())
                .Replace("##CLASS_NAME##", className)
            );
            stream.Write("""
                         /* 重写这个方法可以处理select接口在创建完查询条件之后的回调，这个返回结果将会被传入分页查询的方法。**/
                         protected LambdaQueryWrapper<##CLASS_NAME##> onHandleSelectBefore(##CLASS_NAME##_Select select , Map<String,Object> params , LambdaQueryWrapper<##CLASS_NAME##> queryWrapper){
                          return queryWrapper;
                         }
                         /* 重写这个方法可以处理select接口分页查询之后的结果，这个返回结果将会直接返回给发起请求的客户端。**/
                         protected Object onHandleSelectAfter(Page<##CLASS_NAME##Dynamic> result){
                          return result;
                         }
                         """.Replace("##CLASS_NAME##", className));

            stream.Write(
                "\n@PostMapping(\"template/select\")\n    public Object select(@RequestBody Map<String,Object> params){\n");
            stream.Write("""
                var select=new ##CLASS_NAME##_Select(params);
                if(select.##PAGE_FIELD##==null || select.##PAGE_FIELD##<1){
                    throw new RuntimeException("字段'##PAGE_FIELD##'的值不能小于1，这个字段是Integer类型，在这里也不能是空的。");
                }
                if(select.##PAGE_SIZE_FIELD##==null || select.##PAGE_SIZE_FIELD##<1 || select.##PAGE_SIZE_FIELD##>20){
                    throw new RuntimeException("字段'##PAGE_SIZE_FIELD##'的值不能小于1，并且不能大于20，这个字段是Integer类型，在这里也不能是空的。");
                }
                var query=new LambdaQueryWrapper<##CLASS_NAME##>();
                """.Replace("##CLASS_NAME##", className)
                .Replace("##PAGE_FIELD##", project.PageFieldName)
                .Replace("##PAGE_SIZE_FIELD##", project.PageSizeFieldName));
            stream.Write(selectBody);
            stream.Write("""
                         var result=page(new Page<>(select.page, select.pageSize), this.onHandleSelectBefore(select,params,query));
                         var dynamicResult=new Page<##CLASS_NAME##Dynamic>();
                         dynamicResult.setSize(result.getSize());
                         dynamicResult.setPages(result.getPages());
                         dynamicResult.setCurrent(result.getCurrent());
                         dynamicResult.setTotal(result.getTotal());
                         var dynamicObjects=new ArrayList<##CLASS_NAME##Dynamic>(result.getRecords().size());
                         for (##CLASS_NAME## record : result.getRecords()) {
                             dynamicObjects.add(new ##CLASS_NAME##Dynamic(record));
                         }
                         dynamicResult.setRecords(dynamicObjects);
                         return onHandleSelectAfter(dynamicResult);
                         }
                         """.Replace("##CLASS_NAME##", className));
            // =============================================
            // save方法
            // =============================================
            stream.Write("\nprotected static class ##CLASS_NAME##_Save {\n".Replace("##CLASS_NAME##", className) +
                         saveClassFields + "\npublic void checkLegality(){\n" + saveMethodFieldCheck + "\n}\n}\n");
            stream.Write(
                $"/* 重写这个方法可以处理save接口在调用数据库save方法之前的回调，这个返回结果将会被传入数据库save方法。**/\nprotected {className} onHandleSaveBefore({className} entity){{\n return entity;\n}}\n");
            stream.Write(
                $"/* 重写这个方法可以处理save接口在调用数据库save方法之后的回调，这个返回结果将会直接返回给发起请求的客户端。**/\nprotected Object onHandleSaveAfter({className} entity){{\n return entity;\n}}\n");
            stream.Write(
                "\n@Transactional\n@PostMapping(\"template/save\")\n public Object save(@RequestBody ##CLASS_NAME##_Save save){\n"
                    .Replace("##CLASS_NAME##", className));
            stream.Write("save.checkLegality();\n");
            stream.Write($"var object=create({saveCallCreateBody});\n");
            stream.Write($"return this.onHandleSaveAfter(saveEntity(this.onHandleSaveBefore(object)));\n");
            stream.Write("\n}");
            // ==============================================
            // 除主键id以外的字段修改方法
            // ==============================================
            foreach (var field in project.Table.Columns)
            {
                if (field.Name.Equals("id") || !field.AllowSetField)
                {
                    // ID字段和不允许修改字段的属性不创建修改方法
                    continue;
                }

                if (field.Name.Length == 0)
                {
                    continue;
                }

                // 首字母大写的字段名
                var uppercaseFieldName = field.Name[..1].ToUpper() + field.Name[1..];
                // 值检查，值的字段名为##FIELD_NAME##
                var checkValueText = new StringBuilder();
                // 检查是否为null
                if (!field.AllowNull)
                {
                    checkValueText.Append(
                        "if(##FIELD_NAME##==null){throw new RuntimeException(\"字段“##FIELD_NAME##”的值不能是空的。\");}\n"
                            .Replace("##FIELD_NAME##", field.Name));
                }

                // 检查是否是字符串并且检查字符串允许的长度
                if (field.IsTextType())
                {
                    checkValueText.Append(
                        "if(##FIELD_NAME##.length() < ##MIN_LENGTH## || ##FIELD_NAME##.length() > ##MAX_LENGTH##){throw new RuntimeException(\"字段“##FIELD_NAME##”的内容长度格式不正确，内容长度不能小于##MIN_LENGTH##位，不能大于##MAX_LENGTH##位。\");}\n"
                            .Replace("##FIELD_NAME##", field.Name)
                            .Replace("##MIN_LENGTH##", field.MinLength.ToString())
                            .Replace("##MAX_LENGTH##", field.MaxLength.ToString())
                    );
                }

                // 检查数字的限制
                if (field.IsNumberType())
                {
                    checkValueText.Append(
                        "if(##FIELD_NAME## < ##MIN_VALUE## || ##FIELD_NAME## > ##MAX_VALUE##){throw new RuntimeException(\"字段“##FIELD_NAME##”的值不正确，值不能小于##MIN_VALUE##，不能大于##MAX_VALUE##。\");}\n"
                            .Replace("##FIELD_NAME##", field.Name)
                            .Replace("##MIN_VALUE##", field.MinValue.ToString(CultureInfo.InvariantCulture))
                            .Replace("##MAX_VALUE##", field.MaxValue.ToString(CultureInfo.InvariantCulture))
                    );
                }

                // 创建属性修改参数
                var scriptText = new StringBuilder();
                scriptText.Append("""
                    protected static class ##CLASS_NAME##_Set##UPPERCASE_FIELD_NAME##{
                    
                        // 主键id
                        public Integer id;
                    
                        // ##COMMENT##
                        public ##TYPE## ##FIELD_NAME##;
                    }

                    protected ##CLASS_NAME## onHandleSet##UPPERCASE_FIELD_NAME##Before(##CLASS_NAME## entity,##TYPE## ##FIELD_NAME##)
                    {
                        // 检查值是否符合规则
                        ##CHECK_TEXT##
                        // 直接调用相应的set方法
                        entity.set##UPPERCASE_FIELD_NAME##(##FIELD_NAME##);
                        return entity;
                    }

                    protected ##CLASS_NAME## onHandleSet##UPPERCASE_FIELD_NAME##After(##CLASS_NAME## entity)
                    {
                        return entity;
                    }

                    @Transactional
                    @PostMapping("template/set##UPPERCASE_FIELD_NAME##")
                    public Object set##UPPERCASE_FIELD_NAME##(@RequestBody ##CLASS_NAME##_Set##UPPERCASE_FIELD_NAME## _newValue)
                    {
                        var entity=getEntityById(_newValue.id);
                        if(entity==null){throw new RuntimeException("要修改的“##CN_CLASS_NAME##”对象不存在。");}
                        entity=onHandleSet##UPPERCASE_FIELD_NAME##Before(entity,_newValue.##FIELD_NAME##);
                        saveEntity(entity);
                        return onHandleSet##UPPERCASE_FIELD_NAME##After(entity);
                    }
                    """
                    .Replace("##UPPERCASE_FIELD_NAME##", uppercaseFieldName)
                    .Replace("##TYPE##", field.ToJavaType())
                    .Replace("##FIELD_NAME##", field.Name)
                    .Replace("##CLASS_NAME##", className)
                    .Replace("##CN_CLASS_NAME##", project.Table.CnName)
                    .Replace("##COMMENT##", field.Name + "," + field.Comment)
                    .Replace("##CHECK_TEXT##", checkValueText.ToString())
                );
                stream.Write(scriptText.ToString());
            }


            // 创建通过“Save”创建实例函数
            var createWithSave = new StringBuilder();
            createWithSave.Append("""

                public ##CLASS_NAME## createWithSave(##CLASS_NAME##_Save save)
                {
                    save.checkLegality();
                    ##CLASS_NAME## resultObject;
                    var targetObject = create(##CREATE_PARAMS##);
                    if (save.id != null) {
                        resultObject = getOneEqualNotNull(##CLASS_NAME##::getId, save.id);
                        ##CLASS_NAME##.copy(resultObject, targetObject);
                    } else {
                        resultObject = targetObject;
                    }
                    return resultObject;
                }

                """
                .Replace("##CREATE_PARAMS##", saveCallCreateBody.ToString())
                .Replace("##CLASS_NAME##", className)
            );


            stream.Write(createWithSave.ToString());

            stream.Write("\n}");
            stream.Close();
        }
    }
}