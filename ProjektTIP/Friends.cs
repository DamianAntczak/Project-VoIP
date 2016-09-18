using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjektTIP
{
    class Friends : IEnumerable<Friend>
    {
        List<Friend> friends;

        public IEnumerator<Friend> GetEnumerator()
        {
            return friends.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void addFriend(String nick)
        {
            //szukanie w bazie danych

            friends.Add(new Friend(nick));
        }

        public void addFriend(Friend friend)
        {
            friends.Add(friend);
        }

        public Friends()
        {
            friends = new List<Friend>();
        }
    }
}
