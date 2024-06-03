#if ALL_TESTS

namespace KarambaCommon.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Elements.States;
    using Karamba.Geometry;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using ModelFactory = KarambaCommon.Tests.Utilities.ModelFactory; // todo: move
    using Model = Karamba.Models.Model;
    using TestableBeamProperties = KarambaCommon.Tests.Helpers.TestableBeamProperties;
    using UnitSystem = Karamba.Utilities.UnitSystem;

    public class State1DTests
    {
        private Model CreateHingedBeamModelWithConcentratedForceAtMid()
        {
            double beamLength = 100.0; // cm
            double loadValue = -10; // kN/cm

            var load = Toolkit.CreateToolkit.Load.ConcentratedForceLoad(
                0.5,
                new Vector3(0, 0, loadValue));

            var beamProperties = TestableBeamProperties.HingedBeamDefaultProperties();
            beamProperties.Loads.Add(load);

            return ModelFactory.CreateOneBeamModel(
                new Point3(0, 0, 0),
                new Point3(beamLength, 0, 0),
                beamProperties);
        }

        [Test]
        public void GetResultValue_ReturnsExpectedValues()
        {
            // Arrange
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();
            var crosec = beam.crosec as CroSec_Beam;

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var stateAtStart = beam.GetState1D(0, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: false, out _, out _);
            var stateAtMid = beam.GetState1D(50, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: false, out _, out _);
            var stateAtEnd = beam.GetState1D(100, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: false, out _, out _);

            var resultsAtStart = GetAllResults(stateAtStart);
            var resultsAtMid = GetAllResults(stateAtMid);
            var resultsAtEnd = GetAllResults(stateAtEnd);

            // Assert
            double beamLength = 100.0; // cm
            double loadValue = -10; // kN/cm
            double youngModulus = beam.crosec.material.E(); // kN/cm2
            double momentInertia = ((CroSec_Beam)beam.crosec).Iyy; // cm4

            var expectedResultsAtStart = new Dictionary<Element1DOption, double>()
            {
                { Element1DOption.Nx, 0 },
                { Element1DOption.Vy, 0 },
                { Element1DOption.Vz, loadValue / 2 },
                { Element1DOption.RF, Math.Abs(loadValue / 2) },
                { Element1DOption.Mt, 0 },
                { Element1DOption.My, 0 },
                { Element1DOption.Mz, 0 },
                { Element1DOption.RM, 0 },
                { Element1DOption.TransX, 0 },
                { Element1DOption.TransY, 0 },
                { Element1DOption.TransZ, 0 },
                { Element1DOption.Trans, 0 },
                { Element1DOption.RotX, 0 },
                { Element1DOption.RotY, -(loadValue * beamLength * beamLength) / (16 * youngModulus * momentInertia) },
                { Element1DOption.RotZ, 0 },
                { Element1DOption.Rot, Math.Abs((loadValue * beamLength * beamLength) / (16 * youngModulus * momentInertia)) },
                { Element1DOption.SigXX, 0 },
                { Element1DOption.Util, 0 },
            };
            Assert.That(resultsAtStart.Keys, Is.EquivalentTo(expectedResultsAtStart.Keys));
            Assert.That(resultsAtStart.Values, Is.EqualTo(expectedResultsAtStart.Values).Within(1E-4));

            var expectedResultsAtMid = new Dictionary<Element1DOption, double>()
            {
                { Element1DOption.Nx, 0 },
                { Element1DOption.Vy, 0 },
                { Element1DOption.Vz, -loadValue / 2 },
                { Element1DOption.RF, Math.Abs(loadValue / 2) },
                { Element1DOption.Mt, 0 },
                { Element1DOption.My, loadValue * beamLength / 4 },
                { Element1DOption.Mz, 0 },
                { Element1DOption.RM, Math.Abs(loadValue * beamLength / 4) },
                { Element1DOption.TransX, 0 },
                { Element1DOption.TransY, 0 },
                { Element1DOption.TransZ, (loadValue * Math.Pow(beamLength, 3)) / (48 * youngModulus * momentInertia) },
                { Element1DOption.Trans, Math.Abs((loadValue * Math.Pow(beamLength, 3)) / (48 * youngModulus * momentInertia)) },
                { Element1DOption.RotX, 0 },
                { Element1DOption.RotY, 0 },
                { Element1DOption.RotZ, 0 },
                { Element1DOption.Rot, 0 },
                { Element1DOption.SigXX, loadValue * beamLength / 4 / crosec.Wely_z_pos },
                { Element1DOption.Util, loadValue * beamLength / 4 / crosec.Wely_z_pos / crosec.material.ft() },
            };
            Assert.That(resultsAtMid.Keys, Is.EquivalentTo(expectedResultsAtMid.Keys));
            Assert.That(resultsAtMid.Values, Is.EqualTo(expectedResultsAtMid.Values).Within(1E-2));

            var expectedResultsAtEnd = new Dictionary<Element1DOption, double>()
            {
                { Element1DOption.Nx, 0 },
                { Element1DOption.Vy, 0 },
                { Element1DOption.Vz, -loadValue / 2 },
                { Element1DOption.RF, Math.Abs(-loadValue / 2) },
                { Element1DOption.Mt, 0 },
                { Element1DOption.My, 0 },
                { Element1DOption.Mz, 0 },
                { Element1DOption.RM, 0 },
                { Element1DOption.TransX, 0 },
                { Element1DOption.TransY, 0 },
                { Element1DOption.TransZ, 0 },
                { Element1DOption.Trans, 0 },
                { Element1DOption.RotX, 0 },
                { Element1DOption.RotY, (loadValue * beamLength * beamLength) / (16 * youngModulus * momentInertia) },
                { Element1DOption.RotZ, 0 },
                { Element1DOption.Rot, Math.Abs((loadValue * beamLength * beamLength) / (16 * youngModulus * momentInertia)) },
                { Element1DOption.SigXX, 0 },
                { Element1DOption.Util, 0 },
            };
            Assert.That(resultsAtEnd.Keys, Is.EquivalentTo(expectedResultsAtEnd.Keys));
            Assert.That(resultsAtEnd.Values, Is.EqualTo(expectedResultsAtEnd.Values).Within(1E-4));

            Dictionary<Element1DOption, double> GetAllResults(IStateElement1D state)
            {
                var dictionary = new Dictionary<Element1DOption, double>();

                foreach (var resultType in Enum.GetValues(typeof(Element1DOption)).Cast<Element1DOption>())
                {
                    if (resultType == Element1DOption.None)
                        continue;
                    var value = state.GetResult(resultType, out _).First();
                    dictionary.Add(resultType, value);
                }

                return dictionary;
            }
        }

        [Test]
        public void GetForceVector_Equals_NxVyVz()
        {
            // Arrange
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state = beam.GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var forceVector = state.GetForce(out _);

            // Assert
            Assert.That(forceVector[0], Is.EqualTo(state.GetResult(Element1DOption.Nx, out _).First()));
            Assert.That(forceVector[1], Is.EqualTo(state.GetResult(Element1DOption.Vy, out _).First()));
            Assert.That(forceVector[2], Is.EqualTo(state.GetResult(Element1DOption.Vz, out _).First()));
        }

        [Test]
        public void GetMomentVector_Equals_MtMyMz()
        {
            // Arrange
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state = beam.GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var momentVector = state.GetMoment(out _);

            // Assert
            Assert.That(momentVector[0], Is.EqualTo(state.GetResult(Element1DOption.Mt, out _).First()));
            Assert.That(momentVector[1], Is.EqualTo(state.GetResult(Element1DOption.My, out _).First()));
            Assert.That(momentVector[2], Is.EqualTo(state.GetResult(Element1DOption.Mz, out _).First()));
        }

        [Test]
        public void GetTranslationVector_Equals_TransXTransYTransZ()
        {
            // Arrange
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state = beam.GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var momentVector = state.GetTranslation(out _);

            // Assert
            Assert.That(momentVector[0], Is.EqualTo(state.GetResult(Element1DOption.TransX, out _).First()));
            Assert.That(momentVector[1], Is.EqualTo(state.GetResult(Element1DOption.TransY, out _).First()));
            Assert.That(momentVector[2], Is.EqualTo(state.GetResult(Element1DOption.TransZ, out _).First()));
        }

        [Test]
        public void GetRotationVector_Equals_TransXTransYTransZ()
        {
            // Arrange
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state = beam.GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var momentVector = state.GetRotation(out _);

            // Assert
            Assert.That(momentVector[0], Is.EqualTo(state.GetResult(Element1DOption.RotX, out _).First()));
            Assert.That(momentVector[1], Is.EqualTo(state.GetResult(Element1DOption.RotY, out _).First()));
            Assert.That(momentVector[2], Is.EqualTo(state.GetResult(Element1DOption.RotZ, out _).First()));
        }

        [Test]
        public void GetDeformedPoint_ReturnsExpectedPoint()
        {
            // Arrange
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state = beam.GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var deformedPoint = state.DisplacedPosition(out _);

            // Assert
            double beamLength = 100.0; // cm
            double loadValue = -10; // kN/cm
            double youngModulus = beam.crosec.material.E(); // kN/cm2
            double momentInertia = ((CroSec_Beam)beam.crosec).Iyy; // cm4
            var expectedPoint = new Point3(
                50,
                0,
                (loadValue * Math.Pow(beamLength, 3)) / (48 * youngModulus * momentInertia));

            Assert.That(deformedPoint.ToArray(), Is.EqualTo(expectedPoint.ToArray()).Within(1E-1));
        }

        [Test]
        public void GetDeformedCoordinateSystem()
        {
            // Arrange
            Helper.InitIniConfigTest(UnitSystem.SI, false);
            var model = CreateHingedBeamModelWithConcentratedForceAtMid();
            model = Toolkit.CreateToolkit.Algorithms.AnalyzeThI(model, out _, out _, out _, out var _);
            var beam = (ModelElementStraightLine)model.elems.First();

            // Act
            var lcc = model.lcActivation.LoadCaseCombinations.First();
            var state = beam.GetState1D(0.5, model, new StateElement1DSelectorIndex(model, lcc), isCurveReparametrized: true, out _, out _);
            var coordinateSystem = state.DisplacedCoordinateSystem();

            // Assert
            var expectedCoordinateSystem = new CoordinateSystem3(
                state.DisplacedPosition(out _),
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ);

            Assert.That(coordinateSystem, Is.EqualTo(expectedCoordinateSystem));
        }
    }
}
#endif
