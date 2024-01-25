using StockAnalyzer.Core.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.Windows.Services
{
    public interface IStockStreamSerivce
    {
        IAsyncEnumerable<StockPrice> GetAllStockPrices(CancellationToken cancellationToken = default);
    }


    public class MockStockStreamService : IStockStreamSerivce
    {
        public async IAsyncEnumerable<StockPrice> GetAllStockPrices([EnumeratorCancellation]CancellationToken cancellationToken = default)
        {
            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "MSFT", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "MSFT", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "MSFT", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "MSFT", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "MSFT", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "GOOGL", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "GOOGL", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "GOOGL", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "GOOGL", Change = 0.5m, ChangePercent = 75 };

            await Task.Delay(500, cancellationToken);

            yield return new StockPrice() { Identifier = "GOOGL", Change = 0.5m, ChangePercent = 75 };
        }
    }

    public class StockDiskService : IStockStreamSerivce
    {
        public async IAsyncEnumerable<StockPrice> GetAllStockPrices(CancellationToken cancellationToken = default)
        {
            using var stream = new StreamReader(File.OpenRead("StockPrices_Small.csv"));

            await stream.ReadLineAsync(); // skip header row in the file

            while(await stream.ReadLineAsync() is string line)
            {
                if(cancellationToken.IsCancellationRequested)
                { 
                    break; 
                }
                //for testing cancel
                //await Task.Delay(10);

                yield return StockPrice.FromCSV(line);
            }
        }
    }

}
