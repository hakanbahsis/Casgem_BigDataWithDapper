using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Collections.Immutable;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using WebUI.Models;
using WebUI.Models.Dto;

namespace WebUI.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString = "Server = DESKTOP-PK98KLS; initial catalog = CARPLATES; integrated security = true";

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            await using var connection = new SqlConnection(_connectionString);

            Stopwatch stopwatchBrandMax = new Stopwatch();
            stopwatchBrandMax.Start();
            var brandMax = (await connection.QueryAsync<BrandDto>("SELECT TOP 1 BRAND, COUNT(*) AS COUNT_ FROM PLATES GROUP BY BRAND ORDER BY COUNT_ DESC")).FirstOrDefault();
            stopwatchBrandMax.Stop();
            ViewBag.Stopwatch = stopwatchBrandMax.Elapsed;
            ViewBag.BrandMax = brandMax.BRAND;
            ViewBag.BrandCount = brandMax.COUNT_;

            Stopwatch stopwatchBrandWithCount = new Stopwatch();
            stopwatchBrandWithCount.Start();
            var brandMaxWithCount = (await connection.QueryAsync<BrandDto>("select BRAND,COUNT(*) as COUNT_,ROUND(convert(float,count(*))/(select count(*) from Plates)*100,2) as PERCENT_\r\nfrom PLATES group by BRAND order by 2 desc")).FirstOrDefault();
            stopwatchBrandWithCount.Stop();
            ViewBag.StopwatchMaxWithCount= stopwatchBrandWithCount.Elapsed;
            ViewBag.Brand = brandMaxWithCount.BRAND;
            ViewBag.BrandMaxCount= brandMaxWithCount.COUNT_;
            ViewBag.PERCENT= brandMaxWithCount.PERCENT_;

            Stopwatch stopwatchCarCountForCıty=new Stopwatch();
            stopwatchCarCountForCıty.Start();
            var carCountForCity=(await connection.QueryAsync<BrandDto>("SELECT YEAR_, COUNT(*) AS CAR_COUNT FROM PLATES GROUP BY YEAR_ order by YEAR_ desc")).ToList();
            stopwatchCarCountForCıty.Stop();
            
            ViewBag.CarCountCity = stopwatchCarCountForCıty.Elapsed;
            ViewBag.List=carCountForCity;

            Stopwatch stopWatchFuelCount = new Stopwatch();
            stopWatchFuelCount.Start();
            var fuelType = (await connection.QueryAsync<BrandDto>("SELECT TOP 1 FUEL, COUNT(*) AS count FROM PLATES GROUP BY FUEL ORDER BY count DESC")).FirstOrDefault();
            stopWatchFuelCount.Stop();
            ViewBag.Count = stopWatchFuelCount.Elapsed;



            //yıllara göre araç sayıları
            // var brandMaxWithCount = (await connection.QueryAsync<BrandDto>("SELECT YEAR_, COUNT(*) AS CAR_COUNT FROM PLATES GROUP BY YEAR_ order by YEAR_ desc")).FirstOrDefault();

            //Kırmızı araç sayısı
            //SELECT COUNT(*) AS RED_CAR_COUNT FROM PLATES WHERE COLOR = 'Kırmızı';


            //73 plakalı araç sayısı
            //SELECT count( *) FROM PLATES WHERE PLATE LIKE '73%';

            return View();
        }

        public async Task<IActionResult> Index2(string searchString)
        {
           

            return View();
        }


        [HttpGet]
        public async Task<IActionResult> Search(string searchString)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var allCars = await SearchCarsAsync(searchString);
            stopwatch.Stop();

            var json = JsonConvert.SerializeObject(allCars);

            return Json(new { queryTime = stopwatch.Elapsed, data = json });
        }


        public async Task<IEnumerable<ResultDto>> SearchCarsAsync(string searchString)
        {
            await using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var query =
                $"SELECT * FROM PLATES WHERE BRAND LIKE @search OR PLATE LIKE @search";
            return await connection.QueryAsync<ResultDto>(query, new { search = $"%{searchString}%" });
        }







    }
}