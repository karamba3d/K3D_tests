#if ALL_TESTS

namespace KarambaCommon.Tests.Exporters
{
    using System;
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Exporters.SAF;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Loads.Beam;
    using Karamba.Materials;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;

    [TestFixture]
    public class SAF_Exporter_tests
    {
        [Test]
        public void Cantilever()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double length = 4.0;
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(length, 0, 0);
            Line3 axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            CroSec_Trapezoid crosec = new CroSec_Trapezoid(
                "family",
                "name",
                "country",
                null,
                Material_Default.Instance().steel,
                20,
                10,
                10);
            crosec.Az = 1E10; // make it stiff in shear

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            // create supports
            List<Support> supports = new List<Support>
            {
                // k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-loads
            const double fz = 10;
            var loads = new List<Load>
            {
                // k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz), Vector3.Zero, "LC0"),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            // export the model
            var version = new Version("2.3.0");
            // var version = new Version("1.0.5"); // for export to Dlubal
            var safExporter = new SAFExporter(model, out var msgLogger, version);
            safExporter.ExportModel(
                new List<string>(),
                "SAF_Cantilever.xlsx");
        }

        [Test]
        public void CantileverWithLoadCaseCombination()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double length = 4.0;
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(length, 0, 0);
            Line3 axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            CroSec_Trapezoid crosec = new CroSec_Trapezoid(
                "family",
                "name",
                "country",
                null,
                Material_Default.Instance().steel,
                20,
                10,
                10);
            crosec.Az = 1E10; // make it stiff in shear

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            // create supports
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-loads
            const double fz = 10;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz), Vector3.Zero, "LC0"),
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz / 10.0), Vector3.Zero, "LC1"),
                k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz / 100.0), Vector3.Zero, "LC2"),
                new BuilderLoadCaseCombination(new List<string>() { "LCC = 1.0*LC0 + 2*(LC1|LC2)", }),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            var loadCaseNames = new List<string>
            {
                "LC0",
                "LC1",
                "LC2",
                "LCC"
            };

            // analyze the model
            Analyze.solve(
                model,
                loadCaseNames,
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out var outModel,
                out var warningMessage);

            // export the model
            var version = new Version("2.3.0");
            // var version = new Version("1.0.5"); // for export to Dlubal
            var safExporter = new SAFExporter(outModel, out var msgLogger, version);
            safExporter.ExportModel(
                loadCaseNames,
                "SAF_Cantilever_WithLoads.xlsx");
        }

        [Test]
        public void SimplySupportedBeam_InclinedSupports_Eccentric()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double length = 4.0;
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(length, 0, 0);
            Line3 axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            CroSec_Trapezoid crosec = new CroSec_Trapezoid(
                "family",
                "name",
                "country",
                null,
                Material_Default.Instance().steel,
                20,
                10,
                10);
            crosec.Az = 1E10; // make it stiff in shear

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            double ecc = 0.5;
            // beams[0].ecce_loc = new Vector3(0, ecc, ecc);
            beams[0].ecce_glo = new Vector3(0, ecc, ecc);

            var ori = beams[0].Ori.Writer;
            ori.XOri = null;
            ori.YOri = null;
            ori.ZOri = new Vector3(0, 1, 1);
            ori.Alpha = 0;
            beams[0].Ori = ori.Reader;

            // create supports
            Plane3 plane = new Plane3(new Point3(), new Vector3(1, 0, 1), new Vector3(0, 1, 0));
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, false, false }, plane),
                k3d.Support.Support(p1, new List<bool>() { true, true, true, true, false, false }, plane),
            };

            // create a Point-loads
            const double fz = 10;
            const double my = 5;
            var loads = new List<Load>
            {
                k3d.Load.ConcentratedForceLoad(0.55, new Vector3(0, 0, fz), LoadOrientation.global),
                k3d.Load.ConcentratedMomentLoad(0.65, new Vector3(0, my, 0), LoadOrientation.local),
            };

            // create distribute load
            double m = 2.0;
            DistributedMoment load1 = k3d.Load.DistributedMomentLoad(
                Vector3.YAxis,
                new List<double> { m, m },
                new List<double> { 0.0, 1.0},
                LoadOrientation.local, "LC1");
            loads.Add(load1);

            double v = 3;
            DistributedForce load2 = k3d.Load.DistributedForceLoad(
                 Vector3.ZAxis,
                 new List<double> { v, 0.5 * v, v, 0 },
                 new List<double> { 0.0, 0.4, 0.4, 0.9},
                 LoadOrientation.local, "LC2");
            loads.Add(load2);

            DistributedForce load3 = k3d.Load.DistributedForceLoad(
                Vector3.ZAxis,
                new List<double> { v, 1.5 * v },
                new List<double> { 0.0, 1.0},
                LoadOrientation.local, "LC2");
            loads.Add(load3);

            // Temperature load
            loads.Add(k3d.Load.TemperatureLoad(string.Empty, 10, new Vector3(0, 0, 0), "LC3"));

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            var loadCaseNames = new List<string>
            {
                "LC0", "LC1", "LC2", "LC3"
            };

            // analyze the model
            Analyze.solve(
                model,
                loadCaseNames,
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out var outModel,
                out var warningMessage);

            // export the model
            // var version = new Version("2.3.0");
            var version = new Version("1.0.5"); // for export to Dlubal 
            var safExporter = new SAFExporter(outModel, out var msgLogger, version);
            safExporter.ExportModel(
                loadCaseNames,
                "SAF_SimplySupportedBeam_InclinedSupports_Eccentric.xlsx");
        }

        [Test]
        public void FixedFixedBeamWithSupportMovement()
        {
            Toolkit k3d = new Toolkit();
            MessageLogger logger = new MessageLogger();
            double length = 4.0;
            Point3 p0 = new Point3(0, 0, 0);
            Point3 p1 = new Point3(length, 0, 0);
            Line3 axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            CroSec_Trapezoid crosec = new CroSec_Trapezoid(
                "family",
                "name",
                "country",
                null,
                Material_Default.Instance().steel,
                20,
                10,
                10);
            crosec.Az = 1E10; // make it stiff in shear

            // create the column
            List<BuilderBeam> beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            // create supports
            List<Support> supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }, new Vector3(0, 0, 1), new Vector3(0, 0.02, 0), "LC0"),
                k3d.Support.Support(p1, new List<bool>() { true, true, true, true, true, true }),
            };

            // create a Point-loads
            const double fz = 10;
            var loads = new List<Load>
            {
                // k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz), Vector3.Zero, "LC0"),
            };

            // create the model
            var model = k3d.Model.AssembleModel(
                beams,
                supports,
                loads,
                out var info,
                out var mass,
                out var cog,
                out var message,
                out var warning);

            // export the model
            var version = new Version("2.3.0");
            // var version = new Version("1.0.5"); // for export to Dlubal
            var safExporter = new SAFExporter(model, out var msgLogger, version);
            safExporter.ExportModel(
                new List<string>(),
                "SAF_FixedFixedBeamWithSupportMovement.xlsx");
        }
    }
}

#endif
