using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProjektTIP
{
    class Friend
    {
        private string nick;
        public string Nick
        {
            get{ return nick; }
            set{ nick = value; }
        }

        private int opinion;
        public int Opinion
        {
            get{ return opinion; }
            set
            {
                if(value >= 0 && value <= 5)
                {
                    opinion = value;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private string ip_adrress;
        public int Ip_adrress { get; set; }

        private Image avatar;
        public Image Avatar { get; set; }

        public Friend(string nick)
        {
            Nick = nick;
            avatar = new Image();
        }
    }
}
