#if ALL_TESTS

namespace KarambaCommon.Tests.CrossSections
{
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Karamba.CrossSections;
    using NUnit.Framework;

    [TestFixture]
    public class Serialization_tests
    {
        [Test]
        public void BinarySerialization()
        {
            var crosecs = new List<CroSec>
            {
                new CroSec_Circle("family", "name", "country", null, null, 10.0, 2.0),
            };

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = File.Create(@"crosecs.dat"))
            {
                formatter.Serialize(fs, crosecs);
            }

            var table = new CroSecTable();
            table.read(@"crosecs.dat");

            var h0 = crosecs[0].getHeight();
            var h1 = table.crosecs[0].getHeight();
            Assert.That(h1, Is.EqualTo(h0).Within(1E-5));
        }

        [Test]
        public void BinaryReadWrite()
        {
            var crosecs = new List<CroSec>
            {
                new CroSec_Circle("family", "name", "country", null, null, 10.0, 2.0),
            };

            using (FileStream fs = File.Create(@"crosecs.bin"))
            {
                var writer = new BinaryWriter(fs);
                CroSecReaderWriter.write(crosecs, writer);
            }

            var new_crosecs = new List<CroSec>();
            using (FileStream fs = File.OpenRead(@"crosecs.bin"))
            {
                var reader = new BinaryReader(fs);
                new_crosecs = CroSecReaderWriter.read(reader);
            }

            var h0 = crosecs[0].getHeight();
            var h1 = new_crosecs[0].getHeight();
            Assert.That(h1, Is.EqualTo(h0).Within(1E-5));
        }
    }
}

#endif
