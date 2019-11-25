using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace WebApp_FireSmoke.Models
{
    public class WebAppContext : DbContext
    {

        public WebAppContext(DbContextOptions<WebAppContext> options)
            : base(options)
        { }




        public DbSet<ClassifiedImage> ClassifiedImages { get; set; }

        //nel caso in cui dovessi aggiungere altre classi, lo si fa qui
        //public DbSet<Post> Posts { get; set; }
    }

    public class ClassifiedImage
    {

        public ClassifiedImage(int id, DateTime date, double latitude, double longitude, string geoPolygon, string photo, string photoName, string fileType, string geolocalization, string geoJSON, string fireTypeClassification, string smokeTypeClassification, double fireScoreClassification, double smokeScoreClassification)
        {
            Id = id;
            Date = date;
            Latitude = latitude;
            Longitude = longitude;
            GeoPolygon = geoPolygon;
            Photo = photo;
            PhotoName = photoName;
            FileType = fileType;
            Geolocalization = geolocalization;
            GeoJSON = geoJSON;
            FireTypeClassification = fireTypeClassification;
            SmokeTypeClassification = smokeTypeClassification;
            FireScoreClassification = fireScoreClassification;
            SmokeScoreClassification = smokeScoreClassification;
        }

        public ClassifiedImage(int id, DateTime date, string latitude, string longitude, string geoPolygon, string photo, string photoName, string fileType, string geolocalization, string geoJSON, string fireTypeClassification, string smokeTypeClassification, double fireScoreClassification, double smokeScoreClassification)
        {
            Id = id;
            Date = date;
            Latitude = double.Parse(latitude, CultureInfo.InvariantCulture);
            Longitude = double.Parse(longitude, CultureInfo.InvariantCulture);
            GeoPolygon = geoPolygon;
            Photo = photo;
            PhotoName = photoName;
            FileType = fileType;
            Geolocalization = geolocalization;
            GeoJSON = geoJSON;
            FireTypeClassification = fireTypeClassification;
            SmokeTypeClassification = smokeTypeClassification;
            FireScoreClassification = fireScoreClassification;
            SmokeScoreClassification = smokeScoreClassification;
        }
        public ClassifiedImage()
        {

        }

        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GeoPolygon { get; set; }
        public string Photo { get; set; }
        public string PhotoName { get; set; }
        public string FileType { get; set; }
        public string Geolocalization { get; set; }
        public string GeoJSON { get; set; }
        public string FireTypeClassification { get; set; }
        public string SmokeTypeClassification { get; set; }
        public double FireScoreClassification { get; set; }
        public double SmokeScoreClassification { get; set; }

    }

    public class DesignTimeDbContextFactory : Microsoft.EntityFrameworkCore.Design.IDesignTimeDbContextFactory<WebAppContext>
    {
        public WebAppContext CreateDbContext(string[] args)
        {
            Microsoft.Extensions.Configuration.IConfigurationRoot configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<WebAppContext>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseSqlServer(connectionString);

            return new WebAppContext(builder.Options);


        }

        WebAppContext IDesignTimeDbContextFactory<WebAppContext>.CreateDbContext(string[] args)
        {
            throw new NotImplementedException();
        }
    }
}
