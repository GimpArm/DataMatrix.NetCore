/*
DataMatrix.Net

DataMatrix.Net - .net library for decoding DataMatrix codes.
Copyright (C) 2009/2010 Michael Faschinger

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.
You can also redistribute and/or modify it under the terms of the
GNU Lesser General Public License as published by the Free Software
Foundation; either version 3.0 of the License or (at your option)
any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
General Public License or the GNU Lesser General Public License
for more details.

You should have received a copy of the GNU General Public
License and the GNU Lesser General Public License along with this
library; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

Contact: Michael Faschinger - michfasch@gmx.at

*/

using System;
using System.Text;
using DataMatrix.NetCore;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Test.DataMatrix.NetCore
{
    [TestFixture]
    public class EncodeDecode
    {
        private static readonly Assembly CurrentAssembly = Assembly.GetAssembly(typeof(EncodeDecode));
        private static readonly HashAlgorithm Hasher = MD5.Create();

        [Test]
        public void EncodeImage()
        {
            const string input = "Hello World";

            var encoder = new DmtxImageEncoder();
            var options = new DmtxImageEncoderOptions
            {
                ModuleSize = 8,
                MarginSize = 4,
                BackColor = Color.White,
                ForeColor = Color.Green
            };

            var encodedBitmap = encoder.EncodeImage(input, options);
            var ms = new MemoryStream();
            encodedBitmap.Save(ms, ImageFormat.Png);

            Assert.Greater(ms.Length, 0);
            Assert.AreEqual(input, DecodeHelper(ms));
        }

        [Test]
        public void EncodeSvgImage()
        {
            var encoder = new DmtxImageEncoder();
            var s = encoder.EncodeSvgImage("DataMatrix.net rocks!!one!eleven!!111!eins!!!!", 7, 7, Color.FromArgb(100, 255, 0, 0), Color.Turquoise);
            Assert.AreEqual(57553, s.Length);
            Assert.AreEqual("y7n4EIyCcD4HzTclHv34wA==", Convert.ToBase64String(Hasher.ComputeHash(Encoding.UTF8.GetBytes(s))));
        }

        [Test]
        public void DecodeImage(){
            var ms = CurrentAssembly.GetManifestResourceStream("Test.DataMatrix.NetCore.Resources.helloWorld.png");
            Assert.IsNotNull(ms, "Missing helloWorld.png resource");

            var decoder = new DmtxImageDecoder();
            var codes = decoder.DecodeImage((Bitmap)Image.FromStream(ms), 1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(1, codes.Count);
            Assert.AreEqual("HELLO WORLD", codes[0]);
        }

        [Test]
        public void MultiPass()
        {
            var encoder = new DmtxImageEncoder();
            var decoder = new DmtxImageDecoder();
            for (var i = 1; i < 10; i++)
            {
                var encodedData = Guid.NewGuid().ToString();
                var source = encoder.EncodeImage(encodedData);
                var decodedData = decoder.DecodeImage(source);
                Assert.AreEqual(1, decodedData.Count);
                Assert.AreEqual(encodedData, decodedData[0]);
            }
        }

        [Test]
        public void TestRawEncoder()
        {
            var expected = new[,]
            {
                {true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true},
                {false, false, false, true, true, true, true, true, false, false, false, false, false, true, false, true},
                {true, true, false, true, false, true, false, true, false, true, true, true, true, false, true, true},
                {false, true, false, false, false, false, false, false, false, true, false, true, false, false, false, true},
                {true, false, false, false, false, false, true, true, false, false, false, false, true, false, true, true},
                {false, true, true, true, false, false, false, true, true, true, true, false, true, false, false, true},
                {true, false, true, false, false, true, false, false, true, true, true, true, true, true, false, true},
                {false, false, false, true, false, false, false, true, false, true, true, false, false, false, true, true},
                {true, true, true, false, false, false, true, true, false, false, true, false, true, false, false, true},
                {false, false, true, true, false, true, false, true, true, false, false, false, false, true, true, true},
                {true, false, false, true, false, true, false, true, true, false, true, true, true, false, false, true},
                {false, true, false, true, false, false, true, false, true, true, true, true, true, true, true, true},
                {true, false, false, true, true, true, true, true, false, true, false, true, true, true, false, true},
                {false, false, false, true, true, false, false, false, true, true, true, true, true, true, false, true},
                {true, false, false, false, false, true, false, false, false, false, false, false, false, false, true, true},
                {false, true, false, true, false, true, false, true, false, true, false, true, false, true, false, true}
            };
            const string text = "HELLO WORLD";
            var encoder = new DmtxImageEncoder();
            var rawData = encoder.EncodeRawData(text);

            Assert.AreEqual(expected.GetLength(0), rawData.GetLength(0));
            Assert.AreEqual(expected.GetLength(1), rawData.GetLength(1));

            for (var y = 0; y < rawData.GetLength(0); y++)
            {
                for (var x = 0; x < rawData.GetLength(1); x++)
                {
                    Assert.AreEqual(expected[y, x], rawData[y, x]);
                }
            }
        }

        [Test]
        public void EncodeImageMosaic()
        {
            const string input = "Hello World!";
            var encoder = new DmtxImageEncoder();
            var options = new DmtxImageEncoderOptions
            {
                ModuleSize = 8,
                MarginSize = 4
            };
            var encodedBitmap = encoder.EncodeImageMosaic(input, options);
            var ms = new MemoryStream();
            encodedBitmap.Save(ms, ImageFormat.Png);

            Assert.Greater(ms.Length, 0);
            Assert.AreEqual(input, DecodeHelper(ms, true));
        }

        [Test]
        public void DecodeImageMosaic()
        {
            var decoder = new DmtxImageDecoder();
            var ms = CurrentAssembly.GetManifestResourceStream("Test.DataMatrix.NetCore.Resources.encodedMosaicImg.png");
            Assert.IsNotNull(ms, "Missing encodedMosaicImg.png resource");
            var codes = decoder.DecodeImageMosaic((Bitmap)Image.FromStream(ms), 1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(1, codes.Count);
            Assert.AreEqual("Hello World!", codes[0]);
        }

        [Test]
        public void TestGs1Encode()
        {
            const string gs1Code1 = "10AC3454G3";
            var encoder = new DmtxImageEncoder();
            var options = new DmtxImageEncoderOptions
            {
                ModuleSize = 8,
                MarginSize = 30,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Scheme = DmtxScheme.DmtxSchemeAsciiGS1
            };
            var encodedBitmap1 = encoder.EncodeImage(gs1Code1, options);
            var ms = new MemoryStream();
            encodedBitmap1.Save(ms, ImageFormat.Png);

            Assert.Greater(ms.Length, 0);
            Assert.AreEqual(gs1Code1, DecodeHelper(ms));
        }

        [Test]
        public void TestGs1DecodePng()
        {
            var decoder = new DmtxImageDecoder();
            var ms = CurrentAssembly.GetManifestResourceStream("Test.DataMatrix.NetCore.Resources.gs1DataMatrix1.png");
            Assert.IsNotNull(ms, "Missing gs1DataMatrix1.png resource");
            var decodedCodes = decoder.DecodeImage((Bitmap)Image.FromStream(ms), 1, TimeSpan.FromSeconds(5));

            Assert.IsNotNull(decodedCodes);
            Assert.AreEqual(1, decodedCodes.Count);
            Assert.AreEqual("10AC3454G3", decodedCodes[0]);
        }

        [Test]
        public void TestGs1DecodeGif()
        {
            var decoder = new DmtxImageDecoder();
            var ms = CurrentAssembly.GetManifestResourceStream("Test.DataMatrix.NetCore.Resources.gs1DataMatrix2.gif");
            Assert.IsNotNull(ms, "Missing gs1DataMatrix2.gif resource");
            var decodedCodes = decoder.DecodeImage((Bitmap)Image.FromStream(ms), 1, TimeSpan.FromSeconds(5));

            Assert.IsNotNull(decodedCodes);
            Assert.AreEqual(1, decodedCodes.Count);
            Assert.AreEqual("010761234567890017100503", decodedCodes[0]);
        }

        private static string DecodeHelper(Stream stream, bool isMosaic = false)
        {
            stream.Position = 0;
            var decoder = new DmtxImageDecoder();
            var decoded = isMosaic ? decoder.DecodeImageMosaic((Bitmap)Image.FromStream(stream), 1, TimeSpan.FromSeconds(5)): decoder.DecodeImage((Bitmap)Image.FromStream(stream), 1, TimeSpan.FromSeconds(5));
            return decoded?.FirstOrDefault();
        }
    }
}
