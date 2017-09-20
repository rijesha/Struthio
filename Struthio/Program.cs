using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Struthio
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Dictionary<Double, Image> imageArray = new Dictionary<Double, Image>();
            Dictionary<Double, DataPoint> dataPointArray = new Dictionary<Double, DataPoint>();
            
            Camera cam = new Camera(1280, 960, GeoMath.Radians(43.603), GeoMath.Radians(33.398));
            new Parser(imageArray, dataPointArray, cam);


            //Check if calc is equal to pigeon calculated values
            foreach (KeyValuePair<Double, DataPoint> entry in dataPointArray)
            {
                GeoMath.Georeference(entry.Value);
                int mmError = (int) (entry.Value.calcPigeonCalcError() * 1000);
                Console.WriteLine("pigeon/VS err(mm): " + mmError + " VS/True err: " + entry.Value.calcCalcTrueError());

            }

            var csv = new StringBuilder();
            
            foreach (KeyValuePair<Double, Image> entry in imageArray)
            {
                csv.AppendLine(entry.Value.ToCSV());
            }
            File.AppendAllText(@"C:\Users\Rijesh\Desktop\rates.csv", csv.ToString());

            /*
            
            foreach (KeyValuePair<Double, Image> entry in imageArray)
            {
                Console.WriteLine(entry.Value.dataPointList.Count());
                new Genetic(entry.Value);
                csv.AppendLine(entry.Value.errorsToCSV());
            }
            File.AppendAllText(@"C:\Users\Rijesh\Desktop\errorResults.csv", csv.ToString());
            Console.ReadKey();*/


        }
    }
}
