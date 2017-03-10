//-----------------------------------------------------------------------
// <copyright file="Users.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest.TwoFAUtility
{
    using System.Collections.Generic;

    public class Users2FA
    {
        public List<User> users
        {
            get;
            set;
        }
    }

    public class User
    {
        public string name
        {
            get;
            set;
        }

        public string secretKey
        {
            get;
            set;
        }
    }
}
