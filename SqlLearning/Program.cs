using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Data.SQLite;
using System.Data;
using Dapper;
using FreeSql.DataAnnotations;

namespace SqlLearning
{
    internal class Program
    {

        public class Dog
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public string Name { get; set; }

            public DateTime DataAdded { get; set; }
            public int Legs { get; set; } = 0;

            public static Dog Create(string name)
            {
                return new Dog()
                {
                    Name = name,
                    DataAdded = DateTime.Now,
                };
            }
        }

        
        public class Topic
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public int Clicks { get; set; }
            public string Title { get; set; }
            public DateTime CreateTime { get; set; }
        }

        static void Main(string[] args)
        {

            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=123.db")
                .UseAutoSyncStructure(true) //自动同步实体结构到数据库
                .Build(); //请务必定义成 Singleton 单例模式

            fsql.Update<Topic>(2)
              .Set(a => a.Title == "aaabbb")
              .Set(a => a.CreateTime == DateTime.Now)
              .ExecuteAffrows();

            //UPDATE `Topic` SET `Clicks` = ifnull(`Clicks`,0) + 1, `Time` = now() 
            //WHERE (`Id` = 1)

            fsql.Dispose();

            //var repo = fsql.GetRepository<Dog>(); repo.Update(dog);

            Console.ReadLine();
        }
    }

    public static class Database
    {
        public static readonly string _root = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
        public static readonly string _path = Path.Combine(_root, "123.db");

        public class TestObj
        {
            public TestObj(int id, int score, int age)
            {
                Id = id;
                Score = score;
                Age = age;
            }
            public int Id { get; set; }
            public int Score { get; set; }
            public int Age { get; set; }
        }

        public static void test()
        {
            SQLiteConnection cnn = new SQLiteConnection("data source=" + _path);


            //cnn.Open();
            //cnn.Close();

            //cnn.Execute("alter table t1 add column age int"); //添加列

            //cnn.Execute("update t1 set Age = 1000 where Id = 122"); //update table

            //cnn.Execute("drop table if exists t1"); //delete table

            cnn.Execute("create table t1(Id int,Score int,Age int)");

            var random = new Random();

            for (int i = 1; i < 20; i++)
            {
                cnn.Execute("insert into t1 values (@Id,@Score,@Age)", new TestObj(i, random.Next(1, 100), random.Next(1, 100))); //插入
            }
        }

        public static void LookUpTable()
        {
            SQLiteConnection cn = new SQLiteConnection("data source=" + _path);
            cn.Open();
            SQLiteCommand cmd = cn.CreateCommand();

            cmd.CommandText = "PRAGMA table_info('t1')";

            //写法二：用DataReader，这个效率高些
            SQLiteDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write($"{reader[i]},");
                }
                Console.WriteLine();
            }
            reader.Close();
        }
    }
}
