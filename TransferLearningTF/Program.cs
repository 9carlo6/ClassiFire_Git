using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Data.IO;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Image;
using ServiceStack.Support.Markdown;
using System.Diagnostics;


using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using OpenCvSharp;

namespace TransferLearningTF
{
    public class Program
    {
        static readonly string _assetsPath = Path.Combine(Environment.CurrentDirectory, "assets");
        static readonly string _trainTagsTsv = Path.Combine(_assetsPath, "inputs-train", "data", "tags.tsv");
        static readonly string _predictImageListTsv = Path.Combine(_assetsPath, "inputs-predict", "data", "image_list.tsv");
        static readonly string _trainImagesFolder = Path.Combine(_assetsPath, "inputs-train", "data");
        static readonly string _predictImagesFolder = Path.Combine(_assetsPath, "inputs-predict", "data");
        static readonly string _predictSingleImage = Path.Combine(_assetsPath, "inputs-predict-single", "data", "predict_image.jpg");
        static readonly string _inceptionPb = Path.Combine(_assetsPath, "inputs-train", "inception", "tensorflow_inception_graph.pb");
        static readonly string _inputImageClassifierZip = Path.Combine(_assetsPath, "inputs-predict", "imageClassifier.zip");
        static readonly string _outputImageClassifierZip = Path.Combine(_assetsPath, "outputs", "imageClassifier.zip");


        static readonly string _outputModelLocation = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel.pb");

        //Di seguito c'è una sequenza di path utilizzati per la costruzione di vari modelli
        //fire mymodel4
        static readonly string _outputModelLocation2 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel4.pb");
        //fire mymodel_fire_1000 = quello fatto dovo aver parlato con canfora, 500 img per classe
        static readonly string _outputModelLocation5 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel_fire_1000.pb");
        //fire mymodel_fire_aug_11000 = quello fatto dovo aver parlato con canfora, 55000 img per classe
        static readonly string _outputModelLocation6 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel_fire_aug_11000.pb");
        //smoke mymodel_smoke
        static readonly string _outputModelLocation3 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel_smoke_1.pb");
        //smoke mymodel_smoke_1000 = quello fatto dovo aver parlato con canfora, 500 img per classe
        static readonly string _outputModelLocation7 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel_smoke_1000.pb");
        //smoke mymodel_smoke_aug_11000 = quello fatto dovo aver parlato con canfora, 55000 img per classe
        static readonly string _outputModelLocation8 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel_smoke_aug_11000.pb");
        static readonly string _outputModelLocation4 = Path.Combine(_assetsPath, "inputs-train", "mymodel", "tensorflow_mymodel_fire_prova.pb"); //sbagliato

        static readonly string _capture2 = Path.Combine(_assetsPath, "frame_extraction.py");
        private static string LabelTokey = nameof(LabelTokey);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);


        static void Main(string[] args)
        {
            
            MLContext mlContext = new MLContext(seed: 1);
            
            // caricamento di un modello già esistente per poi successivamente classificare dellle immagini in input
            var model = ReuseAndTuneInceptionModel(mlContext, _trainTagsTsv, _trainImagesFolder, _inceptionPb, _outputImageClassifierZip);
            ClassifyImages(mlContext, _predictImageListTsv, _predictImagesFolder, _outputImageClassifierZip, model);
            //ClassifySingleImage(mlContext, _predictSingleImage, _outputImageClassifierZip, model);
            

            // creazione di un modello
            /*
            MLContext mlContext = new MLContext(seed: 1);
            ITransformer loadedModel = mlContext.Model.Load(_outputModelLocation7, out var modelInputSchema);
            ClassifyImages(mlContext, _predictImageListTsv, _predictImagesFolder, _outputImageClassifierZip, loadedModel);
            //ClassifySingleImage(mlContext, _predictSingleImage, _outputImageClassifierZip, loadedModel);
            */

        }


