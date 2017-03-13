//-----------------------------------------------------------------------
// <copyright file="Helper.cs" company="Studio A&T s.r.l.">
//     Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest.TwoFAUtility
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// class helper
    /// </summary>
    internal class Helper
    {
        /// <summary>
        /// Image To Byte Array
        /// </summary>
        /// <param name="image">object image</param>
        /// <param name="format">format of image</param>
        /// <returns>byte array from image</returns>
        public static byte[] ImageToByteArray(Image image, ImageFormat format)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }
}
