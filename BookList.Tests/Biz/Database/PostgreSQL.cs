﻿using System;
using NUnit.Framework;
using BookList.Biz.Database;

namespace BookList.Tests.Biz.Database
{
    [TestFixture]
    public class PostgreSQL
    {
        PostgreSQLConnection Db { get; set; }

        public PostgreSQL()
        {
            Db = new PostgreSQLConnection();

            // Create table "test" for unit tests
            CreateTestTable();
        }

        ~PostgreSQL()
        {
            // Once tests are complete drop table "test"
            Db.DropTable("test").Execute();
        }

        void CreateTestTable()
        {
            Db.CreateTable("test", Pairing.Of("name", "text")).Execute();

            Db.Insert("test", Pairing.Of("name", "Marla")).Execute();
            Db.Insert("test", Pairing.Of("name", "Susan")).Execute();
            Db.Insert("test", Pairing.Of("name", "John")).Execute();
            Db.Insert("test", Pairing.Of("name", "Jenna")).Execute();
            Db.Insert("test", Pairing.Of("name", "RJ")).Execute();
        }

        [Test]
        public void TestCreateTable()
        {
            Db.CreateTable("newtable", Pairing.Of("name", "text"), Pairing.Of("number", "int")).Execute();

            var table = Db.Take("information_schema.tables").Where(Pairing.Of("table_name", "newtable")).Execute();
            var columns = Db.Take("information_schema.columns").Where(Pairing.Of("table_name", "newtable")).Execute()[3];

            Assert.AreEqual(1, table[0].Count);
            Assert.AreEqual(3, columns.Count);

            Db.DropTable("newtable").Execute();
        }

        [Test]
        public void TestDropTable()
        {
            Db.CreateTable("newtable", Pairing.Of("name", "text"), Pairing.Of("number", "int")).Execute();
            Db.DropTable("newtable").Execute();

            var table = Db.Take("information_schema.tables").Where(Pairing.Of("table_name", "newtable")).Execute();

            Assert.AreEqual(0, table[0].Count);
        }

        [Test]
        public void TestTake()
        {
            var results = Db.Take("test").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Marla", results[1][0]);
        }

        [Test]
        public void TestWhere()
        {
            var results = Db.Take("test").Where(Pairing.Of("name", "Susan"), Pairing.Of("id", 2)).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);
        }

        [Test]
        public void TestOrderBy()
        {
            // Test default orderby
            var results = Db.Take("test").OrderBy("name").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(5, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);

            // Test orderby ascending
            results = Db.Take("test").OrderBy("name", "asc").Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(5, results[0].Count);
            Assert.AreEqual("Jenna", results[1][0]);
        }

        [Test]
        public void TestLimit()
        {
            var results = Db.Take("test").OrderBy("name").Limit(1).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual(1, results[0].Count);
            Assert.AreEqual("Susan", results[1][0]);
        }

        [Test]
        public void TestInsert()
        {
            Db.Insert("test", Pairing.Of("name", "Graydon")).Execute();

            var results = Db.Take("test").Where(Pairing.Of("name", "Graydon")).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Graydon", results[1][0]);
            Assert.AreEqual(1, results[0].Count);

            Db.Delete("test").Where(Pairing.Of("name", "Graydon")).Execute();
        }

        [Test]
        public void TestUpdate()
        {
            Db.Insert("test", Pairing.Of("name", "Graydon")).Execute();
            Db.Update("test", Pairing.Of("name", "Graydon Update")).Where(Pairing.Of("name", "Graydon")).Execute();

            var results = Db.Take("test").Where(Pairing.Of("name", "Graydon Update")).Execute();

            Assert.IsNotNull(results);
            Assert.AreEqual("Graydon Update", results[1][0]);
            Assert.AreEqual(1, results[0].Count);

            Db.Delete("test").Where(Pairing.Of("name", "Graydon Update")).Execute();
        }

        [Test]
        public void TestDelete()
        {
            Db.Insert("test", Pairing.Of("name", "Graydon")).Execute();
            Db.Delete("test").Where(Pairing.Of("name", "Graydon")).Execute();

            var results = Db.Take("test").Where(Pairing.Of("name", "Graydon")).Execute();

            Assert.AreEqual(0, results[0].Count);
        }

        [Test]
        public void TestError()
        {
            Assert.Throws<Exception>(() => { Db.Limit(1).Execute(); }, "Something went wrong with your sql statement, please try again.");
        }
    }
}
