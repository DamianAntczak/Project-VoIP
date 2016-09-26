using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ProjektTIP
{
    public class Friend
    {
        private string nick;
        public string Nick
        {
            get{ return nick; }
            set{ nick = value; }
        }

        private string name;
        public string Name { get; set; }

        private string last_name;
        public string Last_name { get; set; }

        public int Opinion { get; set; }

        private string ip_adrress;
        public int Ip_adrress { get; set; }

        private BitmapImage avatar;
        public BitmapImage Avatar { get; set; }

        public Friend(string nick)
        {
            Nick = nick;
            avatar = new BitmapImage();
            name = null;
            last_name = null;
            Opinion = 0;
        }

        public void setName(string first_name, string last_name)
        {
            Name = first_name;
            Last_name = last_name;
            
        }

        public void setAvatar(string image_path)
        {
            var image = new BitmapImage();
            Avatar = new BitmapImage(new Uri(@"\avatar-man.png",UriKind.Relative));

        }
    }
}