        // le immagini utilizzate per la creazione di un modelo vengono modificate con queste caratteristiche
        private struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }

        // la seguente funzione viene chiamata dalla Web App per poter classificare delle immagini utilizzando
        // il modello di classificazione per la rilevazione del fuoco
        public static List<String> ClassyAPIfire()
        {
            MLContext mlContext = new MLContext(seed: 1);
            ITransformer loadedModel = mlContext.Model.Load(_outputModelLocation6, out var modelInputSchema);
            var score = ClassifyImagesWEBApp(mlContext, _predictImageListTsv, _predictImagesFolder, _outputImageClassifierZip, loadedModel);
            return score;
        }

        // la seguente funzione viene chiamata dalla Web App per poter classificare delle immagini utilizzando
        // il modello di classificazione per la rilevazione del fumo
        public static List<String> ClassyAPIsmoke()
        {
            MLContext mlContext = new MLContext(seed: 1);
            ITransformer loadedModel = mlContext.Model.Load(_outputModelLocation8, out var modelInputSchema);
            var score = ClassifyImagesWEBApp(mlContext, _predictImageListTsv, _predictImagesFolder, _outputImageClassifierZip, loadedModel);
            return score;
        }

        // questa funzione permette di utilizzare un modello di classificazione già esistente per generarne uno nuovo
        public static ITransformer ReuseAndTuneInceptionModel(MLContext mlContext, string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            var data = mlContext.Data.LoadFromTextFile<ImageData>(path: dataLocation, hasHeader: false);

            var estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey, inputColumnName: "Label")
                .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: _trainImagesFolder, inputColumnName: nameof(ImageData.ImagePath)))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
                .Append(mlContext.Model.LoadTensorFlowModel(inputModelLocation).
                     ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelTokey, featureColumnName: "softmax2_pre_activation"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"))
                .AppendCacheCheckpoint(mlContext);
            ITransformer model = estimator.Fit(data);
            var predictions = model.Transform(data);
            var imageData = mlContext.Data.CreateEnumerable<ImageData>(data, false, true);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);
            DisplayResults(imagePredictionData);
            var multiclassContext = mlContext.MulticlassClassification;
            var metrics = multiclassContext.Evaluate(predictions, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");

            //metriche
            Console.WriteLine($"confusionMatrix number of classes: {metrics.ConfusionMatrix.NumberOfClasses}");
            Console.WriteLine($"confusionMatrix number of classes: {metrics.ConfusionMatrix}");
            Console.WriteLine($"confusionMatrix: {metrics.ConfusionMatrix.GetFormattedConfusionTable()}");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"LogLoss is: {metrics.LogLossReduction}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");
            Console.WriteLine($"PerClassRecall is: {String.Join(" , ", metrics.ConfusionMatrix.PerClassRecall.Select(c => c.ToString()))}");
            Console.WriteLine($"PerClassPrecision is: {String.Join(" , ", metrics.ConfusionMatrix.PerClassPrecision.Select(c => c.ToString()))}");


            var trainData = model.Transform(data);
            mlContext.Model.Save(model, trainData.Schema, _outputModelLocation8);

          
            return model;
        }




        // funzione utilizzata per la classificazione delle immagini
        public static void ClassifyImages(MLContext mlContext, string dataLocation, string imagesFolder, string outputModelLocation, ITransformer model)
        {
            var imageData = ReadFromTsv(dataLocation, imagesFolder);
            var imageDataView = mlContext.Data.LoadFromEnumerable<ImageData>(imageData);
            var predictions = model.Transform(imageDataView);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);
            DisplayResults(imagePredictionData);
        }

        // funzione utilizzata per la classificazione di una singola immagine
        public static void ClassifySingleImage(MLContext mlContext, string imagePath, string outputModelLocation, ITransformer model)
        {
            var imageData = new ImageData()
            {
                ImagePath = imagePath
            };
            var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var prediction = predictor.Predict(imageData);
            Console.WriteLine($"Image: {Path.GetFileName(imageData.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
        }

        // funzione chiamata dalla Web App per la classificazione di più immagini
        public static List<String> ClassifyImagesWEBApp(MLContext mlContext, string dataLocation, string imagesFolder, string outputModelLocation, ITransformer model)
        {
            var imageData = ReadFromTsv(dataLocation, imagesFolder);
            var imageDataView = mlContext.Data.LoadFromEnumerable<ImageData>(imageData);
            var predictions = model.Transform(imageDataView);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);
            var score = new List<String>();

            score = DisplayResultsWEBApp(imagePredictionData);
            return score;

        }

        // funzione chiamata dalla Web App per la classificazione di una singola immagine (non serve)
        public static string ClassifySingleImageWEBApp(MLContext mlContext, string imagePath, string outputModelLocation, ITransformer model)
        {
            var imageData = new ImageData()
            {
                ImagePath = imagePath
            };
            var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var prediction = predictor.Predict(imageData);
            Console.WriteLine($"Image: {Path.GetFileName(imageData.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
            string score = "predicted as: " + prediction.PredictedLabelValue.ToString() + " with score: " + prediction.Score.Max().ToString();

            return score;
        }

        // funzione utilizzata per mostrare i risultati della classificazione tramite il terminale
        private static void DisplayResults(IEnumerable<ImagePrediction> imagePredictionData)
        {
            var classification_error_fire = 0;
            var classification_error_not_fire = 0;
            var i = 0;
            foreach (ImagePrediction prediction in imagePredictionData)
            {
                var predizione = prediction.PredictedLabelValue;
                var valore = prediction.Score.Max();
                Console.WriteLine($"Image: {Path.GetFileName(prediction.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");

                if ((Path.GetFileName(prediction.ImagePath).ToString()).Substring(0, 5).Equals("smoke") && prediction.PredictedLabelValue.Equals("not_smoke")) { classification_error_fire = classification_error_fire + 1; }
                if ((Path.GetFileName(prediction.ImagePath).ToString()).Contains("not_smoke") && prediction.PredictedLabelValue.Equals("smoke")) { classification_error_not_fire = classification_error_not_fire + 1; }

            }

            // stampa errori commessi
            Console.WriteLine($"Errors in smoke image classification: {classification_error_fire} ");
            Console.WriteLine($"Errors in not_smoke image classification: {classification_error_not_fire} ");
        }

        // funzione utilizzata per inviare i risultati della classificazione alla Web App
        private static List<String> DisplayResultsWEBApp(IEnumerable<ImagePrediction> imagePredictionData)
        {
            var score_list = new List<String>();

            foreach (ImagePrediction prediction in imagePredictionData)
            {
                var predizione = prediction.PredictedLabelValue;
                var valore = prediction.Score.Max()*100;
                string valore_pred_stringa = valore.ToString();
                string valore_pred_stringa_percent = valore_pred_stringa.Split(',')[0].Trim();
                score_list.Add(prediction.PredictedLabelValue);
                score_list.Add(valore_pred_stringa_percent);
            }

            return score_list;
        }


        public static IEnumerable<ImageData> ReadFromTsv(string file, string folder)
        {
           return File.ReadAllLines(file).Select(line => line.Split('\t')).Select(line => new ImageData(){
                ImagePath = Path.Combine(folder, line[0])
           });
        }
    }
}
