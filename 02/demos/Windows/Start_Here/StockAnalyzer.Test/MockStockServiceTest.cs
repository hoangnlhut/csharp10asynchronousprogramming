using StockAnalyzer.Core.Services;

namespace StockAnalyzer.Test
{
    [TestClass]
    public class MockStockServiceTest
    {
        [TestMethod]
        public async Task Can_Load_All_MSFT_Stocks()
        {
            var service = new MockStockService();
            var stocks = await service.GetStockPricesFor("MSFT", CancellationToken.None);

            Assert.AreEqual(1, stocks.Count());
        }
    }
}