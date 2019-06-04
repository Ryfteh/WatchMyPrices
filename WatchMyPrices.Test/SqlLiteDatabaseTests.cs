namespace WatchMyPrices.Test
{
    using System;
    using System.Data;
    using NSubstitute;
    using NUnit.Framework;
    using WatchMyPrices.New;

    [TestFixture]
    public class SqlLiteDatabaseTests
    {
        [Test]
        public void GetPrice_ProductIsEmptyOrNull_ThrowsError()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLiteDatabase(null).GetPrice(null, "x", DateTime.Today));
        }

        [Test]
        public void GetPrice_SiteIsEmptyOrNull_ThrowsError()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlLiteDatabase(null).GetPrice("x", null, DateTime.Today));
        }

        [Test]
        public void GetPrice_PriceExists_ReturnsPrice()
        {
            var price = 3.50M;
            var dateTime = Math.Round((DateTime.Today.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);

            var dataReader = Substitute.For<IDataReader>();
            dataReader.Read().Returns(true);
            dataReader.GetInt32(0).Returns(0);
            dataReader.GetString(1).Returns("Product1");
            dataReader.GetString(2).Returns("Site1");
            dataReader.GetString(3).Returns("http://www.example.com/product1.html");
            dataReader.GetString(4).Returns(@"\\*\span[1]");
            dataReader.GetDecimal(5).Returns(price);
            dataReader.GetDouble(6).Returns(0);
            var dbCommand = Substitute.For<IDbCommand>();
            dbCommand.ExecuteReader().Returns(dataReader);
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.CreateCommand().Returns(dbCommand);

            var sqlLiteDatabase = new SqlLiteDatabase(dbConnection);
            Assert.That(price == sqlLiteDatabase.GetPrice("Product1", "Site1", DateTime.Today).Price);
        }

        [Test]
        public void GetPrice_PriceNotExists_NotReturnsPrice()
        {
            var dataReader = Substitute.For<IDataReader>();
            dataReader.Read().Returns(false);
            var dbCommand = Substitute.For<IDbCommand>();
            dbCommand.ExecuteReader().Returns(dataReader);
            var dbConnection = Substitute.For<IDbConnection>();
            dbConnection.CreateCommand().Returns(dbCommand);

            var sqlLiteDatabase = new SqlLiteDatabase(dbConnection);

            Assert.That(sqlLiteDatabase.GetPrice("Product1", "Site1", DateTime.Today) == null);
        }
    }
}
