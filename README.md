# Spring Entity Generator

一个能让你挤出更多时间摸鱼的工具。

这个工具能够为你生成``数据库表``，``Controller``，``Service``，``Mapper``，自动创建``增删改查``方法接口，但是前提是需要依赖``MyBatis Plus``。

接口生成默认全部使用的``Post``。可以在``\generator``目录中找到对应的生成器，进行修改即可。

## 日志

[2023年4月25日]

1、完善了“打开”功能。

[2023年4月23日]

1、修复了建立索引时名称重复的问题

[2023年4月17日]

1、修复了Controller的service名称错误的问题。

2、修复了Service引入的mapper路径错误问题。