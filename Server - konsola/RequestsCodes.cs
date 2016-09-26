using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server___konsola {
    enum RequestsCodes {
        GET_REQUESTS_LIST = 0,
        REGISTER = 10,
        HELLO = 20,
        ADD_FRIEND_TO_LIST = 30,
        CHANE_USER_DATA = 40,
        CHANGE_USER_PASSWORD = 50,
        LOOK_FOR_USER_BY_NAME = 60,
        LOOK_FOR_USER_BY_LOGIN = 70,
        LOGOUT = 80,

        WELCOME = 100,

        CALL = 200,
        RINGING = 210,
        OK = 220,
        BYE = 230,

        NO = 300,
    }
}
