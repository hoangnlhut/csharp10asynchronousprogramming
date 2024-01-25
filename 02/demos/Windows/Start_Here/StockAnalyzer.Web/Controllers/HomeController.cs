using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StockAnalyzer.Core.Domain;
using StockAnalyzer.Web.Models;
using System.Diagnostics;

namespace StockAnalyzer.Web.Controllers;

public class HomeController : Controller
{
    private static string API_URL = "https://ps-async.fekberg.com/api/stocks";

    public async Task<IActionResult> IndexAsync()
    {
        IEnumerable<StockPrice> data;
        //add more because there is no block code in the beginning
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync($"{API_URL}/MSFT");

            var content = await response.Content.ReadAsStringAsync();

             data = JsonConvert.DeserializeObject<IEnumerable<StockPrice>>(content);
        }

        return View(data);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}