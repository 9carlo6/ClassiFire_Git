using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp_FireSmoke.Models;

namespace WebApp_FireSmoke.Controllers
{
    public class ClassifiedImages1Controller : Controller
    {
        private readonly WebAppContext _context;

        public ClassifiedImages1Controller(WebAppContext context)
        {
            _context = context;
        }

        // GET: ClassifiedImages1
        public async Task<IActionResult> Index()
        {
            return View(await _context.ClassifiedImages.ToListAsync());
        }

        // GET: ClassifiedImages1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classifiedImage = await _context.ClassifiedImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classifiedImage == null)
            {
                return NotFound();
            }

            return View(classifiedImage);
        }

        // GET: ClassifiedImages1/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ClassifiedImages1/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Latitude,Longitude,GeoPolygon,Photo,PhotoName,FileType,Geolocalization,GeoJSON,FireTypeClassification,SmokeTypeClassification,FireScoreClassification,SmokeScoreClassification")] ClassifiedImage classifiedImage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(classifiedImage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(classifiedImage);
        }

        // GET: ClassifiedImages1/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classifiedImage = await _context.ClassifiedImages.FindAsync(id);
            if (classifiedImage == null)
            {
                return NotFound();
            }
            return View(classifiedImage);
        }

        // POST: ClassifiedImages1/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Latitude,Longitude,GeoPolygon,Photo,PhotoName,FileType,Geolocalization,GeoJSON,FireTypeClassification,SmokeTypeClassification,FireScoreClassification,SmokeScoreClassification")] ClassifiedImage classifiedImage)
        {
            if (id != classifiedImage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(classifiedImage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassifiedImageExists(classifiedImage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(classifiedImage);
        }

        // GET: ClassifiedImages1/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var classifiedImage = await _context.ClassifiedImages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classifiedImage == null)
            {
                return NotFound();
            }

            return View(classifiedImage);
        }

        // POST: ClassifiedImages1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classifiedImage = await _context.ClassifiedImages.FindAsync(id);
            _context.ClassifiedImages.Remove(classifiedImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClassifiedImageExists(int id)
        {
            return _context.ClassifiedImages.Any(e => e.Id == id);
        }
    }
}
