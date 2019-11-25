using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace WebApp_FireSmoke.Models
{
    public class ImageWebApp
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GeoPolygon { get; set; }
        public Image Photo { get; set; }
        public string PhotoName { get; set; }
        public string FileType { get; set; }
        public string Geolocalization { get; set; }
        public string GeoJSON { get; set; }
        public string FireTypeClassification { get; set; }
        public string SmokeTypeClassification { get; set; }
        public double FireScoreClassification { get; set; }
        public double SmokeScoreClassification { get; set; }
    }
}
