﻿#if ALL_TESTS

namespace KarambaCommon.Tests.Result.ShellSection
{
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Elements.States;
    using Karamba.Geometry;
    using Karamba.Results.ShellSection;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class RetrieverGetResultsTests
    {
        private Karamba.Models.Model _model;
        private double _tolerance;

        private List<PolyLine3> _outputPolylines;
        private List<IList<int>> _outputCrossedFaces;

        [OneTimeSetUp]
        public void Initialize()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();

            var mesh = new Mesh3();
            mesh.AddVertex(0, 0, 0);
            mesh.AddVertex(0, 1, 0);
            mesh.AddVertex(1, 0, 0);
            mesh.AddVertex(1, 1, 0);
            mesh.AddFace(0, 2, 3);
            mesh.AddFace(0, 3, 1);

            var supportConditions = new List<bool>() { true, true, true, true, true, true };
            var supports = new List<Support>
            {
                k3d.Support.Support(new Point3(0, 0, 0), supportConditions),
                k3d.Support.Support(new Point3(0, 1, 0), supportConditions),
            };

            var loads = new List<Load>
            {
                k3d.Load.PointLoad(new Point3(1, 1, 0), new Vector3(-5, 0, -5)),
                k3d.Load.PointLoad(new Point3(1, 0, 0), new Vector3(-5, 0, -5)),
            };

            var logger = new MessageLogger();
            CroSec_Shell crosec = k3d.CroSec.ReinforcedConcreteStandardShellConst(1);
            List<Karamba.Elements.BuilderShell> shells = k3d.Part.MeshToShell(
                new List<Mesh3> { mesh },
                null,
                new List<CroSec> { crosec },
                logger,
                out _);
            Karamba.Models.Model model = k3d.Model.AssembleModel(
                shells,
                supports,
                loads,
                out _,
                out _,
                out _,
                out _,
                out _);
            model = k3d.Algorithms.AnalyzeThI(
                model,
                out _,
                out _,
                out _,
                out _);
            _model = model;
            _tolerance = 1E-12;
            _outputPolylines = new List<PolyLine3>()
            {
                new PolyLine3(new Point3(0.5, 0, 0), new Point3(0.5, 0.5, 0), new Point3(0.5, 1, 0)),
            };
            _outputCrossedFaces = new List<IList<int>>() { new List<int>() { 0, 1 } };
        }

        [Test]
        public void ForceRetriever_GetResults()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            // Arrange
            var meshGroup = ShellSectionTestsUtilities.BuildMeshGroup(_model.elems.OfType<ModelMembrane>(), _model);
            var shellSectionGeometry = new ShellSectionGeometry(_model, _model.elems.OfType<ModelMembrane>());

            var info = new ForcesInfo(
                shellSectionGeometry,
                new List<IStateSection2DSelector> { new StateSection2DSelectorIndex(_model, _model.lcActivation.LoadCaseCombinations[0], 0) },
                1E-5);
            var retriever = new ForceRetriever(info);

            // Act
            var output = retriever.GetResults(_outputPolylines, _outputCrossedFaces);

            // Assert
            Assert.That(output[ShellSecResult.N_tt][0][0][0], Is.EqualTo(-0.11520737327188897).Within(_tolerance));
            Assert.That(output[ShellSecResult.N_tt][0][1][0], Is.EqualTo(-0.89861751152073854).Within(_tolerance));

            Assert.That(output[ShellSecResult.N_nn][0][0][0], Is.EqualTo(-10.115207373271891).Within(_tolerance));
            Assert.That(output[ShellSecResult.N_nn][0][1][0], Is.EqualTo(-9.8847926267281174).Within(_tolerance));

            Assert.That(output[ShellSecResult.N_tn][0][0][0], Is.EqualTo(0.11520737327188962).Within(_tolerance));
            Assert.That(output[ShellSecResult.N_tn][0][1][0], Is.EqualTo(-0.11520737327188892).Within(_tolerance));

            Assert.That(output[ShellSecResult.M_tt][0][0][0], Is.EqualTo(0.70864983417139238).Within(_tolerance));
            Assert.That(output[ShellSecResult.M_tt][0][1][0], Is.EqualTo(0.47735904568310589).Within(_tolerance));

            Assert.That(output[ShellSecResult.M_nn][0][0][0], Is.EqualTo(4.7490504974858254).Within(_tolerance));
            Assert.That(output[ShellSecResult.M_nn][0][1][0], Is.EqualTo(5.2509495025141719).Within(_tolerance));

            Assert.That(output[ShellSecResult.M_tn][0][0][0], Is.EqualTo(-0.22885016582860404).Within(_tolerance));
            Assert.That(output[ShellSecResult.M_tn][0][1][0], Is.EqualTo(-0.11609674227024558).Within(_tolerance));

            Assert.That(output[ShellSecResult.V_t][0][0][0], Is.EqualTo(0.30910653150743272).Within(_tolerance));
            Assert.That(output[ShellSecResult.V_t][0][1][0], Is.EqualTo(-0.25541283299453926).Within(_tolerance));

