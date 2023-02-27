# QiuKitORM
一个基于.net和Sql Server的简易ORM框架
( 支持.net framework 和.net core )

# 使用方法：
1、创建sql server数据库和对应的数据表后，创建对应的实体类(使得数据库字段和实体类属性名完全一致)，并且对主键、自增序列做标注。

    [QiuKitModel(IsPrimKey = false, IsField = true, IsIdentity = false)]
    public string field {get; set;}     

2、在项目中引用本框架。仓储层(数据访问层)的类，都继承本框架的BaseDAL<T>这个类，其中泛型T的类型为数据表对应的实体类类型。

    public class TestDAL:BaseDAL<TestModel>
    {
       //可以自定义一些特定业务的方法
    }
    
3、继承BaseDAL类后，建议在子类中定义常量声明该类的数据表表名，以便后续方便的调用类中的方法。

    public class TestDAL:BaseDAL<TestModel>
    {
       public const string table = "T_Test";
    }
            
4、初始化类中的connStr字段为对应的连接字符串。

    string connStr = "" ;   //connStr可自行定义
    TestDAL.Instance.connStr = connStr;

5、直接调用类的实例(类中已声明为Instance)，然后调用对应的增(Insert)删(Delete)改(Update)查(Select)方法。
    
    List<TestModel> selectResult = TestDAL.Instance.Select(TestDAL.table,"");  //查询表中所有结果并返回实体类List集合
