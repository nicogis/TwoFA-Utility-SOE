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

    internal class Helper
    {
        public static byte[] ImageToByte(Image img, ImageFormat format)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, format);
                stream.Close();

                byteArray = stream.ToArray();
            }
            return byteArray;
        }
    }
}
