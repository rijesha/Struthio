using Geodesy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Struthio
{
    public static class GeoMath
    {
        private const double DegreesToRadians = Math.PI / 180.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;
        private const double EarthRadius = 6378137.0;

        public static void Georeference(DataPoint dp, VariableErrors v)
        {
            Camera camera = dp.image.cam;
            //Calculate angle offsets fo selected pixel
            double delta_theta_horiz = Math.Atan((1 - 2 * dp.pointX / camera.image_width) * camera.tan_angle_div_2_horiz);
            double delta_theta_vert = Math.Atan((1 - 2 * dp.pointY / camera.image_height) * camera.tan_angle_div_2_vert);

            //Calculating effective pitch and roll
            double pitch = GeoMath.Radians(dp.image.pitch + v.pitchError) + delta_theta_vert;
            double roll = GeoMath.Radians(dp.image.roll + v.rollError) + delta_theta_horiz;


            //Calculating level distance and angle to pixel
            double distance_y = (dp.image.height + v.altError) * Math.Tan(pitch);
            double distance_x = (dp.image.height + v.altError) * Math.Tan(-roll);
            double distance = Math.Sqrt(distance_x * distance_x + distance_y * distance_y);
            double phi = Math.Atan2(distance_x, distance_y);

            //Calculating angle from north to pixel
            double forward_azimuth = phi + GeoMath.Radians(dp.image.yaw + v.yawError);

            //tep 5: calculating endpoint using pyproj GIS module
            GeoMath.CalculateNewPositionFwd(dp, distance, forward_azimuth, v);
        }

        public static void Georeference(DataPoint dp)
        {
            Camera camera = dp.image.cam;
            //Calculate angle offsets fo selected pixel
            double delta_theta_horiz = Math.Atan((1 - 2 * dp.pointX / camera.image_width) * camera.tan_angle_div_2_horiz);
            double delta_theta_vert = Math.Atan((1 - 2 * dp.pointY / camera.image_height) * camera.tan_angle_div_2_vert);

            //Calculating effective pitch and roll
            double pitch = GeoMath.Radians(dp.image.pitch) + delta_theta_vert;
            double roll = GeoMath.Radians(dp.image.roll) + delta_theta_horiz;


            //Calculating level distance and angle to pixel
            double distance_y = dp.image.height * Math.Tan(pitch);
            double distance_x = dp.image.height * Math.Tan(-roll);
            double distance = Math.Sqrt(distance_x * distance_x + distance_y * distance_y);
            double phi = Math.Atan2(distance_x, distance_y);

            //Calculating angle from north to pixel
            double forward_azimuth = phi + GeoMath.Radians(dp.image.yaw);

            //tep 5: calculating endpoint using pyproj GIS module
            VariableErrors v = new VariableErrors();
            GeoMath.CalculateNewPositionFwd(dp, distance, forward_azimuth, v);
        }
        
        public static void CalculateNewPositionFwd(DataPoint dp, Double distance, Double azimuth, VariableErrors v)
        {
            //GlobalCoordinates basecoordinate = new GlobalCoordinates(new Angle(dp.image.coordinate.Latitude.Degrees + v.utmEastError), new Angle(dp.image.coordinate.Longitude.Degrees + v.utmWestError));
            Oware.LatLngUTMConverter utm = new Oware.LatLngUTMConverter("WGS 84");

            var utmResult =  utm.convertLatLngToUtm(dp.image.coordinate.Latitude.Degrees, dp.image.coordinate.Longitude.Degrees);
            double lat_n, lon_n;
            ToLatLon2(utmResult.Easting + v.utmEastError, utmResult.Northing + v.utmWestError, "12U", out lat_n, out lon_n);
            GlobalCoordinates basecoordinate = new GlobalCoordinates(new Angle(lat_n), new Angle(lon_n));

            GeodeticCalculator geoCalc = new GeodeticCalculator(Ellipsoid.WGS84);
            dp.calcCoordinate = geoCalc.CalculateEndingGlobalCoordinates(basecoordinate, Degrees(azimuth), distance);
        }

        public static void ToLatLon2(double utmX, double utmY, string utmZone, out double latitude, out double longitude)
        {
            latitude = 0;
            longitude = 0;

            bool isNorthHemisphere = utmZone.Last() >= 'N';

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = int.Parse(utmZone.Remove(utmZone.Length - 1));
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (6366197.724 * 0.9996); // Change c_sa for 6366197.724
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = (5 * j4 + a2 * Math.Pow((Math.Cos(lat)), 2)) / 3.0; // saque a2 de multiplicar por el coseno de lat y elevar al cuadrado
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = (delt / Math.PI) * 180 + s;
            latitude = (((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat))) / Math.PI) * 180; // era incorrecto el calculo

        }
        /*        public static void calculateDistanceAndBearing(DataPoint dp)
                {
                    double dLat = Radians(dp.image.imageLat) - Radians(dp.trueLat);
                    double dLon = Radians(dp.image.imageLon) - Radians(dp.trueLon);
                    double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(Radians(dp.trueLat)) * Math.Cos(Radians(dp.image.imageLat)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    dp.inverseDistance = c * EarthRadius;

                    double lat1 = Radians(dp.image.imageLat);
                    double lat2 = Radians(dp.trueLat);
                    dLon = Radians(dp.trueLon) - Radians(dp.image.imageLon);

                    double y = Math.Sin(dLon) * Math.Cos(lat2);
                    double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(dLon);
                    double brng = Math.Atan2(y, x);

                    dp.inverseForwardAzimuth = (Degrees(brng) + 360) % 360;
                }
                */
        public static Double Radians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static Double Degrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

    }
}
