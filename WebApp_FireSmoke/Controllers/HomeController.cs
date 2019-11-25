using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp_FireSmoke.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.ML;
using System.Threading;
using System.Web;
using System.Net.Http;
using System.Net;
using ServiceStack;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using WebApp_FireSmoke.Models;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace WebApp_FireSmoke.Controllers
{
    public class HomeController : Controller
    {
        private readonly WebAppContext _context;

        private readonly IHostingEnvironment he;
        public HomeController(IHostingEnvironment e, WebAppContext context)
        {
            he = e;
            _context = context;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult ShowClassification(string localization, List<IFormFile> files)
        {

            ViewData["localization"] = localization; //localization contiene il posizione in termini di latitudine e longitudine

            var photo_note = new List<String>(); //photo_note è una nota di testo contentente i nomi di tutte le immagini passate in input (serve per la classificazione)
            
            string path1 = @"assets\inputs-predict\data\image_list.tsv"; //path1 percorso della nota di testo
            string path2 = @"assets\inputs-predict\data"; //path2 percorso della cartella contenente le foto
            


            //se la cartella relativa al 'path2' non esiste viene creata
            if (System.IO.Directory.Exists(path2) == false)
            {
                Directory.CreateDirectory(path2); //viene creata la cartella che conterrà le immagini
            }

            //se la cartella relativa al 'path2' esiste ed è piena viene svuotata per far spazio alle nuove foto in input
            System.IO.DirectoryInfo di = new DirectoryInfo(path2);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            //files_from_video è la lista che conterrà i nome dei frame video (nel caso in cui in input ci fosse un video)
            List<String> files_from_video = new List<String>();

            //cotruisco una lista di oggetti da salvare nel database successivamente
            List<ClassifiedImage> classifiedImages = new List<ClassifiedImage>();

            int i = 0; //mi serve per la gestione della lista 'score'
            int n = 0; //mi serve per la gestione della lista 'classifiedImages'

            foreach (var image in files) //iterazione di tutte le immagini in input
            {
                if (image != null)
                {
                    /*
                     * GESTIONE CARICAMENTO IMMAGINI
                     */

                    

                    //il seguente if serve per controllare se il file caricato è un immagine o no
                    if (Path.GetExtension(image.FileName).ToLower() == ".jpg"
                        || Path.GetExtension(image.FileName).ToLower() == ".png"
                        || Path.GetExtension(image.FileName).ToLower() == ".gif"
                        || Path.GetExtension(image.FileName).ToLower() == ".jpeg")
                    {

                        var fileName = Path.Combine(he.WebRootPath, Path.GetFileName(image.FileName));
                        image.CopyTo(new FileStream(fileName, FileMode.Create)); 

                        photo_note.Add(image.FileName); //il nome dell'immagine viene aggiunto alla nota di testo 

                        string FileToCopy = null;
                        string NewCopy = null;
                        FileToCopy = fileName;
                        NewCopy = Path.Combine(path2, image.FileName); //viene caricata l'immagine nel 'path2'

                        //se l'immagine gia esiste viene sovrascritta
                        if (System.IO.File.Exists(FileToCopy) == true)
                        {
                            if (System.IO.File.Exists(NewCopy))
                                System.IO.File.Delete(NewCopy);
                            System.IO.File.Copy(FileToCopy, NewCopy);
                        }


                        //visto che in ingresso ci sono immagini viene settata a null la lista files_from_video
                        files_from_video = null;

                        Image p = Image.FromFile("assets/inputs-predict/data/" + image.FileName);


                        //converto l'immagine in una stringa base64
                        MemoryStream m = new MemoryStream();
                        p.Save(m, p.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        string base64String = Convert.ToBase64String(imageBytes);

                        string img_db = base64String;
                        string gpsLatitudeRef = " ";
                        double latitude = 0;
                        string gpsLongitudeRef = " ";
                        double longitude = 0;


                        //la seguente porzione di codice serve per estrapolare i metadati dalle immagine
                        try{
                            gpsLatitudeRef = BitConverter.ToChar(p.GetPropertyItem(1).Value, 0).ToString();
                            latitude = DecodeRational64u(p.GetPropertyItem(2));
                            gpsLongitudeRef = BitConverter.ToChar(p.GetPropertyItem(3).Value, 0).ToString();
                            longitude = DecodeRational64u(p.GetPropertyItem(4));
                            }
                        catch (System.ArgumentException e)
                            {
                            Console.WriteLine("{0} Exception caught.", e);
                            }
                       
                        string geolocalization = (latitude.ToString().Replace(",",".") + "," + longitude.ToString().Replace(",","."));
                        string filetype = Path.GetExtension(image.FileName);
                        string photoname = image.FileName;


                        //creo un nuovo oggetto da aggiungere al database
                        ClassifiedImage classifiedImage = new ClassifiedImage
                        {
                            PhotoName = photoname,
                            Date = DateTime.Now,
                            Photo = img_db,
                            Latitude = latitude,
                            Longitude = longitude,
                            FileType = filetype,
                            Geolocalization = geolocalization,
                            FireScoreClassification = 200,
                            SmokeScoreClassification = 233
                        };

                        classifiedImages.Add(classifiedImage);

                    }

                    /*
                     * GESTIONE CARICAMENTO IMMAGINI DA VIDEO
                     */
                    /*
                     * il servizio chiamato estrae i frame dal video caricato e successivamente
                     * carica all'interno della cartella (che poi si trasformerà in un db) sia questi ultimi che
                     * il file di testo.
                     * 
                     */
                    if (Path.GetExtension(image.FileName).ToLower() == ".avi")
                    {


                        var fileName = Path.Combine(he.WebRootPath, Path.GetFileName(image.FileName));
                        FileStream filestream = new FileStream(fileName, FileMode.Create);
                        image.CopyTo(filestream);
                        filestream.Close();
                        
                        string FileToCopy = fileName;
                        string NewCopy = null;
                        NewCopy = Path.Combine(path2, image.FileName); //viene caricato il video  
                        

                        //se il video gia esiste viene sovrascritta
                        if (System.IO.File.Exists(FileToCopy) == true)
                        {
                            if (System.IO.File.Exists(NewCopy))
                                System.IO.File.Delete(NewCopy);
                            System.IO.File.Copy(FileToCopy, NewCopy);
                        }

                        String uriString = "http://localhost:5020";
                        // Create a new WebClient instance.
                        WebClient myWebClient = new WebClient();

                        string fileNameHttpUpload = NewCopy;
                        // Caricamento del file sull'URI.
                        // Il metodo 'UploadFile(uriString,fileName)' usa implicitamente il metodo POST HTTP.
                        byte[] responseArray = myWebClient.UploadFile(uriString, fileNameHttpUpload);

                        // Decodificazione e visualizzazione della risposta.
                        Console.WriteLine("\nResponse Received.The contents of the file uploaded are:\n{0}",
                            System.Text.Encoding.ASCII.GetString(responseArray));

                        // estrapolazione dei nomi dei frame contenuti all'interno del file di testo per salvarli nell'arraylist photo_note
                        // lettura file usando StreamReader.
                        using (StreamReader file = new StreamReader(path1))
                        {
                            int counter = 0;
                            string ln;

                            while ((ln = file.ReadLine()) != null)
                            {
                                photo_note.Add(ln);
                                files_from_video.Add("/" + ln);
                                counter++;
                            }
                            file.Close();
                        }

                        foreach(var frame in files_from_video)
                        {
                            Image frame_video = Image.FromFile("assets/inputs-predict/data" + frame);

                            // conversione dell'immagine in una stringa base64
                            MemoryStream m = new MemoryStream();
                            frame_video.Save(m, frame_video.RawFormat);
                            byte[] imageBytes = m.ToArray();
                            string base64Stringvideo = Convert.ToBase64String(imageBytes);

                            string frame_db = base64Stringvideo;

                            string gpsLatitudeRef = " ";
                            double latitude = 0;
                            string gpsLongitudeRef = " ";
                            double longitude = 0;

                            try
                            {
                                gpsLatitudeRef = BitConverter.ToChar(frame_video.GetPropertyItem(1).Value, 0).ToString();
                                latitude = DecodeRational64u(frame_video.GetPropertyItem(2));
                                gpsLongitudeRef = BitConverter.ToChar(frame_video.GetPropertyItem(3).Value, 0).ToString();
                                longitude = DecodeRational64u(frame_video.GetPropertyItem(4));
                            }
                            catch (System.ArgumentException e)
                            {
                                Console.WriteLine("{0} Exception caught.", e);
                            }

                            string geolocalization = (latitude.ToString().Replace(",",".") + "," + longitude.ToString().Replace(",","."));
                            string filetype = Path.GetExtension(frame);
                            string photoname = frame;



                            //creazione di un nuovo oggetto da aggiungere al database
                            ClassifiedImage classifiedImage = new ClassifiedImage
                            {
                                PhotoName = photoname,
                                Date = DateTime.Now,
                                Photo = frame_db,
                                Latitude = latitude,
                                Longitude = longitude,
                                FileType = filetype,
                                Geolocalization = geolocalization,
                                FireScoreClassification = 200,
                                SmokeScoreClassification = 233
                            };

                            classifiedImages.Add(classifiedImage);
                        }
                    }
                }
            }


            // se il file di testo gia esiste viene cancellato e poi creato
            if (System.IO.File.Exists(path1) == true)
            {
                System.IO.File.Delete(path1);
            }
            using (StreamWriter sw = System.IO.File.CreateText(path1))
            {
                foreach (var photo in photo_note) //iterazione della nota per scrivere i nomi all'interno di essa
                {
                    sw.WriteLine(photo);
                }

            }

            // creazione di una lista di stringhe che contiene i nomi di ogni immagine classicata
            var listfilenames = new List<String>();
            foreach (var o in files) { listfilenames.Add("/" + Path.GetFileName(o.FileName)); }

            var score_fire = new List<String>(); // creazione di una lista di stringhe che conterrà i valori di classificazione con il modello del fuoco
            var score_smoke = new List<String>(); // creazione di una lista di stringhe che conterrà i valori di classificazione con il modello del fumo
            var score = new List<String>(); // creazione di una lista di stringhe che conterrà i valori di classificazione di entrambi i modelli

            // le liste vengono riempite chiamando un metodo proveniente dal progetto di tipo ML.NET
            score_fire = TransferLearningTF.Program.ClassyAPIfire();
            score_smoke = TransferLearningTF.Program.ClassyAPIsmoke();

            // riempimento della lista score iterando sulle liste dei due modelli
            i = 0;
            foreach (var sco in score_fire)
            {
                score.Add(sco);
                score.Add(score_smoke[i]);
                i++;
            }

            n = 0;
            foreach (var cimg in classifiedImages)
            {
                cimg.FireTypeClassification = score[n];
                cimg.SmokeTypeClassification = score[n + 1];
                cimg.FireScoreClassification = Convert.ToDouble(score[n + 2].ToString());
                cimg.SmokeScoreClassification = Convert.ToDouble(score[n + 3].ToString());
                n = n + 4;
            }


            if (files_from_video != null)
            {
                ViewData["filenames"] = files_from_video;
            }
            else
            {
                ViewData["filenames"] = listfilenames;
            }


            int countrow = CountRowInDB(); //qui conto il numero degli elementi all'interno del DB per calcolare l'Id

            Console.WriteLine("AAAAAAAAA: " + countrow);

            // questo serve per settare l'Id all'interno della lista classifiedImages da passare a ShowClassification
            int countrow_noid = 0;
            if (!countrow.Equals(1))
            {
                // se non è la prima immagine da inserire nel database allora bisogna prendere l'ultimo numero messo come id
                countrow_noid = GetLastIndexUpdate(countrow-2);
            }
            
            PostClassifiedImages(classifiedImages, countrow);

            ViewData["classifiedImages"] = classifiedImages;
            ViewData["score"] = score; // lista classificazione di ogni immagine

            return View();
        }

        // funzione utilizzata per l'estrazione dei metadati
        private static double DecodeRational64u(System.Drawing.Imaging.PropertyItem propertyItem)
        {
            uint dN = BitConverter.ToUInt32(propertyItem.Value, 0);
            uint dD = BitConverter.ToUInt32(propertyItem.Value, 4);
            uint mN = BitConverter.ToUInt32(propertyItem.Value, 8);
            uint mD = BitConverter.ToUInt32(propertyItem.Value, 12);
            uint sN = BitConverter.ToUInt32(propertyItem.Value, 16);
            uint sD = BitConverter.ToUInt32(propertyItem.Value, 20);

            decimal deg;
            decimal min;
            decimal sec;

            if (dD > 0) { deg = (decimal)dN / dD; } else { deg = dN; }
            if (mD > 0) { min = (decimal)mN / mD; } else { min = mN; }
            if (sD > 0) { sec = (decimal)sN / sD; } else { sec = sN; }

            double deg1 = (double)deg;
            double min1 = (double)min;
            double sec1 = (double)sec;

            if (sec == 0)
            {
                double conversion = ConvertDegreeAngleToDouble(deg1, min1, sec1);
                return conversion;
            }
            else
            {
                double conversion = ConvertDegreeAngleToDouble(deg1, min1, sec1);
                return conversion;
            }
        }

        public static double ConvertDegreeAngleToDouble(double degrees, double minutes, double seconds)
        {
            if (seconds == 0)
            {
                return degrees + (minutes / 60);
            }
            else
            {
                return degrees + (minutes / 60) + (seconds / 3600);
            }
        }


        public static void SaveJpeg(string path, Image img, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

            // Encoder parameter for image quality 
            EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
            // JPEG image codec 
            ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;
            img.Save(path, jpegCodec, encoderParams);
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }



        [HttpPost]
        public async Task<IActionResult> PostClassifiedImages([FromBody] List<ClassifiedImage> classifiedImages, int count)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            foreach (var img in classifiedImages)
            {
                _context.ClassifiedImages.Add(img);
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetClassifiedImage", new { id = count}, classifiedImages);
        }

        [HttpGet]
        public IEnumerable<ClassifiedImage> GetClassifiedImages()
        {
            return _context.ClassifiedImages;
        }

        public int CountRowInDB()
        {
            Console.WriteLine("CONTEGGIO: " + GetClassifiedImages().Count() + 1);
            Console.WriteLine("Id Image: " + GetClassifiedImages().ToString());
            return (GetClassifiedImages().Count() + 1);
        }

        public int GetLastIndexUpdate(int row)
        {
            Console.WriteLine("CONTEGGIO: " + GetClassifiedImages().Count() + 1);
            Console.WriteLine("ID Image: " + GetClassifiedImages().GetId());
            return (GetClassifiedImages().ElementAt(row).Id);
        }

        
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

        private bool ClassifiedImageExists(int id)
        {
            return _context.ClassifiedImages.Any(e => e.Id == id);
        }


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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var classifiedImage = await _context.ClassifiedImages.FindAsync(id);
            _context.ClassifiedImages.Remove(classifiedImage);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> PutClassifiedImage(int Id, DateTime Date, string Latitude, string Longitude, string GeoPolygon, string Photo, string PhotoName, string FileType, string Geolocalization, string GeoJSON, string FireTypeClassification, string SmokeTypeClassification, double FireScoreClassification, double SmokeScoreClassification)
            {

            ClassifiedImage classifiedImageobj = new ClassifiedImage(Id, Date, Latitude, Longitude, GeoPolygon, Photo, PhotoName, FileType, Geolocalization, GeoJSON, FireTypeClassification, SmokeTypeClassification, FireScoreClassification, SmokeScoreClassification);
            _context.Entry(classifiedImageobj).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassifiedImageExists(classifiedImageobj.Id))
                {
                    Console.WriteLine("L'immagine selezionata non esiste");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        public async Task<IActionResult> Nuova(int? Id, float Latitude)
        {

            return NoContent();
        }



        public static float? GetLatitude(Image targetImg)
        {
            try
            {
                //Property Item 0x0001 - PropertyTagGpsLatitudeRef
                PropertyItem propItemRef = targetImg.GetPropertyItem(1);
                //Property Item 0x0002 - PropertyTagGpsLatitude
                PropertyItem propItemLat = targetImg.GetPropertyItem(2);
                return ExifGpsToFloat(propItemRef, propItemLat);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static float? GetLongitude(Image targetImg)
        {
            try
            {
                ///Property Item 0x0003 - PropertyTagGpsLongitudeRef
                PropertyItem propItemRef = targetImg.GetPropertyItem(3);
                //Property Item 0x0004 - PropertyTagGpsLongitude
                PropertyItem propItemLong = targetImg.GetPropertyItem(4);
                return ExifGpsToFloat(propItemRef, propItemLong);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static float ExifGpsToFloat(PropertyItem propItemRef, PropertyItem propItem)
        {
            uint degreesNumerator = BitConverter.ToUInt32(propItem.Value, 0);
            uint degreesDenominator = BitConverter.ToUInt32(propItem.Value, 4);
            float degrees = degreesNumerator / (float)degreesDenominator;

            uint minutesNumerator = BitConverter.ToUInt32(propItem.Value, 8);
            uint minutesDenominator = BitConverter.ToUInt32(propItem.Value, 12);
            float minutes = minutesNumerator / (float)minutesDenominator;

            uint secondsNumerator = BitConverter.ToUInt32(propItem.Value, 16);
            uint secondsDenominator = BitConverter.ToUInt32(propItem.Value, 20);
            float seconds = secondsNumerator / (float)secondsDenominator;

            float coorditate = degrees + (minutes / 60f) + (seconds / 3600f);
            string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propItemRef.Value[0] }); //N, S, E, or W
            if (gpsRef == "S" || gpsRef == "W")
                coorditate = 0 - coorditate;
            return coorditate;
        }

        private static double ExifGpsToDouble(PropertyItem propItemRef, PropertyItem propItem)
        {
            double degreesNumerator = BitConverter.ToUInt32(propItem.Value, 0);
            double degreesDenominator = BitConverter.ToUInt32(propItem.Value, 4);
            double degrees = degreesNumerator / (double)degreesDenominator;

            double minutesNumerator = BitConverter.ToUInt32(propItem.Value, 8);
            double minutesDenominator = BitConverter.ToUInt32(propItem.Value, 12);
            double minutes = minutesNumerator / (double)minutesDenominator;

            double secondsNumerator = BitConverter.ToUInt32(propItem.Value, 16);
            double secondsDenominator = BitConverter.ToUInt32(propItem.Value, 20);
            double seconds = secondsNumerator / (double)secondsDenominator;


            double coorditate = degrees + (minutes / 60d) + (seconds / 3600d);
            string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propItemRef.Value[0] }); //N, S, E, or W
            if (gpsRef == "S" || gpsRef == "W")
                coorditate = coorditate * -1;
            return coorditate;
        }

        public void OnReceiveFiles(object sender, FileSystemEventArgs e)
            {
            WaitForFile(e.FullPath.ToString());
                //My process...
        }

        public void WaitForFile(string fullPath)
        {
            while (true)
            {
                try
                {
                    using (StreamReader stream = new StreamReader(fullPath))
                    {
                        break;
                    }
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
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
}
