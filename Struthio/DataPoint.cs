using System;
using System.Collections.Generic;
using Geodesy;

namespace Struthio
{
    public class DataPoint
    {
        public Double pointX, pointY;
        public GlobalCoordinates trueCoordinate, pigeonCoordinate, calcCoordinate;

        public Image image;
        public String targName;
        
        private GeodeticCalculator geoCalc = new GeodeticCalculator(Ellipsoid.WGS84);


        public DataPoint()
        {
            
            
        }

        public double calcPigeonTrueError()
        {
            return geoCalc.CalculateGeodeticCurve(pigeonCoordinate, trueCoordinate).EllipsoidalDistance;
        }

        public double calcPigeonCalcError()
        {
            return geoCalc.CalculateGeodeticCurve(pigeonCoordinate, calcCoordinate).EllipsoidalDistance;
        }

        public double calcCalcTrueError()
        {
            return geoCalc.CalculateGeodeticCurve(calcCoordinate, trueCoordinate).EllipsoidalDistance;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + pointX.GetHashCode();
            hash = hash * 23 + pointY.GetHashCode();
            hash = hash * 23 + pigeonCoordinate.GetHashCode();

            return hash;
        }
    }

    public class Image
    {
        public String fileName;
        public GlobalCoordinates coordinate;
        public Double gpsAlt, gpsAltRate, height, heightRate, pitch, pitchRate, roll, rollRate, yaw, yawRate, speed, bytes;

        public List<DataPoint> dataPointList = new List<DataPoint>();
        public Camera cam;

        public VariableErrors solvedErrors;
        public int solvedGenNum;

        public Image(Camera cam)
        {
            this.cam = cam;
        }

        public string ToCSV()
        {
            return string.Format("{0},{1},{2},{3},{4},{5},{6}", fileName, rollRate, pitchRate, yawRate, bytes, gpsAltRate, dataPointList.Count);
        }

        public string errorsToCSV()
        {
           return string.Format("{0},{1},{2}", fileName, solvedGenNum, solvedErrors.toCSV());
        }

        public void addDataPointToImage(DataPoint dp)
        {
            dataPointList.Add(dp);
        }

        public void georeferenceAllDatapoints()
        {
            foreach (DataPoint dp in dataPointList)
            {
                GeoMath.Georeference(dp);
            }
        }

        public void georeferenceAllDatapoints(VariableErrors v)
        {
            foreach (DataPoint dp in dataPointList)
            {
                GeoMath.Georeference(dp, v);
            }
        }

        public double calcAverageDistanceError()
        {
            double sum = 0;
            foreach (DataPoint dp in dataPointList)
            {
                sum += dp.calcCalcTrueError();
            }
            return sum / dataPointList.Count;
        }

        public double calcSquaresDistanceError()
        {
            double sum = 0;
            foreach (DataPoint dp in dataPointList)
            {
                sum += Math.Pow(dp.calcCalcTrueError(),2);
            }
            return Math.Sqrt(sum);
        }

        public override string ToString() {
            return yaw + " " + height + " " + roll + " " + pitch;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + fileName.GetHashCode();
            hash = hash * 23 + coordinate.GetHashCode();
            hash = hash * 23 + bytes.GetHashCode();

            return hash;
        }
    }

    public class VariableErrors
    {
        public Double rollError = 0, pitchError = 0, yawError = 0, altError = 0, utmEastError = 0, utmWestError = 0;
        
        public VariableErrors()
        {
        }

        public VariableErrors(double[] values)
        {
            rollError = values[0];
            pitchError = values[1];
            yawError = values[2];
            altError = values[3];
            utmEastError = values[4];
            utmWestError = values[5];
        }
        public string toCSV()
        {
            return string.Format(
                        "{0}, {1}, {2}, {3}, {4}, {5}",
                        rollError,
                        pitchError,
                        yawError,
                        altError,
                        utmEastError,
                        utmWestError
                    );
        }


        public override string ToString()
        {
            return string.Format(
                        "rollErr {0}, pitchErr {1}, yawErr {2}, altErr {3}, utmEastErr {4}, utmNorthErr {5}",
                        rollError,
                        pitchError,
                        yawError,
                        altError,
                        utmEastError,
                        utmWestError
                    );
        }

    }
}
