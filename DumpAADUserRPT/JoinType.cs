using System;
using System.Collections.Generic;
using System.Text;

namespace GetAadJoinInformation
{

    //DSREG_UNKNOWN_JOIN
    //Value: 0
    //The type of join is not known.

    //DSREG_DEVICE_JOIN
    //Value: 1
    //The device is joined to Azure Active Directory (Azure AD).

    //DSREG_WORKPLACE_JOIN
    //Value: 2
    //An Azure AD work account is added on the device.

    public enum JoinType
    {
        DSREG_UNKNOWN_JOIN,
        DSREG_DEVICE_JOIN,
        DSREG_WORKPLACE_JOIN,
        DSREG_NO_JOIN
    }
}
