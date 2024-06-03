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
            List<CroSec> crosecs = new List<CroSec>
            {
                new CroSec_Circle("family", "name", "country", null, null, 10.0, 2.0),
            };

            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = File.Create(@"crosecs.dat"))
            {
                formatter.Serialize(fs, crosecs);
            }

            CroSecTable table = new CroSecTable();
            table.read(@"crosecs.dat");

            double h0 = crosecs[0].getHeight();
            double h1 = table.crosecs[0].getHeight();
            Assert.That(h1, Is.EqualTo(h0).Within(1E-5));
        }

        [Test]
        public void BinaryReadWrite()
        {
            List<CroSec> crosecs = new List<CroSec>
            {
                new CroSec_Circle("family", "name", "country", null, null, 10.0, 2.0),
            };

            using (FileStream fs = File.Create(@"crosecs.bin"))
            {
                BinaryWriter writer = new BinaryWriter(fs);
                CroSecReaderWriter.write(crosecs, writer);
            }

            List<CroSec> new_crosecs = new List<CroSec>();
            using (FileStream fs = File.OpenRead(@"crosecs.bin"))
            {
                BinaryReader reader = new BinaryReader(fs);
                new_crosecs = CroSecReaderWriter.read(reader);
            }

            double h0 = crosecs[0].getHeight();
            double h1 = new_crosecs[0].getHeight();
            Assert.That(h1, Is.EqualTo(h0).Within(1E-5));
        }
    }
}

#endif
