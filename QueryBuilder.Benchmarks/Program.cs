using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SqlKata;
using SqlKata.Compilers;

MySqlCompiler _compiler = new();

_compiler.Compile(
    new Query()
    .From("Table1 AS t1")
    .Join("Table2 AS t2", "t1.Id", "t2.Id")
    .Join("Table3 AS t3", "t2.Id", "t2.Id")
    .Join("Table4 AS t4", "t3.Id", "t2.Id")
    .Join("Table5 AS t5", "t4.Id", "t2.Id")
    .Join("Table6 AS t6", "t5.Id", "t2.Id")
    .Where("Campo", "=", "123")
    .Select("Field1", "Field2", "Field3"));

//BenchmarkRunner.Run<Bench>();

//[MemoryDiagnoser]
//public class Bench
//{
//    private static readonly MySqlCompiler _compiler = new();

//    [Benchmark]
//    public SqlResult SimpleQuery()
//        => _compiler.Compile(
//            new Query()
//            .From("Teste")
//            .Where("Campo", "=", new UnsafeLiteral("'test'", false))
//            .Select("Field1", "Field2", "Field3"));

//    [Benchmark]
//    public SqlResult ComplexQuery()
//        => _compiler.Compile(
//            new Query()
//            .From("Table1 AS t1")
//            .Join("Table2 AS t2", "t1.Id", "t2.Id")
//            .Join("Table3 AS t3", "t2.Id", "t2.Id")
//            .Join("Table4 AS t4", "t3.Id", "t2.Id")
//            .Join("Table5 AS t5", "t4.Id", "t2.Id")
//            .Join("Table6 AS t6", "t5.Id", "t2.Id")
//            .Where("Campo", "=", new UnsafeLiteral("'test'", false))
//            .Select("Field1", "Field2", "Field3"));
//}
