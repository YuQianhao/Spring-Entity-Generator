using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpringEntityGenerator.Models;

namespace SpringEntityGenerator.Generators
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
                package ##PACKAGE_NAME##.service.template;

                import com.baomidou.mybatisplus.core.metadata.IPage;
                import com.baomidou.mybatisplus.extension.plugins.pagination.Page;
                import com.baomidou.mybatisplus.extension.service.impl.ServiceImpl;
                import ##PACKAGE_NAME##.entity.##CLASS_NAME##;
                import ##PACKAGE_NAME##.mapper.##CLASS_NAME##Mapper;
                import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
                import java.util.List;
                import java.lang.reflect.Field;
                import java.lang.RuntimeException;
                import com.baomidou.mybatisplus.core.toolkit.support.SFunction;
                import org.jetbrains.annotations.NotNull;
                import org.springframework.stereotype.Service;
                import java.util.*;
                import jakarta.annotation.PostConstruct;
                import java.util.function.Consumer;

                /**
                 * ##CLASS_NAME##相关的数据库服务，这个服务类实例遵循Spring Bean生命周期。
                 */
                @Service
                public class ##CLASS_NAME##ServiceTemplate extends ServiceImpl<##CLASS_NAME##Mapper, ##CLASS_NAME##> {
                """.Replace("##CLASS_NAME##", className).Replace("##PACKAGE_NAME##", project.PackageName)
            );

            // 创建静态对象，用于在与ServiceTemplate无关联的业务中使用ServiceTemplate
            stream.Write("""
                
                /**
                 * 已经生效的Java Bean静态实例对象
                 */
                private static ##CLASS_NAME##ServiceTemplate _$tp_instance=null;

                /**
                 * Spring Bean创建成功的回调，将Bean实例赋值为静态实例对象
                 */
                @PostConstruct
                private void _$_tp_init(){
                    _$tp_instance=this;
                }

                /**
                 * 在Spring Bean生命周期外获取当前类的实例对象
                 */
                public static ##CLASS_NAME##ServiceTemplate getInstance(){
                    if(_$tp_instance==null){
                        throw new RuntimeException(##CLASS_NAME##ServiceTemplate.class.getName()+"未在目标生命周期中初始化完成，无法使用，请确保主调函数在Spring初始化完成之后调用。");
                    }
                    return _$tp_instance;
                }

                """.Replace("##CLASS_NAME##",className));

            // =============================================
            // 创建动态类型，提供给select方法和getEntity方法使用
            // =============================================
            // 创建动态类型之前，需要将这个数据结构的属性解包，创建相应的get，set，remove方法
            var dynamicStructInlineMethod = new StringBuilder() ;
            foreach(var field in project.Table.Columns)
            {

                dynamicStructInlineMethod.Append("""

                        /**
                         * 修改字段##FIELD_NAME##的值
                         */    
                        public ##CLASS_NAME##Dynamic set##FIRST_BIG_FIELD_NAME##(Object value){
                            set("##FIELD_NAME##",value);
                            return this;
                        }


                        /**
                         * 获取字段##FIELD_NAME##的值
                         */ 
                        public Object get##FIRST_BIG_FIELD_NAME##(){
                            return get("##FIELD_NAME##");
                        }

                        /**
                         * 删除字段##FIELD_NAME##
                         */ 
                        public ##CLASS_NAME##Dynamic remove##FIRST_BIG_FIELD_NAME##(){
                        
                            remove("##FIELD_NAME##");
                            return this;
                        }

                    """.Replace("##CLASS_NAME##", className)
                    .Replace("##FIRST_BIG_FIELD_NAME##", char.ToUpper(field.Name[0])+ field.Name.Substring(1))
                    .Replace("##FIELD_NAME##", field.Name)
                    );

            }
            stream.Write("""

                /**
                 * ##CLASS_NAME##类的动态类型，动态类型是将##CLASS_NAME##的所有字段拆分成一个Map对象，可以通过这个Dynamic对象灵活的在##CLASS_NAME##的基础上增加或者减少字段。
                 */
                public static class ##CLASS_NAME##Dynamic extends HashMap<String, Object> {

                    private ##CLASS_NAME## _object;

                    public ##CLASS_NAME##Dynamic(##CLASS_NAME## entity) {
                        _object=entity;
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

                    ##INLINE_FIELD_METHOD##

                    /**
                     * 获取最后一次序列化的对象的引用<br/>
                     * 这个方法不会通过反射创建对象，而是将{@link ##CLASS_NAME##Dynamic#to##CLASS_NAME##()}最后一次产生的结果返回，如果{@link ##CLASS_NAME##Dynamic#to##CLASS_NAME##()}
                     * 一次也没有被调用，那么这个方法返回的是创建当前Dynamic动态类型实例的对象
                     */
                    public ##CLASS_NAME## getReference##CLASS_NAME##(){
                        return _object;
                    }

                    /**
                     * 通用的字段值修改方法
                     * @param fieldName 字段名
                     * @param value     值
                     */
                    public ##CLASS_NAME##Dynamic set(String fieldName,Object value){
                    
                        remove(fieldName);
                        put(fieldName,value);
                        return this;

                    }
                    
                    /**
                     * 将多个{@link ##CLASS_NAME##}对象穿换成多个对应的{@link ##CLASS_NAME##Dynamic}
                     */
                    public static List<##CLASS_NAME##Dynamic> formatList(List<##CLASS_NAME##> itemList){
                        var result=new ArrayList<##CLASS_NAME##Dynamic>(itemList.size());
                        for (##CLASS_NAME## item : itemList) {
                            result.add(new ##CLASS_NAME##Dynamic(item));
                        }
                        return result;
                    }

                    /**
                     * 通用的字段增加方法，调用这个方法为当前对象增加一个字段
                     * @param key       字段名
                     * @param value     值
                     */
                    public ##CLASS_NAME##Dynamic add(String key, Object value) {
                        remove(key);
                        put(key, value);
                        return this;
                    }

                    /**
                     * 根据当前的字段表，反射创建一个全新的对象
                     */
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
                            _object=entity;
                            return entity;
                        } catch (Exception e) {
                            throw new RuntimeException("将##CLASS_NAME##Dynamic转换成##CLASS_NAME##时发生错误。" + e.getMessage());
                        }
                    }

                }

                """.Replace("##CLASS_NAME##", className)
                .Replace("##INLINE_FIELD_METHOD##",dynamicStructInlineMethod.ToString()));
            // ===================================
            // 增加Create方法
            // ===================================
            stream.Write("""
                         /**
                          * 创建一个{@link ##CLASS_NAME##}的实例对象
                          */
                         """.Replace("##CLASS_NAME##", className));
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
            stream.Write("""
                         /**
                          * 创建一个{@link ##CLASS_NAME##}对象并保存，如果参数id不是null，会调用对应的updateById更新对象，否则会调用insert插入对象。
                          */
                         """.Replace("##CLASS_NAME##", className));
            stream.Write($"\npublic {className} save({saveMultipleParamsText})" + "{\n");
            stream.Write($"""
                {className} object = null;
                if(id!=null){"{"}
                    object=getById({project.Table.Columns.Find(item => item.Key)?.Name});
                {"}"}
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
            stream.Write("""
                         /**
                          * 保存一个非空的{@link ##CLASS_NAME##}实例对象
                          */
                         """.Replace("##CLASS_NAME##", className));
            stream.Write($"\npublic {className} saveEntity(@NotNull {className} object)" + "{\n");
            stream.Write($"return this.save({saveSingleBodyText});\n}}\n");
            // ===================================
            // 增加get方法
            // ===================================
            stream.Write("""
                         /**
                          * 根据ID查询对象
                          */
                         """);
            stream.Write($"public {className} getEntityById(@NotNull " + project.Table.Columns.Find(item => item.Key)?.ToJavaType() + " selectKey){\n");
            stream.Write("return getById(selectKey);\n}\n");
            // ===================================
            // 增加remove方法
            // ===================================
            stream.Write("""
                         /**
                          * 根据ID删除对象
                          */
                         """);
            stream.Write($"public void removeById(@NotNull " + project.Table.Columns.Find(item => item.Key)?.ToJavaType() + " removeKey){\n");
            stream.Write("getBaseMapper().deleteById(removeKey);\n}\n");
            // ===================================
            // 增加removeEntity方法
            // ===================================
            stream.Write("""
                         /**
                          * 删除一个非空的##CLASS_NAME##对象
                          */
                         """.Replace("##CLASS_NAME##", className));
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
                    /**
                     * 根据字段相等条件查询单个匹配的对象。如果有多个匹配的查询结果，将会抛出异常。参数key接受一个lambda函数表达式，value接受一个值，这个值不能是null。
                     * <p>lambda函数表达式示例：</p>
                     * <p>假设{@code class LambdaTest}中有方法{@code String getName()}</p>
                     * <p>他对应的lambda函数表达式调用方式是{@code LambdaTest::getName}。详情请参阅关于Java 8中关于{@code Function}的说明。</p>
                     */
                                    public ##CLASSNAME## getOneEqual(##PARAMS##) {
                        return getOne(new LambdaQueryWrapper<##CLASSNAME##>()##CONDITION##);
                    }
                    """.Replace("##PARAMS##", functionParams.ToString())
                    .Replace("##CLASSNAME##", className)
                    .Replace("##CONDITION##", conditionText.ToString()));
                stream.Write("\n");
                // getOneEqualNotNull方法
                stream.Write("""
                    /**
                     * 根据字段相等条件查询单个匹配的对象。如果有多个匹配的查询结果，将会抛出异常，如果查询的对象不存在，也将会抛出异常。参数key接受一个lambda函数表达式，value接受一个值，这个值不能是null。
                     * <p>lambda函数表达式示例：</p>
                     * <p>假设{@code class LambdaTest}中有方法{@code String getName()}</p>
                     * <p>他对应的lambda函数表达式调用方式是{@code LambdaTest::getName}。详情请参阅关于Java 8中关于{@code Function}的说明。</p>
                     */
                                    public ##CLASSNAME## getOneEqualNotNull(##PARAMS##) {
                        var _value=getOne(new LambdaQueryWrapper<##CLASSNAME##>()##CONDITION##);
                        if(_value==null){
                            throw new RuntimeException("要查询的“##CN_CLASSNAME##”对象不存在。");
                        }
                        return _value;
                    }
                    """.Replace("##PARAMS##", functionParams.ToString())
                    .Replace("##CLASSNAME##", className)
                    .Replace("##CN_CLASSNAME##", project.Table.CnName)
                    .Replace("##CONDITION##", conditionText.ToString()));
                stream.Write("\n");
                // remove
                stream.Write("""
                    /**
                     * 根据字段相等条件删除所有匹配的数据。参数key接受一个lambda函数表达式，value接受一个值，这个值不能是null。
                     * <p>lambda函数表达式示例：</p>
                     * <p>假设{@code class LambdaTest}中有方法{@code String getName()}</p>
                     * <p>他对应的lambda函数表达式调用方式是{@code LambdaTest::getName}。详情请参阅关于Java 8中关于{@code Function}的说明。</p>
                     */
                                    public int removeEqual(##PARAMS##) {
                        return getBaseMapper().delete(new LambdaQueryWrapper<##CLASSNAME##>()##CONDITION##);
                    }
                    """.Replace("##PARAMS##", functionParams.ToString())
                    .Replace("##CLASSNAME##", className)
                    .Replace("##CONDITION##", conditionText.ToString()));
                stream.Write("\n");
                // listEqual
                stream.Write("""
                    /**
                     * 根据字段相等条件查询多个匹配的对象。参数key接受一个lambda函数表达式，value接受一个值，这个值不能是null。
                     * <p>lambda函数表达式示例：</p>
                     * <p>假设{@code class LambdaTest}中有方法{@code String getName()}</p>
                     * <p>他对应的lambda函数表达式调用方式是{@code LambdaTest::getName}。详情请参阅关于Java 8中关于{@code Function}的说明。</p>
                     */
                                    public List<##CLASSNAME##> listEqual(##PARAMS##) {
                        return getBaseMapper().selectList(new LambdaQueryWrapper<##CLASSNAME##>()##CONDITION##);
                    }
                    """.Replace("##PARAMS##", functionParams.ToString())
                    .Replace("##CLASSNAME##", className)
                    .Replace("##CONDITION##", conditionText.ToString()));
                stream.Write("\n");
                // getCountEqual
                stream.Write("""
                    /**
                     * 根据字段相等条件查询匹配成功的对象数量。参数key接受一个lambda函数表达式，value接受一个值，这个值不能是null。
                     * <p>lambda函数表达式示例：</p>
                     * <p>假设{@code class LambdaTest}中有方法{@code String getName()}</p>
                     * <p>他对应的lambda函数表达式调用方式是{@code LambdaTest::getName}。详情请参阅关于Java 8中关于{@code Function}的说明。</p>
                     */
                                    public Long getCountEqual(##PARAMS##) {
                        return getBaseMapper().selectCount(new LambdaQueryWrapper<##CLASSNAME##>()##CONDITION##);
                    }
                    """.Replace("##PARAMS##", functionParams.ToString())
                    .Replace("##CLASSNAME##", className)
                    .Replace("##CONDITION##", conditionText.ToString()));
            }
            // ==============================================
            //          生成创建lambda查询对象方法
            // ==============================================
            stream.Write("""
                /**
                 * 创建当前一个当前类结构的{@link LambdaQueryWrapper}实例
                 */
                public LambdaQueryWrapper<##CLASS_NAME##> lambda(){
                    return new LambdaQueryWrapper<##CLASS_NAME##>();
                }
                """.Replace("##CLASS_NAME##", className));
            // ==============================================
            //          生成创建page查询对象方法
            // ==============================================
            stream.Write("""
                /**
                 * 按照分页和条件查询数据
                 * @param page              分页页码
                 * @param pageSize          数据页的长度
                 * @param queryWrapper      查询条件
                 */
                public IPage<##CLASS_NAME##> page(Integer page, Integer pageSize, LambdaQueryWrapper<##CLASS_NAME##> queryWrapper){
                    if(page==null || page<0){
                        throw new RuntimeException("分页页码page格式不正确。");
                    }
                    if(pageSize==null || pageSize<0){
                        throw new RuntimeException("分页参数pageSize格式不正确。");
                    }
                    return page(new Page<>(page,pageSize),queryWrapper);
                }

                """.Replace("##CLASS_NAME##", className));
            stream.Write("""
                /**
                 * 按照分页查询数据
                 * @param page              分页页码
                 * @param pageSize          数据页的长度
                 */
                public IPage<##CLASS_NAME##> page(Integer page, Integer pageSize){
                    if(page==null || page<0){
                        throw new RuntimeException("分页页码page格式不正确。");
                    }
                    if(pageSize==null || pageSize<0){
                        throw new RuntimeException("分页参数pageSize格式不正确。");
                    }
                    return page(new Page<>(page,pageSize));
                }

                """.Replace("##CLASS_NAME##", className));
            // ==============================================
            //          生成创建operatoe查询对象方法
            // ==============================================
            stream.Write("""

                /**
                 * 创建一个{@link ##CLASS_NAME##Operator}对象实例
                 */
                            public ##CLASS_NAME##Operator operator(){
                    return new ##CLASS_NAME##Operator(getBaseMapper());
                }

                /**
                 * 链式查询操作包装类。
                 * <p>通过这个类可以快速构建一个查询条件链，可以在查询条件链的每一个节点分别做查询操作，并且不会影响到查询条件链的其他节点。这个类是对{@link LambdaQueryWrapper}的包装。</p>
                 */
                public static class ##CLASS_NAME##Operator{

                    /**
                     * 当前查询条件的{@code LambdaQueryWrapper}包装
                     */
                    private final LambdaQueryWrapper<##CLASS_NAME##> queryWrapper=new LambdaQueryWrapper<>();

                    /**
                     * 数据库{@code mapper}对象的引用
                     */
                    private final ##CLASS_NAME##Mapper baseMapper;

                    public ##CLASS_NAME##Operator(##CLASS_NAME##Mapper baseMapper) {
                        this.baseMapper = baseMapper;
                    }

                    /**
                     * 设置本次查询只查询的字段
                     * @param columns   仅查询的字段lambda函数表达式列表
                     *                  <p>假设{@code class LambdaTest}中有3个字段，分别是{@code id,name,age}，他们分别对应{@code getId(),getName(),getAge()}</p>
                     *                  三个方法，如果本次查询仅查询{@code id,name}两个字段，就需要在此处传入{@code LambdaTest::getId}和{@code LambdaTest::getName}，明确
                     *                  的指出要查询的字段。
                     */
                    @SafeVarargs
                    public final ##CLASS_NAME##Operator onlyColumn(SFunction<##CLASS_NAME##, ?>... columns){
                        queryWrapper.select(columns);
                        return this;
                    }

                    /**
                     * 在查询条件拼接完成后的末尾增加一条sql，这条sql不会被检查，而是直接拼接到sql的末尾。
                     */
                    public ##CLASS_NAME##Operator lastSql(String sql){
                        queryWrapper.last(sql);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#and(Consumer)}的包装。
                     */
                    public ##CLASS_NAME##Operator and(Consumer<LambdaQueryWrapper<##CLASS_NAME##>> consumer){
                        queryWrapper.and(consumer);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#or(Consumer)}的包装。
                     */
                    public ##CLASS_NAME##Operator or(Consumer<LambdaQueryWrapper<##CLASS_NAME##>> consumer){
                        queryWrapper.or(consumer);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#isNotNull(Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator notNull(SFunction<##CLASS_NAME##,?> function){
                        queryWrapper.isNotNull(function);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#isNull(Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator isNull(SFunction<##CLASS_NAME##,?> function){
                        queryWrapper.isNull(function);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#eq(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator eq(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.eq(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#ne(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator ne(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.ne(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#le(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator le(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.le(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#lt(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator lt(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.lt(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#ge(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator ge(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.ge(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#gt(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator gt(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.gt(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#like(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator like(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.like(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#notLike(Object, Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator notLike(SFunction<##CLASS_NAME##,?> function,Object value){
                        queryWrapper.notLike(function,value);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#orderByAsc(Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator orderByAsc(SFunction<##CLASS_NAME##,?> function){
                        queryWrapper.orderByAsc(function);
                        return this;
                    }

                    /**
                     * 对{@link LambdaQueryWrapper#orderByDesc(Object)}的包装。
                     */
                    public ##CLASS_NAME##Operator orderByDesc(SFunction<##CLASS_NAME##,?> function){
                        queryWrapper.orderByDesc(function);
                        return this;
                    }

                    /**
                     * 根据查询条件查询相匹配的一个结果。如果结果存在多个，将会抛出异常。
                     */
                    public ##CLASS_NAME## selectOne(){
                        return baseMapper.selectOne(queryWrapper);
                    }

                    /**
                     * 根据查询条件查询所有匹配的结果。
                     */
                    public List<##CLASS_NAME##> selectList(){
                        return baseMapper.selectList(queryWrapper);
                    }

                    /**
                     * 根据查询条件查询相匹配的一个结果。如果结果存在多个，只返回第一条查询结果。
                     */
                    public ##CLASS_NAME## selectOnlyFirst(){
                        return baseMapper.selectOne(queryWrapper.last("LIMIT 1"));
                    }

                    /**
                     * 根据查询条件查询匹配的数据数量
                     */
                    public long selectCount(){
                        return baseMapper.selectCount(queryWrapper);
                    }

                    /**
                     * 根据查询条件分页查询数据
                     * @param page      查询的页码
                     * @param pageSize  数据页的长度
                     */
                    public IPage<##CLASS_NAME##> selectPage(Integer page, Integer pageSize) {
                        if (page == null || page < 0) {
                            throw new RuntimeException("分页页码page格式不正确。");
                        }
                        if (pageSize == null || pageSize < 0) {
                            throw new RuntimeException("分页参数pageSize格式不正确。");
                        }
                        return baseMapper.selectPage(new Page<>(page, pageSize),queryWrapper);
                    }

                }

                """.Replace("##CLASS_NAME##", className));
            stream.Write("}");
            stream.Close();
        }
    }
}
