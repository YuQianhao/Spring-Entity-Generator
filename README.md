# Spring Entity Generator

**<font color="red">该项目已停止维护，新项目请移驾至“[YuQianhao/spring-generate: Spring's rapid development tools. (github.com)](https://github.com/YuQianhao/spring-generate)”。</font>**

``Spring Boot``和``Mybatis Plus``专用的数据生成器。

这个工具能根据你配置的数据库表结构，直接在数据库中创建相应的``表结构``，在``Spring Boot``项目代码中生成可以直接使用的``ControllerTemplate``，``ServiceTemplate``，``Mapper``，自动创建``增删改查``方法接口。

接口生成默认全部使用的``Post``。可以在``\generator``目录中找到对应的生成器，进行修改即可。

## 日志

[2024年3月8日]

优化了代码注释结构。

[2024年1月17日]

修复了一些问题。

[2024年1月16日]

增加了一些功能，让代码模板工程化。

[2024年1月5日]

1、优化了每个``ServiceTemplate``生成的``Operator``对象。

[2024年1月3日]

1、优化了``ServiceTemplate``。

[2023年12月22日]

1、修复``Entity``中``operator()``方法获取到的``ServiceTemplate``无法被调用的问题。

[2023年12月21日]

1、为``ControllerTemplate``中生成的``Select``方法查询参数，生成了更加细致的控制方法，为每一个参与查询的字段都生成了``Handle``方法，供给重载使用。

2、将``ControllerTemplate``中的``select``方法模板参数变更为``Map<String,Object>``，并将这个参数传入了``onHandleSelectBefor``中，可以让接口实现者更自由的控制请求参数和查询结果，原``select``方法业务不会因此而改变，生成的``SelectClass``会通过``Map``参数自动创建。

[2023年12月18日]

1、优化了生成的``Dynamic``数据结构。

[2023年12月15日]

1、将生成的``Controller``和``Service``模板全部移动到对应的目录下的二级目录``template``中。例如``controller/TestController.java``变更为``controller/template/TestController.java``。

2、优化了生成的``Dynamic``动态数据结构。

3、修复了一些问题。

4、将生成的``Dynamic``数据结构调整为``public``。

5、将生成的``Dynamic``数据结构从``Controller模板``移动到了``Service模板``中。

6、优化了生成的``Dynamic``数据结构，增加了一个轻量级获取绑定对象引用的方法。

[2023年12月6日]

1、在生成的``Entity``模板中增加了直接获取对应``ServiceTemplate``的方法，并且增加了``insert()、update()、save()``三个方法，增加了``operator()``方法来获取对应的数据库操作对象。

[2023年11月23日]

1、在生成的``Controller``模板中增加了``对应类型的Dynamic``类型，这个类型将作为``select``和``getEntity``的返回值类型，``Dynamic``能够将聚合的类型拆散为``Map``，方便修改返回值的字段，并且能够将拆散后的Map重新组装回``Company``。

[2023年11月22日]

1、修复了``save``模板方法无法保存更新的问题。

[2023年11月16日]

1、优化了提示信息，让报错信息更加明确，例如``找不到User的操作对象``将会根据``类中文名``的设置，将报错信息变更为``找不到用户的操作对象``。

[2023年11月15日]

1、整理了UI操作界面

2、控制器模板生成的所有接口都增加了``template``前缀，例如``save``变更为``template/save``。

3、将控制器模板生成的类名称增加了类名前缀，例如``OnlyId``在``User``类作用域中，变更为``User_OnlyId``。

[2023年11月13日]

1、在Controller的模板方法``remove``中增加了未实现时调用抛出异常业务。

[2023年11月9日]

1、在Entity中增加了``copy``方法。

2、在生成的``save``模板方法中，增加了``更新/修改数据的情况下，先将旧的数据查询出，后将新的数据字段通过copy方法修改至旧的数据中``业务。

3、对于``createWithSave``模板方法，同样增加了``#2``条业务。

4、增加了更加详细的错误信息。

[2023年11月1日]

1、在Entity模板中增加了静态方法``create``方法，这个方法接受一个``Object``类型参数，这个函数会创建一个Entity，并将参数对象中的``相同类型相同名称``的字段值复制给Entity对象。

2、在Service模板中的``Operate``对象中增加了``orderBy``相关的方法，提供了``只查询一条``的方法。

[2023年10月31日]

1、在Service模板中增加了``operator``方法和``Operator``类，这个类用于简化链式查询。

[2023年9月19日]

1、优化了一些功能。

[2023年9月14日]

1、在Service中增加了对page的支持。

[2023年9月8日]

1、修复了一些问题

[2023年9月5日]

1、在Controller中增加了“createWithSave”方法。

[2023年8月31日]

1、修复了一些问题

[2023年7月11日]

1、增加了针对每个字段进行修改的接口方法

[2023年6月20日]

1、修复了一些问题

[2023年6月13日]

1、修复了一些问题

[2023年6月6日]

1、在Service中提供了几个便捷查询到方法。

[2023年5月23日]

1、修复了一些问题。

[2023年5月11日]

1、修复了生成的文档字段错误问题

2、修复了无索引的情况下生成失败的问题

3、修复在没有select条件的情况下不生成select方法的问题

[2023年4月28日]

1、调整了界面

[2023年4月27日]

1、将英文的错误信息更换为中文的错误信息。

2、修复了save方法缺少字段的问题。

3、修改了“onHandleGetAfter”的参数名不正确问题。

[2023年4月25日]

1、完善了“打开”功能。

[2023年4月23日]

1、修复了建立索引时名称重复的问题

[2023年4月17日]

1、修复了Controller的service名称错误的问题。

2、修复了Service引入的mapper路径错误问题。