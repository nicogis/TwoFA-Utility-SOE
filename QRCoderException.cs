//-----------------------------------------------------------------------
// <copyright file="QRCoderException.cs" company="Studio A&T s.r.l.">
//  Copyright (c) Studio A&T s.r.l. All rights reserved.
// </copyright>
// <author>Nicogis</author>
//-----------------------------------------------------------------------
namespace Studioat.ArcGis.Soe.Rest
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// class QRCoderException Exception
    /// </summary>
    [Serializable]
    public class QRCoderException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the QRCoderException class
        /// </summary>
        public QRCoderException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the QRCoderException class
        /// </summary>
        /// <param name="message">message error</param>
        public QRCoderException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QRCoderException class
        /// </summary>
        /// <param name="message">message error</param>
        /// <param name="innerException">object Exception</param>
        public QRCoderException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the QRCoderException class
        /// </summary>
        /// <param name="info">object SerializationInfo</param>
        /// <param name="context">object StreamingContext</param>
        protected QRCoderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
