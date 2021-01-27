using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Elements;
using Karamba.Loads;
using Karamba.Joints;
using Karamba.Supports;
using Karamba.Models;
using Karamba.Utilities;
using Karamba.Algorithms;
using Karamba.Materials;

namespace KarambaCommon.Tests.Model
{
    [TestFixture]
    public class CantileverTests
    {
#if ALL_TESTS
        [Test]
        public void CantileverEigenfrequency()
        {
            // make temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();

            var ini = INIReader.Instance();
            ini.Values["UnitsSystem"] = "imperial";
            ini.Values["gravity"] = "9.80665";

            var k3d = new Toolkit();
            var E = 70000; // (kip/ft2) == 486111.1 (psi)
            var gamma = 1.0; // (kip/ft3)
            var unit_material = new FemMaterial_Isotrop("unit", "unit", E, 0.5*E, 0.5*E, gamma, 1.0, -1.0, FemMaterial.FlowHypothesis.mises, 1.0, null);
            var b = 6; // (inch)
            var t = 3; // (inch)
                       // Iy = 108 inch^4
                       // g = 20.833 lb/inch
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);
            
            var elems = new List<BuilderBeam>() {
                k3d.Part.IndexToBeam(0, 1, "A", unit_crosec),
            };

            var L = 10.0; // in feet
            var points = new List<Point3> { new Point3(), new Point3(L, 0, 0) };

            var supports = new List<Support> {k3d.Support.Support(0, k3d.Support.SupportFixedConditions)};

            var model = k3d.Model.AssembleModel(elems, supports, null, out var info, out var mass, out var cog, out var msg,
                out var runtimeWarning, new List<Joint>(), points);

            // calculate the natural vibrations
            int from_shape_ind = 1;
            int shapes_num = 1;
            int max_iter = 100;
            double eps = 1e-8;
            var disp_dummy = new List<double>();
            var scaling = EigenShapesScalingType.matrix;
            model = k3d.Algorithms.NaturalVibes(model, from_shape_ind, shapes_num, max_iter, eps, disp_dummy, scaling,
                out var nat_frequencies, out var modal_masses, out var participation_facs,
                out var participation_facs_disp, out model);

            // calculate the expected value of the first eigen-frequency
            // see Young, W. C., Budynas, R. G.(2002). Roark's Formulas for Stress and Strain .
            // 7nd Edition, McGraw-Hill, Chapter 16 , pp 767 - 768
            var E_ = E * 1000.0 * Math.Pow(UnitConversionCollection.inch_to_ft, 2);
            var I_ = unit_crosec.Iyy / Math.Pow(UnitConversionCollection.inch_to_ft, 4);
            var w_ = unit_crosec.A * gamma * 1000.0 * UnitConversionCollection.inch_to_ft;
            var g_ = UnitConversionCollection.g_IU / UnitConversionCollection.inch_to_ft;
            var L_ = L / UnitConversionCollection.inch_to_ft;
            var Kn = 3.52;
            var f1_expected = Kn / 2.0 / Math.PI * Math.Sqrt(E_ * I_ * g_ / w_ / Math.Pow(L_, 4));
            
            Assert.AreEqual(nat_frequencies[0], f1_expected, 1e-2);

            // clear temporary changes to the the ini-file and units-conversion
            INIReader.ClearSingleton();
            UnitsConversionFactory.ClearSingleton();
            ini = INIReader.Instance();
            // switch back to SI units
            ini.Values["UnitsSystem"] = "SI";
        }

        [Test]
        public void ModelMassSIUnits()
        {
            var k3d = new Toolkit();
            var gamma = 1.0; // (kN/m3)
            var unit_material = new FemMaterial_Isotrop("unit", "unit", 1, 0.5, 0.5, gamma, 1.0, -1.0, FemMaterial.FlowHypothesis.mises, 1.0, null);
            var b = 100; // (cm)
            var t = 50; // (cm)
            var unit_crosec = k3d.CroSec.Box(b, b, b, t, t, t, 0, 0, unit_material);

            var elems = new List<BuilderBeam>() {
                k3d.Part.IndexToBeam(0, 1, "A", unit_crosec),
            };

            var L = 1.0; // in m
            var points = new List<Point3> { new Point3(), new Point3(L, 0, 0) };

            var model = k3d.Model.AssembleModel(elems, null, null, out var info, out var mass, out var cog, out var msg,
                out var runtimeWarning, new List<Joint>(), points);

            var ucf = UnitsConversionFactory.Conv();
            mass = ucf.kg().toUnit(mass);

            Assert.AreEqual(mass, 100, 1e-10);
        }
#endif
    }
}
