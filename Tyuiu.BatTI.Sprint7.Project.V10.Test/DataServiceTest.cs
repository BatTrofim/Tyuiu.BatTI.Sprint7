using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using Tyuiu.BatTI.Sprint7.Project.V10.Lib;

namespace Tyuiu.BatTI.Sprint7.Project.V10.Test
{
    [TestClass]
    public class DataServiceTest
    {
        [TestMethod]
        public void ValidCalcTotalStockValue()
        {
            DataService ds = new DataService();

            List<ItemModel> testData = new List<ItemModel>
            {
                new ItemModel
                {
                    Name = "Товар1",
                    Price = 100,
                    Quantity = 5
                },

                new ItemModel
                {
                    Name = "Товар2",
                    Price = 50,
                    Quantity = 2
                }
            };

            decimal res = ds.GetTotalStockValue(testData);

            Assert.AreEqual(600, res);
        }
    }
}