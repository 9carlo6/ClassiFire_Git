using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp_FireSmoke.Models;

namespace WebApp_FireSmoke.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassifiedImagesController : ControllerBase
    {
        private readonly WebAppContext _context;

        public ClassifiedImagesController(WebAppContext context)
        {
            _context = context;
        }

        // GET: api/ClassifiedImages
        [HttpGet]
        public IEnumerable<ClassifiedImage> GetClassifiedImages()
        {
            return _context.ClassifiedImages;
        }

        // GET: api/ClassifiedImages/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClassifiedImage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var classifiedImage = await _context.ClassifiedImages.FindAsync(id);

            if (classifiedImage == null)
            {
                return NotFound();
            }

            return Ok(classifiedImage);
        }

        // PUT: api/ClassifiedImages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutClassifiedImage([FromRoute] int id, [FromBody] ClassifiedImage classifiedImage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != classifiedImage.Id)
            {
                return BadRequest();
            }

            _context.Entry(classifiedImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassifiedImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ClassifiedImages
        [HttpPost]
        public async Task<IActionResult> PostClassifiedImage([FromBody] ClassifiedImage classifiedImage)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.ClassifiedImages.Add(classifiedImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClassifiedImage", new { id = classifiedImage.Id }, classifiedImage);
        }

        // DELETE: api/ClassifiedImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClassifiedImage([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var classifiedImage = await _context.ClassifiedImages.FindAsync(id);
            if (classifiedImage == null)
            {
                return NotFound();
            }

            _context.ClassifiedImages.Remove(classifiedImage);
            await _context.SaveChangesAsync();

            return Ok(classifiedImage);
        }

        private bool ClassifiedImageExists(int id)
        {
            return _context.ClassifiedImages.Any(e => e.Id == id);
        }
    }
}