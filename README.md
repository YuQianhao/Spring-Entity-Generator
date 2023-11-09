# Spring Entity Generator

一个能让你挤出更多时间摸鱼的工具。

这个工具能够为你生成``数据库表``，``Controller``，``Service``，``Mapper``，自动创建``增删改查``方法接口，但是前提是需要依赖``MyBatis Plus``。

接口生成默认全部使用的``Post``。可以在``\generator``目录中找到对应的生成器，进行修改即可。

## 日志

[2023年11月9日]

1、在Entity中增加了``copy``方法。

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