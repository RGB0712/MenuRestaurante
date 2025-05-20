using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Amazon.DynamoDBv2.DataModel;
using MenuRestaurante_RaulGonzalez.Models;
using Microsoft.AspNetCore.Authorization;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace MenuRestaurante_RaulGonzalez.Controllers
{
    public class MenuController : Controller
    {

        // Variables privadas
        private readonly IDynamoDBContext _context;
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName = "restaurant-menu-images"; // Nombre del Bucket
        private readonly string _imagePrefix = "menu/"; // Carpeta del bucket

        public MenuController(IDynamoDBContext context,IAmazonS3 s3Client)
        {
            _context = context;
            _s3Client = s3Client;
        }

        // ================================
        // PUBLIC VIEW
        // GET: /Menu
        // ================================
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var menuItems = await _context.ScanAsync<MenuItem>(new List<ScanCondition>()).GetRemainingAsync();
            return View(menuItems);
        }

        // ================================
        // CREATE
        // GET: /Menu/Create
        // ================================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create() 
        {
            var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
            ViewBag.ImageOptions = new SelectList(imageKeys);
            return View(); 
        }

        // ================================
        // CREATE
        // POST: /Menu/Create
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
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
                
                // Volver a cargar imágenes si el modelo es inválido
                var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
                ViewBag.ImageOptions = new SelectList(imageKeys);
                return View(menuItem);

            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        // ================================
        // EDIT
        // GET: /Menu/Edit/{id}
        // ================================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string id)
        {
            var item = await _context.LoadAsync<MenuItem>(id);
            if (item == null) return NotFound();

            var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
            ViewBag.ImageOptions = new SelectList(imageKeys, item.ImageKey); // ← valor preseleccionado

            return View(item);
        }

        // ================================
        // EDIT
        // POST: /Menu/Edit/{id}
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Edit(string id, MenuItem updatedItem)
        {
            try
            {
                if (id != updatedItem.ItemId) return BadRequest();

                if (ModelState.IsValid)
                {
                    await _context.SaveAsync(updatedItem);
                    return RedirectToAction(nameof(Index));
                }

                // Si algo falla, volver a cargar las imágenes para el select
                var imageKeys = await GetImageKeysFromS3Async(_imagePrefix);
                ViewBag.ImageOptions = new SelectList(imageKeys, updatedItem.ImageKey);

                return View(updatedItem);
            
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }

        }

        // ================================
        // DELETE
        // GET: /Menu/Delete/{id}
        // ================================
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(string id)
        {
            var item = await _context.LoadAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }

        // ================================
        // DELETE
        // POST: /Menu/Delete/{id}
        // ================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _context.DeleteAsync<MenuItem>(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        // ================================
        // HELPER: Get image keys from S3
        // ================================
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
