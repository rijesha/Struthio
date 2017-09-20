using Geodesy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Struthio
{
    class Parser
    {
        public Parser(Dictionary<Double, Image> imageArray, Dictionary<Double, DataPoint> dataPointArray, Camera cam)
        {
            using (var reader = new System.IO.StreamReader(@"C:\Users\Rijesh\Desktop\markerResults.csv"))
            {
                reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    String[] values = reader.ReadLine().Split(',');
                    Image i = new Image(cam)
                    {
                        fileName = values[0],
                        coordinate = new GlobalCoordinates(new Angle(Convert.ToDouble(values[1])), new Angle(Convert.ToDouble(values[2]))),
                        gpsAlt = Convert.ToDouble(values[3]),
                        gpsAltRate = Convert.ToDouble(values[4]),
                        height = Convert.ToDouble(values[5]),
                        heightRate = Convert.ToDouble(values[6]),
                        pitch = Convert.ToDouble(values[7]),
                        pitchRate = Convert.ToDouble(values[8]),
                        roll = Convert.ToDouble(values[9]),
                        rollRate = Convert.ToDouble(values[10]),
                        yaw = Convert.ToDouble(values[11]),
                        yawRate = Convert.ToDouble(values[12]),
                        speed = Convert.ToDouble(values[13]),
                        bytes = Convert.ToDouble(values[14]),
                    };

                    //Console.WriteLine(i.GetHashCode());
                    if (!imageArray.ContainsKey(i.GetHashCode()))
                        imageArray.Add(i.GetHashCode(), i);
                    else
                        imageArray.TryGetValue(i.GetHashCode(), out i);

                    var pigeonCoordinate = new GlobalCoordinates(new Angle(Convert.ToDouble(values[17])), new Angle(Convert.ToDouble(values[18])));
                    var trueCoordinate = new GlobalCoordinates(new Angle(Convert.ToDouble(values[20])), new Angle(Convert.ToDouble(values[21])));

                    var dp = new DataPoint()
                    {
                        image = i,
                        targName = values[19],
                        pointX = Convert.ToDouble(values[15]),
                        pointY = Convert.ToDouble(values[16]),
                        pigeonCoordinate = pigeonCoordinate,
                        trueCoordinate = trueCoordinate
                    };

                    i.addDataPointToImage(dp);
                    
                    if (!dataPointArray.ContainsKey(dp.GetHashCode()))
                        dataPointArray.Add(dp.GetHashCode(), dp);
                    
                }
            }
        }
    }
}