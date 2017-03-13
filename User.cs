//-----------------------------------------------------------------------
// <copyright file="User.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest.TwoFAUtility
{
    /// <summary>
    /// class user
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets user
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets secret key
        /// </summary>
        public string SecretKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets issuer ID
        /// </summary>
        public string IssuerID
        {
            get;
            set;
        }
    }
}
