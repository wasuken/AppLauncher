using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppLauncher_v3
{
    public struct ImageList
    {
        public ITEM_TYPE type;
        public string MyImageName { get; set; }
        public ImageSource MyImage { get; set; }
        public string fullPath { set; get; }
        public ImageList(string fullPath, ImageSource MyImage, ITEM_TYPE type)
        {
            this.MyImage = MyImage;
            this.fullPath = fullPath;
            this.MyImageName = fullPath.Split('\\')[fullPath.Split('\\').Length - 1];
            this.type = type;
        }
    }
}