            Assert.That(output[ShellSecResult.V_n][0][0][0], Is.EqualTo(-10.309106531507435).Within(_tolerance));
            Assert.That(output[ShellSecResult.V_n][0][1][0], Is.EqualTo(-9.6908934684925558).Within(_tolerance));
        }

        [Test]
        public void DisplacementRetriever_GetResults()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            // Arrange
            var meshGroup = ShellSectionTestsUtilities.BuildMeshGroup(_model.elems.OfType<ModelMembrane>(), _model);
            var shellSectionGeometry = new ShellSectionGeometry(_model, _model.elems.OfType<ModelMembrane>());
            var info = new DisplacementsInfo(
                shellSectionGeometry,
                new List<IStateSection2DSelector> { new StateSection2DSelectorIndex(_model, _model.lcActivation.LoadCaseCombinations[0], 0) },
                1E-5);
            var retriever = new DisplacementRetriever(info);

            // Act
            var output = retriever.GetResults(_outputPolylines, _outputCrossedFaces);

            // Assert
            Assert.That(output[ShellSecResult.X][0][0][0], Is.EqualTo(-1.68412232928183E-05).Within(_tolerance));
            Assert.That(output[ShellSecResult.Y][0][0][0], Is.EqualTo(-9.21658986175114E-07).Within(_tolerance));
            Assert.That(output[ShellSecResult.Z][0][0][0], Is.EqualTo(-0.647970257394647).Within(_tolerance));

            Assert.That(output[ShellSecResult.X][0][1][0], Is.EqualTo(-1.63385002094896E-05).Within(_tolerance));
            Assert.That(output[ShellSecResult.Y][0][1][0], Is.EqualTo(4.18935902857243E-07).Within(_tolerance));
            Assert.That(output[ShellSecResult.Z][0][1][0], Is.EqualTo(-0.685157079612762).Within(_tolerance));

            Assert.That(output[ShellSecResult.X][0][2][0], Is.EqualTo(-1.63385002094896E-05).Within(_tolerance));
            Assert.That(output[ShellSecResult.Y][0][2][0], Is.EqualTo(4.18935902857243E-07).Within(_tolerance));
            Assert.That(output[ShellSecResult.Z][0][2][0], Is.EqualTo(-0.685157079612762).Within(_tolerance));
        }

        [Test]
        public void StressesStrainsRetriever_GetResults()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            // Arrange
            var meshGroup = ShellSectionTestsUtilities.BuildMeshGroup(_model.elems.OfType<ModelMembrane>(), _model);
            var shellSectionGeometry = new ShellSectionGeometry(_model, _model.elems.OfType<ModelMembrane>());
            var info = new StressesStrainsInfo(
                shellSectionGeometry,
                new List<IStateSection2DSelector> { new StateSection2DSelectorIndex(_model, _model.lcActivation.LoadCaseCombinations[0], 0) },
                1E-5,
                0,
                0);
            var retriever = new StressesStrainsRetriever(info);

            // Act
            var output = retriever.GetResults(_outputPolylines, _outputCrossedFaces);

            // Assert
            Assert.That(output[ShellSecResult.Sig_tt][0][0][0], Is.EqualTo(-0.0011520737327188996).Within(_tolerance));
            Assert.That(output[ShellSecResult.Sig_tt][0][1][0], Is.EqualTo(-0.00898617511520737).Within(_tolerance));

            Assert.That(output[ShellSecResult.Sig_nn][0][0][0], Is.EqualTo(-0.10115207373271895).Within(_tolerance));
            Assert.That(output[ShellSecResult.Sig_nn][0][1][0], Is.EqualTo(-0.098847926267281172).Within(_tolerance));

            Assert.That(output[ShellSecResult.Sig_tn][0][0][0], Is.EqualTo(0.0011520737327189025).Within(_tolerance));
            Assert.That(output[ShellSecResult.Sig_tn][0][1][0], Is.EqualTo(-0.0011520737327189012).Within(_tolerance));

            Assert.That(output[ShellSecResult.Eps_tt][0][0][0], Is.EqualTo(0.0026811897779639689).Within(_tolerance));
            Assert.That(output[ShellSecResult.Eps_tt][0][1][0], Is.EqualTo(-3.2132054729097926E-20).Within(_tolerance));

            Assert.That(output[ShellSecResult.Eps_nn][0][0][0], Is.EqualTo(-0.033682446585672404).Within(_tolerance));
            Assert.That(output[ShellSecResult.Eps_nn][0][1][0], Is.EqualTo(-0.032677000418935918).Within(_tolerance));

            Assert.That(output[ShellSecResult.Eps_tn][0][0][0], Is.EqualTo(0.00041893590280687348).Within(_tolerance));
            Assert.That(output[ShellSecResult.Eps_tn][0][1][0], Is.EqualTo(-0.0004189359028068732).Within(_tolerance));
        }
    }
}

#endif
