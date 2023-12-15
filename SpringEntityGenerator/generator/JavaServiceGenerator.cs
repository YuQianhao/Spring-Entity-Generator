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
            var filePath = project.Path + "\\src\\main\\java\\" + project.PackageName.Replace(".", "\\") + "\\service\\template\\";
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
                package ####PACKAGE_NAME####.service.template;

                import com.baomidou.mybatisplus.core.metadata.IPage;
                import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
                import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
                import ####PACKAGE_NAME####.entity.####CLASS_NAME####;
                import ####PACKAGE_NAME####.mapper.####CLASS_NAME####Mapper;
                import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
                import java.util.List;
                import java.lang.reflect.Field;
                import java.lang.RuntimeException;
                import com.baomidou.mybatisplus.core.toolkit.support.SFunction;
                import org.jetbrains.annotations.NotNull;
                import org.springframework.stereotype.Service;
                import java.util.*;
                import java.util.function.Consumer;

                @Service
                public class ####CLASS_NAME####ServiceTemplate extends ServiceImpl<####CLASS_NAME####Mapper, ####CLASS_NAME####> {
                """.Replace("####CLASS_NAME####", className).Replace("####PACKAGE_NAME####", project.PackageName)
            );
            // =============================================
            // 创建动态类型，提供给select方法和getEntity方法使用
            // =============================================
            stream.Write("""

                /**
                 * 内置构建的动态类型，这个类型可以在数据结构固有字段的基础上，额外进行一些修改
                 */
                public static class ##CLASS_NAME##Dynamic extends HashMap<String, Object> {

                    public ##CLASS_NAME##Dynamic(##CLASS_NAME## entity) {
                        var classType = entity.getClass();
                        try {
                            for (Field field : classType.getDeclaredFields()) {
                                if(field.getName().startsWith("_$tp_")){
                                    continue;
                                }
                                field.setAccessible(true);
                                add(field.getName(), field.get(entity));
                            }
                        } catch (Exception e) {
                            throw new RuntimeException("类型" + classType.getName() + "无法被创建成为" + getClass().getName() + "，在创建字段时发生错误。" + e.getMessage());
                        }
                    }

                    public static List<##CLASS_NAME##Dynamic> formatList(List<##CLASS_NAME##> itemList){
                        var result=new ArrayList<##CLASS_NAME##Dynamic>(itemList.size());
                        for (##CLASS_NAME## item : itemList) {
                            result.add(new ##CLASS_NAME##Dynamic(item));
                        }
                        return result;
                    }

                    public ##CLASS_NAME##Dynamic add(String key, Object value) {
                        remove(key);
                        put(key, value);
                        return this;
                    }

                    public ##CLASS_NAME## to##CLASS_NAME##() {
                        try {
                            var entity = new ##CLASS_NAME##();
                            var classType = entity.getClass();
                            var fields = classType.getDeclaredFields();
                            for (String key : keySet()) {
                                var fieldOptional = Arrays.stream(fields).filter(item -> item.getName().equals(key)).findFirst();
                                if (fieldOptional.isPresent()) {
                                    var field = fieldOptional.get();
                                    field.setAccessible(true);
                                    field.set(entity, get(key));
                                }
                            }
                            return entity;
                        } catch (Exception e) {
                            throw new RuntimeException("将##CLASS_NAME##Dynamic转换成##CLASS_NAME##时发生错误。" + e.getMessage());
                        }
                    }

                }

                """.Replace("##CLASS_NAME##", className));
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
            stream.Write($"""
                {className} object = getById({project.Table.Columns.Find(item => item.Key)?.Name});
                if(id!=null && object==null){"{"} 
                    throw new RuntimeException("要修改的{project.Table.CnName}数据对象不存在。");
                {"}"}
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
            // ===================================
            // 生成可以容纳5个字段的查询、列表和数量方法
            // ===================================
            // 查询单个的方法
            for (var i = 0; i < 5; i++)
            {
                stream.Write("\n");
                // 参数列表
                var functionParams = new StringBuilder();
                // 条件列表
                var conditionText = new StringBuilder();
                for (var paramsSize = 0; paramsSize < i + 1; paramsSize++)
                {
                    var key = $"_{paramsSize + 1}_key";
                    var value = $"_{paramsSize + 1}_value";
                    functionParams.Append($"SFunction<{className}, ?> {key}, Object {value},");
                    conditionText.Append($".eq({key}, {value})");
                }
                if (functionParams.Length > 0)
                {
                    functionParams.Remove(functionParams.Length - 1,1);
                }
                // getOneEqual方法
                stream.Write("""
                                    public ####CLASSNAME#### getOneEqual(####PARAMS####) {
                        return getOne(new LambdaQueryWrapper<####CLASSNAME####>()####CONDITION####);
                    }
                    """.Replace("####PARAMS####", functionParams.ToString())
                    .Replace("####CLASSNAME####", className)
                    .Replace("####CONDITION####", conditionText.ToString()));
                stream.Write("\n");
                // getOneEqualNotNull方法
                stream.Write("""
                                    public ####CLASSNAME#### getOneEqualNotNull(####PARAMS####) {
                        var _value=getOne(new LambdaQueryWrapper<####CLASSNAME####>()####CONDITION####);
                        if(_value==null){
                            throw new RuntimeException("要查询的“####CN_CLASSNAME####”对象不存在。");
                        }
                        return _value;
                    }
                    """.Replace("####PARAMS####", functionParams.ToString())
                    .Replace("####CLASSNAME####", className)
                    .Replace("####CN_CLASSNAME####", project.Table.CnName)
                    .Replace("####CONDITION####", conditionText.ToString()));
                stream.Write("\n");
                // remove
                stream.Write("""
                                    public int removeEqual(####PARAMS####) {
                        return getBaseMapper().delete(new LambdaQueryWrapper<####CLASSNAME####>()####CONDITION####);
                    }
                    """.Replace("####PARAMS####", functionParams.ToString())
                    .Replace("####CLASSNAME####", className)
                    .Replace("####CONDITION####", conditionText.ToString()));
                stream.Write("\n");
                // listEqual
                stream.Write("""
                                    public List<####CLASSNAME####> listEqual(####PARAMS####) {
                        return getBaseMapper().selectList(new LambdaQueryWrapper<####CLASSNAME####>()####CONDITION####);
                    }
                    """.Replace("####PARAMS####", functionParams.ToString())
                    .Replace("####CLASSNAME####", className)
                    .Replace("####CONDITION####", conditionText.ToString()));
                stream.Write("\n");
                // getCountEqual
                stream.Write("""
                                    public Long getCountEqual(####PARAMS####) {
                        return getBaseMapper().selectCount(new LambdaQueryWrapper<####CLASSNAME####>()####CONDITION####);
                    }
                    """.Replace("####PARAMS####", functionParams.ToString())
                    .Replace("####CLASSNAME####", className)
                    .Replace("####CONDITION####", conditionText.ToString()));
            }
            // ==============================================
            //          生成创建lambda查询对象方法
            // ==============================================
            stream.Write("""

                public LambdaQueryWrapper<####CLASS_NAME####> lambda(){
                    return new LambdaQueryWrapper<####CLASS_NAME####>();
                }
                """.Replace("####CLASS_NAME####", className));
            // ==============================================
            //          生成创建page查询对象方法
            // ==============================================
            stream.Write("""

                public IPage<####CLASS_NAME####> page(Integer page, Integer pageSize, LambdaQueryWrapper<####CLASS_NAME####> queryWrapper){
                    if(page==null || page<0){
                        throw new RuntimeException("分页页码page格式不正确。");
                    }
                    if(pageSize==null || pageSize<0){
                        throw new RuntimeException("分页参数pageSize格式不正确。");
                    }
                    return page(new Page<>(page,pageSize),queryWrapper);
                }

                """.Replace("####CLASS_NAME####", className));
            stream.Write("""

                public IPage<####CLASS_NAME####> page(Integer page, Integer pageSize){
                    if(page==null || page<0){
                        throw new RuntimeException("分页页码page格式不正确。");
                    }
                    if(pageSize==null || pageSize<0){
                        throw new RuntimeException("分页参数pageSize格式不正确。");
                    }
                    return page(new Page<>(page,pageSize));
                }

                """.Replace("####CLASS_NAME####", className));
            // ==============================================
            //          生成创建operatoe查询对象方法
            // ==============================================
            stream.Write("""

                            public ####CLASS_NAME####Operator operator(){
                    return new ####CLASS_NAME####Operator(getBaseMapper());
                }

                public static class ####CLASS_NAME####Operator{

                    private final LambdaQueryWrapper<####CLASS_NAME####> queryWrapper=new LambdaQueryWrapper<>();

                    private final ####CLASS_NAME####Mapper baseMapper;

                    public ####CLASS_NAME####Operator(####CLASS_NAME####Mapper baseMapper) {
                        this.baseMapper = baseMapper;
                    }

                    public ####CLASS_NAME####Operator and(Consumer<LambdaQueryWrapper<####CLASS_NAME####>> consumer){
                        queryWrapper.and(consumer);
                        return this;
                    }

                    public ####CLASS_NAME####Operator or(Consumer<LambdaQueryWrapper<####CLASS_NAME####>> consumer){
                        queryWrapper.or(consumer);
                        return this;
                    }

                    public ####CLASS_NAME####Operator notNull(SFunction<####CLASS_NAME####,?> function){
                        queryWrapper.isNotNull(function);
                        return this;
                    }

                    public ####CLASS_NAME####Operator eq(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.eq(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator ne(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.ne(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator le(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.le(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator lt(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.lt(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator ge(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.ge(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator gt(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.gt(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator like(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.like(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator notLike(SFunction<####CLASS_NAME####,?> function,Object value){
                        queryWrapper.notLike(function,value);
                        return this;
                    }

                    public ####CLASS_NAME####Operator orderByAsc(SFunction<####CLASS_NAME####,?> function){
                        queryWrapper.orderByAsc(function);
                        return this;
                    }

                    public ####CLASS_NAME####Operator orderByDesc(SFunction<####CLASS_NAME####,?> function){
                        queryWrapper.orderByDesc(function);
                        return this;
                    }

                    public ####CLASS_NAME#### selectOne(){
                        return baseMapper.selectOne(queryWrapper);
                    }

                    public List<####CLASS_NAME####> selectList(){
                        return baseMapper.selectList(queryWrapper);
                    }

                    public ####CLASS_NAME#### selectOnlyFirst(){
                        return baseMapper.selectOne(queryWrapper.last("LIMIT 1"));
                    }

                    public long selectCount(){
                        return baseMapper.selectCount(queryWrapper);
                    }

                    public IPage<####CLASS_NAME####> selectPage(Integer page, Integer pageSize) {
                        if (page == null || page < 0) {
                            throw new RuntimeException("分页页码page格式不正确。");
                        }
                        if (pageSize == null || pageSize < 0) {
                            throw new RuntimeException("分页参数pageSize格式不正确。");
                        }
                        return baseMapper.selectPage(new Page<>(page, pageSize),queryWrapper);
                    }

                }

                """.Replace("####CLASS_NAME####", className));
            stream.Write("}");
            stream.Close();
        }
    }
}
