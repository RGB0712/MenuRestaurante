using Amazon.DynamoDBv2.DataModel;
using MenuRestaurante_RaulGonzalez.Models;
using Microsoft.AspNetCore.Mvc;

namespace MenuRestaurante_RaulGonzalez.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDynamoDBContext _context;
        private readonly IConfiguration _config;

        public HomeController(IDynamoDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<IActionResult> Index()
        {
            var featuredItems = await _context
                .ScanAsync<MenuItem>(new List<ScanCondition>
                {
                new ScanCondition("IsFeatured", Amazon.DynamoDBv2.DocumentModel.ScanOperator.Equal, true)
                }).GetRemainingAsync();

            ViewBag.BucketUrl = $"https://{_config["AWS:S3BucketName"]}.s3.amazonaws.com";
            return View(featuredItems);
        }
    }
}
