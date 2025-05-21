using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using MenuRestaurante_RaulGonzalez.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MenuRestaurante_RaulGonzalez.Controllers
{
    public class MenuController : Controller
    {
        private readonly IDynamoDBContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _imagePrefix = "menu/";

        public MenuController(IDynamoDBContext context, IAmazonS3 s3Client, IConfiguration config)
        {
            _context = context;
            _s3Client = s3Client;
            _bucketName = config["AWS:S3BucketName"] ?? throw new ArgumentNullException("Missing S3 bucket config");
        }

        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.ScanAsync<MenuItem>(new List<ScanCondition>()).GetRemainingAsync();
            return View(menuItems);
        }

        public async Task<IActionResult> Create()
        {
            var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
            ViewBag.ImageOptions = new SelectList(imageKeys);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    menuItem.ItemId = Guid.NewGuid().ToString();
                    await _context.SaveAsync(menuItem);
                    return RedirectToAction(nameof(Index));
                }

                var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
                ViewBag.ImageOptions = new SelectList(imageKeys);
                return View(menuItem);
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var item = await _context.LoadAsync<MenuItem>(id);
            if (item == null) return NotFound();

            var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
            ViewBag.ImageOptions = new SelectList(imageKeys, item.ImageKey);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, MenuItem updatedItem)
        {
            if (id != updatedItem.ItemId) return BadRequest();

            if (ModelState.IsValid)
            {
                await _context.SaveAsync(updatedItem);
                return RedirectToAction(nameof(Index));
            }

            var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
            ViewBag.ImageOptions = new SelectList(imageKeys, updatedItem.ImageKey);
            return View(updatedItem);
        }

        public async Task<IActionResult> Delete(string id)
        {
            var item = await _context.LoadAsync<MenuItem>(id);
            if (item == null) return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                Console.WriteLine(id);
                await _context.DeleteAsync<MenuItem>(id);
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View("Error", ex.Message);
            }
        }

        private async Task<List<string>> GetImageKeysFromS3Async(string prefix)
        {
            var keys = new List<string>();
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            foreach (var obj in response.S3Objects)
            {
                keys.Add(obj.Key);
            }

            return keys;
        }
    }
}
