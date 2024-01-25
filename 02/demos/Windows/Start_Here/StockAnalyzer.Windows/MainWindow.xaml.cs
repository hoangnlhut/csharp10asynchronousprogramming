using Newtonsoft.Json;
using StockAnalyzer.Core;
using StockAnalyzer.Core.Domain;
using StockAnalyzer.Core.Services;
using StockAnalyzer.Windows.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace StockAnalyzer.Windows;

public partial class MainWindow : Window
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";
    private Stopwatch stopwatch = new Stopwatch();
    CancellationTokenSource? cancellationTokenSource;

    public MainWindow()
    {
        InitializeComponent();
    }

    // new Function for using Asynchronous Streams and Disposables
    private async void Search_ClickNew(object sender, RoutedEventArgs e)
    {
        if (cancellationTokenSource is not null)
        {
            // already have an instance of the cancellation token source?
            // This means the button has already been pressed!
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            Search.Content = "Search";
            return;
        }

        Notes.Text = string.Empty;
        Search.Content = "Cancel";
        try
        {
            cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Token.Register(() =>
            {
                Notes.Text = "Cancelation requested";
            });

            BeforeLoadingStockData();

            var identifiers = StockIdentifier.Text.Split(' ', ',');

            var data = new ObservableCollection<StockPrice>();

            Stocks.ItemsSource = data;

            //mock service
            //var service = new MockStockStreamService();

            //stock disk service
            var service = new StockDiskService();

            var enumerator = service.GetAllStockPrices(cancellationTokenSource.Token);

            await foreach (var price in enumerator.WithCancellation(cancellationTokenSource.Token))
            {
                if (identifiers.Contains(price.Identifier))
                {
                    data.Add(price);
                }
            }
        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }
        finally
        {
            AfterLoadingStockData();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            Search.Content = "Search";
        }
    }
    private async void Search_ClickOriginal(object sender, RoutedEventArgs e)
    {
        if (cancellationTokenSource is not null)
        {
            // already have an instance of the cancellation token source?
            // This means the button has already been pressed!
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            Search.Content = "Search";
            return;
        }

        Notes.Text = string.Empty;
        #region try to continue after an execution 
        // NOTE : using TaskContinuationOptions.OnlyOnFaulted when have fault and using TaskContinuationOptions.OnlyOnRanToCompletion when it has complete. 
        //var loadLineTask = Task.Run(() => 
        //{
        //    Console.WriteLine("dfsfdf");
        //}, TaskContinuationOptions.OnlyOnRanToCompletion);

        // loadLineTask.ContinueWith((completeTask) =>
        //{
        //    Dispatcher.Invoke(() =>
        //    {
        //        Notes.Text = completeTask.Exception?.InnerException?.Message;
        //    });
        //}, TaskContinuationOptions.OnlyOnFaulted)
        //    .ContinueWith(t =>
        //{
        //    Console.WriteLine("sdfsdfdfsdfsdfs");
        //}) ;


        #endregion

        #region Nested Asynchronous Operations
        //try {
        //    BeforeLoadingStockData();
        //    // using async await in task
        //    var loadLineTask = Task.Run(async () =>
        //    {
        //        using var stream = new StreamReader(File.OpenRead("StockPrices_Small.csv"));

        //        var lines = new List<string>();

        //        while(await stream.ReadLineAsync() is string line)
        //        {
        //            lines.Add(line);
        //        }
        //        return lines;
        //    });

        //    var processStocksTask = loadLineTask.ContinueWith(task =>
        //    {
        //        var lines = task.Result;

        //        var data = new List<StockPrice>();

        //        foreach (var line in lines.Skip(1))
        //        {
        //            var price = StockPrice.FromCSV(line);
        //            data.Add(price);
        //        }
        //        Dispatcher.Invoke(() =>
        //        {
        //            Stocks.ItemsSource = data.Where(x => x.Identifier == StockIdentifier.Text).ToList();
        //        });
        //    }).ContinueWith(_ =>
        //    {
        //        //must place it in Dispatcher invoke to queue work on the UI thread
        //        Dispatcher.Invoke(() =>
        //        {
        //            AfterLoadingStockData();
        //        });
        //    });
        //}
        //catch (System.Exception ex)
        //{

        //    Notes.Text = ex.Message;
        //}

        #endregion

        #region Using task and handle to get the total time get the data
        try
        {
            cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Token.Register(() =>
            {
                Notes.Text = "Cancelation requested";
            });

            Search.Content = "Cancel";

            BeforeLoadingStockData();
            //var token = new CancellationToken( true);
            //var loadLinesTask = SearchForStocks(token);

            #region process to display data to UI by calling StockService

            // add the case that user can input many text to textbox search for multiple results

            var identifiers = StockIdentifier.Text.Split(',', ' ');

            var service = new StockService();
            //var service = new MockStockService();

            #region using task.WhenAll: when ALL task marked completed so it will create new task 
            //var loadingData = new List<Task<IEnumerable<StockPrice>>>();

            //foreach (var identifier in identifiers)
            //{
            //    var loadData = service.GetStockPricesFor(identifier, cancellationTokenSource.Token);
            //    loadingData.Add(loadData);
            //}

            //var data = await Task.WhenAll(loadingData);
            //Stocks.ItemsSource = data.SelectMany(x => x);
            #endregion

            #region using Task.WhenAny: one of tasks in the list is marked completed
            //var timeout = Task.Delay(5000);

            var loadingData = new List<Task<IEnumerable<StockPrice>>>();
            var stocks = new ConcurrentBag<StockPrice>();
            foreach (var identifier in identifiers)
            {
                var loadTask = service.GetStockPricesFor(identifier, cancellationTokenSource.Token);

                loadTask = loadTask.ContinueWith(t =>
                {
                    var aFewStocks = t.Result;

                    foreach (var stock in aFewStocks)
                    {
                        stocks.Add(stock);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        Stocks.ItemsSource = stocks.ToArray();
                    });


                    return aFewStocks;
                }, cancellationTokenSource.Token);

                loadingData.Add(loadTask);
            }
            await Task.WhenAll(loadingData);
            //var allStockLoadingTask = Task.WhenAll(loadingData);

            //var completedTask = await Task.WhenAny(timeout, allStockLoadingTask);
            ////using Task.WhenAny
            //if (completedTask == timeout)
            //{
            //    cancellationTokenSource.Cancel();
            //    throw new OperationCanceledException("Timeout!");
            //}

            //no need anymore when we use ConcurrentBag
            //Stocks.ItemsSource = allStockLoadingTask.Result.SelectMany(x => x);

            #endregion


            #endregion

            #region proces to display data to UI in Task way
            //var loadLinesTask = SearchForStocks(cancellationTokenSource.Token);

            //var processStocksTask = loadLinesTask.ContinueWith((completedTask) =>
            //{
            //    //we can use result in this case becaue the task was completed . In normal way, if we you this property Result , it will block until the task was completed
            //    var lines = completedTask.Result;

            //    var data = new List<StockPrice>();

            //    //Skip first row due to it's header
            //    foreach (var line in lines.Skip(1)) 
            //    {
            //        var price = StockPrice.FromCSV(line);
            //        data.Add(price);
            //    }

            //    //Queued work on the UI thread
            //    Dispatcher.Invoke(() =>
            //    {
            //        Stocks.ItemsSource = data.Where(x => x.Identifier == StockIdentifier.Text).ToList();
            //    });

            //});


            //processStocksTask.ContinueWith(_ =>
            //{
            //    //must place it in Dispatcher invoke to queue work on the UI thread
            //    Dispatcher.Invoke(() =>
            //    {
            //        AfterLoadingStockData();

            //        cancellationTokenSource?.Dispose();
            //        cancellationTokenSource = null;

            //        Search.Content = "Search";
            //    });
            //});
            #endregion
        }
        catch (System.Exception ex)
        {

            Notes.Text = ex.Message;
        }
        finally
        {
            //AfterLoadingStockData(); only using in normal way with TAsk

            //  have to use below if you use async await
            AfterLoadingStockData();

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;

            Search.Content = "Search";
        }
        #endregion

        #region Use Task - not using async await to represent single asynchronous operation but this way we can get the total time we got the data
        //Task.Run(() =>
        //{
        //    var lines = File.ReadAllLines("StockPrices_Small.csv");
        //    var data = new List<StockPrice>();

        //    //Skip first row due to it's header
        //    foreach (var line in lines.Skip(1))
        //    {
        //        var price = StockPrice.FromCSV(line);
        //        data.Add(price);
        //    }

        //    //Queued work on the UI thread
        //    Dispatcher.Invoke(() =>
        //    {
        //        Stocks.ItemsSource = data.Where(x => x.Identifier == StockIdentifier.Text).ToList();
        //    });
        //});
        #endregion
        #region synchronous way
        //BeforeLoadingStockData();

        //var client = new WebClient();

        //var content = client.DownloadString($"{API_URL}/{StockIdentifier.Text}");

        //// Simulate that the web call takes a very long time
        //Thread.Sleep(10000);

        //var data = JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);

        //Stocks.ItemsSource = data;

        //AfterLoadingStockData();
        #endregion

        #region New Way to use HTTP client async await
        //
        //using (var client = new HttpClient())
        //{
        //    var response = await client.GetAsync($"{API_URL}/{StockIdentifier.Text}");

        //    var content = await response.Content.ReadAsStringAsync();

        //    var data = JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);

        //    Stocks.ItemsSource = data;

        //    AfterLoadingStockData();
        //}
        #endregion

        #region use helper from class DataStore
        //BeforeLoadingStockData();
        //var store = new DataStore();
        //var repsonseTask = store.GetStockPrices(StockIdentifier.Text);

        //var data = await repsonseTask;

        //Stocks.ItemsSource = data;
        //AfterLoadingStockData();
        #endregion
    }

    //Search click for using IProgress for report  on the progress of a task
    private async void Search_ClickIProgress(object sender, RoutedEventArgs e)
    {
        try
        {
            Notes.Text = string.Empty;
            BeforeLoadingStockDataProgress();
            var progress = new Progress<IEnumerable<StockPrice>>();
            progress.ProgressChanged += Progress_ProgressChanged;

            //way to handle update
            //Note about when using IProgress , try to figure out a way to pass PROGRESS into asynchronous method
            await SearchForStocksWithProgress(progress);
        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }
        finally
        {
            AfterLoadingStockData();
        }
    }

    //Search click for using TaskCompletionSource using threadPool
    private async void Search_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            BeforeLoadingStockData();

            var data = await SearchForStocksTaskCompletion();

            Stocks.ItemsSource = data.Where(price => price.Identifier == StockIdentifier.Text);
        }
        catch (Exception ex)
        {
            Notes.Text = ex.Message;
        }
        finally
        {
            AfterLoadingStockData();
        }
    }

    private  Task<IEnumerable<StockPrice>> SearchForStocksTaskCompletion()
    {
        //When we queue the work onto the ThreadPool, we can't await that. We need to introduce the TaskCompletionSource to help us with this
        var tcs = new TaskCompletionSource<IEnumerable<StockPrice>>();

        //the file is being loaded using the non-asynchronous methods. BUT doing that on a separate thread because
        // we queued the work on ThreadPool. 
        ThreadPool.QueueUserWorkItem(_ =>
        {
            var lines = File.ReadAllLines("StockPrices_Small.csv");
            var prices = new List<StockPrice>();

            foreach (var line in lines.Skip(1))
            {
                prices.Add(StockPrice.FromCSV(line));
            }
            //TODO: commonicate the result of 'prices' ?
            tcs.SetResult(prices);
        });

        //TODO: return a Task<IEnumerable<StockPrice>> ?
        //represent asynchronous operations can be AWAIT somewhere else.
        return tcs.Task;
    }

    private async Task SearchForStocksWithProgress(IProgress<IEnumerable<StockPrice>> progress)
    {
        var service = new StockService();
        var loadingTasks = new List<Task<IEnumerable<StockPrice>>>();

        foreach (var identifier in StockIdentifier.Text.Split(' ', ','))
        {
            var loadTask = service.GetStockPricesFor(identifier, CancellationToken.None);

            loadTask = loadTask.ContinueWith(completedTask =>
            {
                //report the progress
                progress?.Report(completedTask.Result);

                return completedTask.Result;
            });

            loadingTasks.Add(loadTask);
        }

        var data = await Task.WhenAll(loadingTasks);
        Stocks.ItemsSource = data.SelectMany(x => x);
    }

    private void Progress_ProgressChanged(object? sender, IEnumerable<StockPrice> stocks)
    {
        StockProgress.Value += 1;
        Notes.Text += $"Loaded {stocks.Count()} for {stocks.First().Identifier}{Environment.NewLine}";
    }

    private static Task<List<string>> SearchForStocks(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            using var stream = new StreamReader(File.OpenRead("StockPrices_SmallHoang.csv"));
            var lines = new List<string>();

            while (await stream.ReadLineAsync() is string line)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                lines.Add(line);
            }

            return lines;
        }, cancellationToken);
    }

    private void BeforeLoadingStockDataProgress()
    {
        stopwatch.Restart();
        StockProgress.Visibility = Visibility.Visible;
        StockProgress.IsIndeterminate = false;
        //add some code to report on the progress of a task
        StockProgress.Value = 0;
        StockProgress.Maximum = StockIdentifier.Text.Split(' ', ',').Length;
    }

    private void BeforeLoadingStockData()
    {
        stopwatch.Restart();
        StockProgress.Visibility = Visibility.Visible;
        StockProgress.IsIndeterminate = true;
        //add some code to report on the progress of a task
    }

    private void AfterLoadingStockData()
    {
        StocksStatus.Text = $"Loaded stocks for {StockIdentifier.Text} in {stopwatch.ElapsedMilliseconds}ms";
        StockProgress.Visibility = Visibility.Hidden;
    }

    private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });

        e.Handled = true;
    }

    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }
}