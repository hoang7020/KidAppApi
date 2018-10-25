using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KidApp.Models
{
    public class ImageDTO
    {
        public string imageId { get; set; }
        public string imageName { get; set; }
        public Nullable<double> timeShoot { get; set; }
        public string userId { get; set; }
        public Nullable<bool> active { get; set; }
    }
}