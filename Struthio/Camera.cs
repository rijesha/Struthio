using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Struthio
{
    public class Camera
    {
        public double image_width;
        public double image_height;
        public double tan_angle_div_2_horiz;
        public double tan_angle_div_2_vert;

        public Camera(double image_width, double image_height, double hfv, double vfv)
        {
            this.image_height = image_height;
            this.image_width = image_width;

            tan_angle_div_2_horiz = Math.Tan(hfv / 2);
            tan_angle_div_2_vert  = Math.Tan(vfv / 2);
        } 
    }
}
