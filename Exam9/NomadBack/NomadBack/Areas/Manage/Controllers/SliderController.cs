using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NomadBack.DAL;
using NomadBack.Models;
using NomadBack.Utilities.Extention;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NomadBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class SliderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            return View(_context.Sliders.ToList());
        }
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Slider slider)
        {

            if (slider.Photo.CheckSize(800))
            {
                ModelState.AddModelError("Photo", "File size must be less than 800kb");
                return View();
            }
            if (!slider.Photo.CheckType("image/"))
            {
                ModelState.AddModelError("Photo", "File must be image");
                return View();
            }
            slider.Image = await slider.Photo.SaveFileAsync(Path.Combine(_env.WebRootPath, "Images"));
            await _context.Sliders.AddAsync(slider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            Slider slider = _context.Sliders.FirstOrDefault(c => c.Id == id);
            if (slider == null) return NotFound();
            return View(slider);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Slider slider)
        {
            if (ModelState.IsValid)
            {
                var s = await _context.Sliders.FindAsync(slider.Id);
                s.Name = slider.Name;
                s.Founder = slider.Founder;
                if (slider.Photo != null)
                {
                    if (slider.Image != null)
                    {
                        string filePath = Path.Combine(_env.WebRootPath, "Images", slider.Image);
                        System.IO.File.Delete(filePath);
                    }
                    s.Image = ProcessUploadedFile(slider);
                }
                _context.Update(s);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }
        private string ProcessUploadedFile(Slider slider)
        {
            string uniqueFileName = null;

            if (slider.Photo != null)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + slider.Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    slider.Photo.CopyTo(fileStream);
                }
            }

            return uniqueFileName;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Slider slider = _context.Sliders.Find(id);
            if (slider == null) return NotFound();
            if (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Images", slider.Image)))
            {
                System.IO.File.Delete(Path.Combine(_env.WebRootPath, "Images", slider.Image));
            }
            _context.Sliders.Remove(slider);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
