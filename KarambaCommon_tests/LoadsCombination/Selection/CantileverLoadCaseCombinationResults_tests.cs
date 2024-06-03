#if ALL_TESTS

namespace KarambaCommon.Tests.Loads
{
    using System;
    using System.Collections.Generic;
    using Karamba.Algorithms;
    using Karamba.CrossSections;
    using Karamba.Elements;
    using Karamba.Geometry;
    using Karamba.Loads;
    using Karamba.Materials;
    using Karamba.Models;
    using Karamba.Results;
    using Karamba.Supports;
    using Karamba.Utilities;
    using KarambaCommon;
    using NUnit.Framework;
    using Helper = KarambaCommon.Tests.Helpers.Helper;
    using Load = Karamba.Loads.Load;

    [TestFixture]
    public class CantileverLoadCaseCombinationResults_tests
    {
        /// <summary>
        /// Creates and solves a cantilever with three loads at its tip: LCC = A|B|C
        /// </summary>
        /// <param name="fA">magnitude of first load at tip.</param>
        /// <param name="fB">magnitude of second load at tip.</param>
        /// <param name="fC">magnitude of third load at tip.</param>
        /// <param name="length">length of the cantilever.</param>
        /// <returns></returns>
        private Model CreateAndSolveModel(out double fA, out double fB, out double fC, out double length, out CroSec_Trapezoid crosec)
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);

            var k3d = new Toolkit();
            var logger = new MessageLogger();
            length = 4.0;
            var p0 = new Point3(0, 0, 0);
            var p1 = new Point3(length, 0, 0);
            var axis = new Line3(p0, p1);

            // get a cross section from the cross section table in the folder 'Resources'
            crosec = new CroSec_Trapezoid(
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
            var beams = k3d.Part.LineToBeam(
                new List<Line3> { axis },
                new List<string>() { "B1" },
                new List<CroSec>() { crosec },
                logger,
                out List<Point3> out_points);

            // create supports
            var supports = new List<Support>
            {
                k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
            };

            // create two Point-loads in load-case-combinations 'LCA' and 'LCB'
            fA = 10;
            fB = -5;
            fC = -2.5;
            var loads = new List<Load>
            {
                k3d.Load.PointLoad(p1, new Vector3(-fA, fA, fA), new Vector3(), "A"),
                k3d.Load.PointLoad(p1, new Vector3(-fB, fB, fB), new Vector3(), "B"),
                k3d.Load.PointLoad(p1, new Vector3(-fC, fC, fC), new Vector3(), "C"),
                new BuilderLoadCaseCombination(new List<string>() { "LCC = (A|B|C)", }),
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

            Analyze.solve(
                model,
                new List<string>() { "LCC" },
                out IReadOnlyList<double> maxDisp,
                out IReadOnlyList<Vector3> force,
                out IReadOnlyList<double> energy,
                out var outModel,
                out var warningMessage);

            return outModel;
        }

        [Test]
        public void BeamCantilever_Stresses_SigXX_min_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Beam stresses at support of cantilever
            //--
            BeamStresses.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/SigXX/min/max",
                new List<double> { 0.0 },
                out var beamStresses,
                out var governingLoadCasesStresses);

            var sigTargetMin = -fA * length / crosec.Wely_z_pos - fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetMax = fA * length / crosec.Wely_z_pos + fA * length / crosec.Welz_y_pos - fA / crosec.A;
            Assert.That(beamStresses[0][0][0], Is.EqualTo(sigTargetMin).Within(1e-8));
            Assert.That(beamStresses[0][0][1], Is.EqualTo(sigTargetMax).Within(1e-8));

            var loadCase = governingLoadCasesStresses[0][0][1].ToString();
            Assert.That(loadCase == "A");
            loadCase = governingLoadCasesStresses[0][0][0].ToString();
            Assert.That(loadCase == "A");
        }

        [Test]
        public void BeamCantilever_Stresses_SigXX_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            var sigTargetMin = -fA * length / crosec.Wely_z_pos - fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetMax = fA * length / crosec.Wely_z_pos + fA * length / crosec.Welz_y_pos - fA / crosec.A;

            // Beam stresses at support of cantilever
            //--
            BeamStresses.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/SigXX/max",
                new List<double> { 0.0 },
                out var beamStresses,
                out var governingLoadCasesStresses);

            Assert.That(beamStresses[0][0][0], Is.EqualTo(sigTargetMax).Within(1e-8));
            var loadCase = governingLoadCasesStresses[0][0][0].ToString();
            Assert.That(loadCase == "A");
        }

        [Test]
        public void BeamCantilever_Stresses_SigXX_min()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            var sigTargetMin = -fA * length / crosec.Wely_z_pos - fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetMax = fA * length / crosec.Wely_z_pos + fA * length / crosec.Welz_y_pos - fA / crosec.A;

            // Beam stresses at support of cantilever
            //--
            BeamStresses.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/SigXX/min",
                new List<double> { 0.0 },
                out var beamStresses,
                out var governingLoadCasesStresses);

            Assert.That(beamStresses[0][0][0], Is.EqualTo(sigTargetMin).Within(1e-8));

            var loadCase = governingLoadCasesStresses[0][0][0].ToString();
            Assert.That(loadCase == "A");
        }

        [Test]
        public void BeamCantilever_Stresses_all()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            var sigTargetAMin = -fA * length / crosec.Wely_z_pos - fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetAMax = fA * length / crosec.Wely_z_pos + fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetBMax = -fB * length / crosec.Wely_z_pos - fB * length / crosec.Welz_y_pos - fB / crosec.A;
            var sigTargetBMin = fB * length / crosec.Wely_z_pos + fB * length / crosec.Welz_y_pos - fB / crosec.A;
            var sigTargetCMax = -fC * length / crosec.Wely_z_pos - fC * length / crosec.Welz_y_pos - fC / crosec.A;
            var sigTargetCMin = fC * length / crosec.Wely_z_pos + fC * length / crosec.Welz_y_pos - fC / crosec.A;

            // Beam stresses at support of cantilever
            //--
            BeamStresses.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC",
                new List<double> { 0.0 },
                out var beamStresses,
                out var governingLoadCasesStresses);

            Assert.That(beamStresses[0][0][0], Is.EqualTo(sigTargetAMin).Within(1e-8));
            Assert.That(beamStresses[0][0][1], Is.EqualTo(sigTargetAMax).Within(1e-8));
            Assert.That(beamStresses[0][0][2], Is.EqualTo(sigTargetBMin).Within(1e-8));
            Assert.That(beamStresses[0][0][3], Is.EqualTo(sigTargetBMax).Within(1e-8));
            Assert.That(beamStresses[0][0][4], Is.EqualTo(sigTargetCMin).Within(1e-8));
            Assert.That(beamStresses[0][0][5], Is.EqualTo(sigTargetCMax).Within(1e-8));

            Assert.That(governingLoadCasesStresses[0][0][0].ToString() == "A");
            Assert.That(governingLoadCasesStresses[0][0][1].ToString() == "A");
            Assert.That(governingLoadCasesStresses[0][0][2].ToString() == "B");
            Assert.That(governingLoadCasesStresses[0][0][3].ToString() == "B");
            Assert.That(governingLoadCasesStresses[0][0][4].ToString() == "C");
            Assert.That(governingLoadCasesStresses[0][0][5].ToString() == "C");
        }

        [Test]
        public void BeamCantilever_Stresses_envelop()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            var sigTargetAMin = -fA * length / crosec.Wely_z_pos - fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetAMax = fA * length / crosec.Wely_z_pos + fA * length / crosec.Welz_y_pos - fA / crosec.A;
            var sigTargetBMax = -fB * length / crosec.Wely_z_pos - fB * length / crosec.Welz_y_pos - fB / crosec.A;
            var sigTargetBMin = fB * length / crosec.Wely_z_pos + fB * length / crosec.Welz_y_pos - fB / crosec.A;
            var sigTargetCMax = -fC * length / crosec.Wely_z_pos - fC * length / crosec.Welz_y_pos - fC / crosec.A;
            var sigTargetCMin = fC * length / crosec.Wely_z_pos + fC * length / crosec.Welz_y_pos - fC / crosec.A;

            // Beam stresses at support of cantilever
            //--
            BeamStresses.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/min/max",
                new List<double> { 0.0 },
                out var beamStresses,
                out var governingLoadCasesStresses);

            Assert.That(beamStresses[0][0][0], Is.EqualTo(sigTargetAMin).Within(1e-8));
            Assert.That(beamStresses[0][0][1], Is.EqualTo(sigTargetAMax).Within(1e-8));
            Assert.That(governingLoadCasesStresses[0][0][0] == null);
            Assert.That(governingLoadCasesStresses[0][0][1] == null);
        }

        [Test]
        public void BeamCantilever_ElasticEnergy()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Axial and bending reactions
            //---
            var EA = crosec.A * crosec.material.E();
            var EIy = crosec.Iyy * crosec.material.E();
            var EIz = crosec.Izz * crosec.material.E();
            var engA_Axial = 0.5 * fA * fA * length / EA;
            var engB_Axial = 0.5 * fB * fB * length / EA;
            var engC_Axial = 0.5 * fC * fC * length / EA;
            var engA_Bend = fA * fA * Math.Pow(length, 3) * (1.0 / (6 * EIy) + 1.0 / (6 * EIz));
            var engB_Bend = fB * fB * Math.Pow(length, 3) * (1.0 / (6 * EIy) + 1.0 / (6 * EIz));
            var engC_Bend = fC * fC * Math.Pow(length, 3) * (1.0 / (6 * EIy) + 1.0 / (6 * EIz));

            DeformEnergy.solve(
                outModel,
                "LCC",
                new List<string>() { "B1" },
                new List<Guid>(),
                3,
                out var axialDeformationEnergy,
                out var axialDeformationEnergyInds,
                out var bendingDeformationEnergy,
                out var bendingDeformationEnergyInds,
                out var elementIndices);

            Assert.That(axialDeformationEnergy[0][0].Count, Is.EqualTo(3));
            Assert.That(axialDeformationEnergy[0][0][0], Is.EqualTo(engA_Axial).Within(1e-8));
            Assert.That(axialDeformationEnergy[0][0][1], Is.EqualTo(engB_Axial).Within(1e-8));
            Assert.That(axialDeformationEnergy[0][0][2], Is.EqualTo(engC_Axial).Within(1e-8));
            Assert.That(axialDeformationEnergyInds[0][0].Count, Is.EqualTo(3));
            Assert.That(axialDeformationEnergyInds[0][0][0], Is.EqualTo(0));
            Assert.That(axialDeformationEnergyInds[0][0][1], Is.EqualTo(1));
            Assert.That(axialDeformationEnergyInds[0][0][2], Is.EqualTo(2));
            Assert.That(bendingDeformationEnergy[0][0].Count, Is.EqualTo(3));
            Assert.That(bendingDeformationEnergy[0][0][0], Is.EqualTo(engA_Bend).Within(1e-8));
            Assert.That(bendingDeformationEnergy[0][0][1], Is.EqualTo(engB_Bend).Within(1e-8));
            Assert.That(bendingDeformationEnergy[0][0][2], Is.EqualTo(engC_Bend).Within(1e-8));
            Assert.That(bendingDeformationEnergyInds[0][0].Count, Is.EqualTo(3));
            Assert.That(bendingDeformationEnergyInds[0][0][0], Is.EqualTo(0));
            Assert.That(bendingDeformationEnergyInds[0][0][1], Is.EqualTo(1));
            Assert.That(bendingDeformationEnergyInds[0][0][2], Is.EqualTo(2));
        }

        [Test]
        public void BeamCantilever_SupportReactions_Indexed()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Axial and bending reactions
            //---
            Reaction.solve(
                outModel,
                "LCC/0",
                new List<int>() { 0 },
                out var orientations,
                out var forces,
                out var moments,
                out var sumForces,
                out var sumMoments,
                out _,
                out var governingLoadCaseInds);

            Assert.That(forces[0].Count, Is.EqualTo(1));
            Assert.That(forces[0][0].X, Is.EqualTo(fA).Within(1e-8));
            Assert.That(forces[0][0].Z, Is.EqualTo(-fA).Within(1e-8));
        }

        [Test]
        public void BeamCantilever_SupportReactions_all()
        {
            var outModel = CreateAndSolveModel(
                out var fA,
                out var fB,
                out var fC,
                out var length,
                out CroSec_Trapezoid crosec);

            Reaction.solve(
                outModel,
                "LCC",
                new List<int> { 0 },
                out var orientations,
                out var forces_all,
                out var moments_all,
                out var sum_forces_all,
                out var sum_moments_all,
                out var governingLoadCases,
                out var governingLoadCaseInds);

            Assert.That(forces_all.Count, Is.EqualTo(1));
            Assert.That(forces_all[0].Count, Is.EqualTo(3));
            Assert.That(forces_all[0][0].Z, Is.EqualTo(-10).Within(1e-8));
            Assert.That(forces_all[0][1].Z, Is.EqualTo(5).Within(1e-8));
            Assert.That(forces_all[0][2].Z, Is.EqualTo(2.5).Within(1e-8));
        }

        [Test]
        public void BeamCantilever_SupportReactions_Fz_min()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Support reactions
            //---
            Reaction.solve(
                outModel,
                "LCC/S-Fz/min",
                new List<int> { 0 },
                out var orientations,
                out var forces_all,
                out var moments_all,
                out var sum_forces_all,
                out var sum_moments_all,
                out var governingLoadCases,
                out var governingLoadCaseInds);

            var rzMin = forces_all[0][0].Z;
            Assert.That(rzMin, Is.EqualTo(-fA).Within(1e-8));
            var mzMin = moments_all[0][0].Y;
            Assert.That(mzMin, Is.EqualTo(fA * length).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "A");
        }

        [Test]
        public void BeamCantilever_SupportReactions_Fz_min_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Support reactions
            //---
            Reaction.solve(
                outModel,
                "LCC/S-Fz/min/max",
                new List<int> { 0 },
                out var orientations,
                out var forces_all,
                out var moments_all,
                out var sum_forces_all,
                out var sum_moments_all,
                out var governingLoadCases,
                out var governingLoadCaseInds);

            var rzMin = forces_all[0][0].Z;
            Assert.That(rzMin, Is.EqualTo(-fA).Within(1e-8));
            var mzMin = moments_all[0][0].Y;
            Assert.That(mzMin, Is.EqualTo(fA * length).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "A");

            var rzMax = forces_all[0][1].Z;
            Assert.That(rzMax, Is.EqualTo(-fB).Within(1e-8));
            var mzMax = moments_all[0][1].Y;
            Assert.That(mzMax, Is.EqualTo(fB * length).Within(1e-8));
            loadCase = governingLoadCases[0][1].ToString();
            Assert.That(loadCase == "B");
        }

        [Test]
        public void BeamCantilever_SupportReactions_Mx_min()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Support reactions
            //---
            Reaction.solve(
                outModel,
                "LCC/S-Mx/min",
                new List<int> { 0 },
                out var orientations,
                out var forces_all,
                out var moments_all,
                out var sum_forces_all,
                out var sum_moments_all,
                out var governingLoadCases,
                out var governingLoadCaseInds);

            // in case of multiple optimum values (Mt == 0 for all load-cases) the minimum or maximum hull is returned
            var rzMin = forces_all[0][0].Z;
            Assert.That(rzMin, Is.EqualTo(-fA).Within(1e-8));
            var mzMin = moments_all[0][0].Y;
            Assert.That(mzMin, Is.EqualTo(fB * length).Within(1e-8));
            Assert.That(governingLoadCases[0][0] == null);
        }

        [Test]
        public void BeamCantilever_NodalDisplacements_TransZ_min()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            NodeDisplacements.solve(
                outModel,
                "LCC/TransZ/min",
                new List<int> { 1 },
                out var translation,
                out var rotation,
                out var governingLoadCases,
                out var _,
                out var _);

            var uzMin = translation[0][0].Z;
            Assert.That(uzMin, Is.EqualTo(uzB).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "B");
        }

        [Test]
        public void BeamCantilever_NodalDisplacements_TransZ_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            // Nodal Displacement at tip of cantilever
            //--
            NodeDisplacements.solve(
                outModel,
                "LCC/TransZ/max",
                new List<int> { 1 },
                out var translation,
                out var rotation,
                out var governingLoadCases,
                out var _,
                out var _);

            var uzMin = translation[0][0].Z;
            Assert.That(uzMin, Is.EqualTo(uzA).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "A");
        }

        [Test]
        public void BeamCantilever_NodeDisplacements_all()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            NodeDisplacements.solve(
                outModel,
                "LCC",
                new List<int> { 1 },
                out var elemTranslations,
                out var elemRotations,
                out var elemGoverningLoadCases,
                out var _,
                out _);

            Assert.That(elemTranslations[0].Count, Is.EqualTo(3));
            var uz0 = elemTranslations[0][0].Z;
            Assert.That(uz0, Is.EqualTo(uzA).Within(1e-8));
            var uz1 = elemTranslations[0][1].Z;
            Assert.That(uz1, Is.EqualTo(uzB).Within(1e-8));
            var uz2 = elemTranslations[0][2].Z;
            Assert.That(uz2, Is.EqualTo(uzC).Within(1e-8));

            // there are multiple load cases that make up the displacement/rotation vectors
            Assert.That(elemGoverningLoadCases[0][0].ToString() == "A");
            Assert.That(elemGoverningLoadCases[0][1].ToString() == "B");
            Assert.That(elemGoverningLoadCases[0][2].ToString() == "C");
        }

        [Test]
        public void BeamCantilever_NodeDisplacements_envelope()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            NodeDisplacements.solve(
                outModel,
                "LCC/min/max",
                new List<int> { 1 },
                out var elemTranslations,
                out var elemRotations,
                out var elemGoverningLoadCases,
                out var _,
                out _);

            var uzMin = elemTranslations[0][0].Z;
            Assert.That(uzMin, Is.EqualTo(uzB).Within(1e-8));
            var uxMin = elemTranslations[0][0].X;
            Assert.That(uxMin, Is.EqualTo(uxA).Within(1e-8));
            var uzMax = elemTranslations[0][1].Z;
            Assert.That(uzMax, Is.EqualTo(uzA).Within(1e-8));
            var uxMax = elemTranslations[0][1].X;
            Assert.That(uxMax, Is.EqualTo(uxB).Within(1e-8));

            // there are multiple load cases that make up the displacement/rotation vectors
            Assert.That(elemGoverningLoadCases[0][0] == null);
            Assert.That(elemGoverningLoadCases[0][1] == null);
        }

        [Test]
        public void BeamCantilever_NodalDisplacements_Trans_min_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            // Nodal Displacement at tip of cantilever
            //--
            NodeDisplacements.solve(
                outModel,
                "LCC/Trans/min/max",
                new List<int> { 1 },
                out var translation,
                out var rotation,
                out var governingLoadCases,
                out var _,
                out _);

            var uMin = translation[0][0].Z;
            Assert.That(uMin, Is.EqualTo(uzC).Within(1e-8));
            var loadCase = governingLoadCases[0][0].ToString();
            Assert.That(loadCase == "C");

            var uMax = translation[0][1].Z;
            Assert.That(uMax, Is.EqualTo(uzA).Within(1e-8));
            loadCase = governingLoadCases[0][1].ToString();
            Assert.That(loadCase == "A");
        }

        [Test]
        public void BeamCantilever_BeamDisplacements_TransZ_min_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            // Nodal Displacement at tip of cantilever
            //--
            BeamDisplacements.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/TransZ/min/max",
                new List<double> { 1.0 },
                out var elemTranslations,
                out var elemRotations,
                out var elemGoverningLoadCases,
                out var elemGoverningLoadCaseInds,
                out _);

            var uzMin = elemTranslations[0][0][0].Z;
            Assert.That(uzMin, Is.EqualTo(uzB).Within(1e-8));
            var uxMin = elemTranslations[0][0][0].X;
            Assert.That(uxMin, Is.EqualTo(uxB).Within(1e-8));
            var uzMax = elemTranslations[0][0][1].Z;
            Assert.That(uzMax, Is.EqualTo(uzA).Within(1e-8));
            var uxMax = elemTranslations[0][0][1].X;
            Assert.That(uxMax, Is.EqualTo(uxA).Within(1e-8));

            var loadCaseInd = elemGoverningLoadCaseInds[0][0][0];
            Assert.That(loadCaseInd == 1);
            loadCaseInd = elemGoverningLoadCaseInds[0][0][1];
            Assert.That(loadCaseInd == 0);
        }

        [Test]
        public void BeamCantilever_BeamDisplacements_envelope()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            BeamDisplacements.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/min/max",
                new List<double> { 1.0 },
                out var elemTranslations,
                out var elemRotations,
                out var elemGoverningLoadCases,
                out var elemGoverningLoadCaseInds,
                out _);

            var uzMin = elemTranslations[0][0][0].Z;
            Assert.That(uzMin, Is.EqualTo(uzB).Within(1e-8));
            var uxMin = elemTranslations[0][0][0].X;
            Assert.That(uxMin, Is.EqualTo(uxA).Within(1e-8));
            var uzMax = elemTranslations[0][0][1].Z;
            Assert.That(uzMax, Is.EqualTo(uzA).Within(1e-8));
            var uxMax = elemTranslations[0][0][1].X;
            Assert.That(uxMax, Is.EqualTo(uxB).Within(1e-8));

            // there are multiple load cases that make up the displacement/rotation vectors
            Assert.That(elemGoverningLoadCases[0][0][0] == null);
            Assert.That(elemGoverningLoadCases[0][0][1] == null);
        }

        [Test]
        public void BeamCantilever_BeamDisplacements_all()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);
            var uzFak = Math.Pow(length, 3) / 3.0 / crosec.material.E() / crosec.Iyy;
            var uzA = fA * uzFak;
            var uzB = fB * uzFak;
            var uzC = fC * uzFak;
            var uxFak = -length / crosec.material.E() / crosec.A;
            var uxA = fA * uxFak;
            var uxB = fB * uxFak;
            var uxC = fC * uxFak;

            BeamDisplacements.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC",
                new List<double> { 1.0 },
                out var elemTranslations,
                out var elemRotations,
                out var elemGoverningLoadCases,
                out var elemGoverningLoadCaseInds,
                out _);

            Assert.That(elemTranslations[0][0].Count, Is.EqualTo(3));
            var uz0 = elemTranslations[0][0][0].Z;
            Assert.That(uz0, Is.EqualTo(uzA).Within(1e-8));
            var uz1 = elemTranslations[0][0][1].Z;
            Assert.That(uz1, Is.EqualTo(uzB).Within(1e-8));
            var uz2 = elemTranslations[0][0][2].Z;
            Assert.That(uz2, Is.EqualTo(uzC).Within(1e-8));

            // there are multiple load cases that make up the displacement/rotation vectors
            Assert.That(elemGoverningLoadCases[0][0][0].ToString() == "A");
            Assert.That(elemGoverningLoadCases[0][0][1].ToString() == "B");
            Assert.That(elemGoverningLoadCases[0][0][2].ToString() == "C");
        }

        [Test]
        public void BeamCantilever_BeamForces_My_min_max()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Beam Bending moment at support of cantilever
            //--
            BeamForces.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/My/min/max",
                new List<double> { 0.0 },
                out List<List<List<Vector3>>> beamForces,
                out List<List<List<Vector3>>> beamMoments,
                out List<List<List<Karamba.Loads.Combination.LoadCase>>> beamGoverningLoadCases,
                out List<List<List<int>>> beamGoverningLoadCaseInds,
                out List<int> elemInds);

            var m = beamMoments[0][0][0];
            var myTarget = -fA * length;
            Assert.That(m.Y, Is.EqualTo(myTarget).Within(1e-8));

            var loadCase = beamGoverningLoadCases[0][0][0].ToString();
            Assert.That(loadCase == "A");

            BeamForces.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/max",
                new List<double> { 0.0 },
                out beamForces,
                out beamMoments,
                out beamGoverningLoadCases,
                out beamGoverningLoadCaseInds,
                out elemInds);

            m = beamMoments[0][0][0];
            myTarget = -fB * length;
            Assert.That(m.Y, Is.EqualTo(myTarget).Within(1e-8));

            // there are multiple governing load cases that make up the force vector
            Assert.That(beamGoverningLoadCases[0][0][0] == null);
        }

        [Test]
        public void BeamCantilever_BeamForces_all()
        {
            var outModel = CreateAndSolveModel(out var fA, out var fB, out var fC, out var length, out CroSec_Trapezoid crosec);

            // Beam Bending moment at support of cantilever
            //--
            BeamForces.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC",
                new List<double> { 0.0 },
                out List<List<List<Vector3>>> beamForces,
                out List<List<List<Vector3>>> beamMoments,
                out List<List<List<Karamba.Loads.Combination.LoadCase>>> beamGoverningLoadCases,
                out List<List<List<int>>> beamGoverningLoadCaseInds,
                out List<int> elemInds);

            var myA = -fA * length;
            var myB = -fB * length;
            var myC = -fC * length;

            Assert.That(beamMoments[0][0][0].Y, Is.EqualTo(myA).Within(1e-8));
            Assert.That(beamMoments[0][0][0].Z, Is.EqualTo(-myA).Within(1e-8));
            Assert.That(beamMoments[0][0][1].Y, Is.EqualTo(myB).Within(1e-8));
            Assert.That(beamMoments[0][0][1].Z, Is.EqualTo(-myB).Within(1e-8));
            Assert.That(beamMoments[0][0][2].Y, Is.EqualTo(myC).Within(1e-8));
            Assert.That(beamMoments[0][0][2].Z, Is.EqualTo(-myC).Within(1e-8));

            Assert.That(beamGoverningLoadCases[0][0][0].ToString() == "A");
            Assert.That(beamGoverningLoadCases[0][0][1].ToString() == "B");
            Assert.That(beamGoverningLoadCases[0][0][2].ToString() == "C");
        }

        [Test]
        public void BeamCantilever_BeamForces_envelop()
        {
            var outModel = CreateAndSolveModel(
                out var fA,
                out var fB,
                out var fC,
                out var length,
                out CroSec_Trapezoid crosec);

            // Beam Bending moment at support of cantilever
            //--
            BeamForces.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/min/max",
                new List<double> { 0.0 },
                out List<List<List<Vector3>>> beamForces,
                out List<List<List<Vector3>>> beamMoments,
                out List<List<List<Karamba.Loads.Combination.LoadCase>>> beamGoverningLoadCases,
                out List<List<List<int>>> beamGoverningLoadCaseInds,
                out List<int> elemInds);

            var MyA = -fA * length;
            var MyB = -fB * length;

            Assert.That(beamMoments[0][0][0].Y, Is.EqualTo(MyA).Within(1e-8));
            Assert.That(beamMoments[0][0][0].Z, Is.EqualTo(-MyB).Within(1e-8));
            Assert.That(beamMoments[0][0][1].Y, Is.EqualTo(MyB).Within(1e-8));
            Assert.That(beamMoments[0][0][1].Z, Is.EqualTo(-MyA).Within(1e-8));

            Assert.That(beamGoverningLoadCases[0][0][0] == null);
            Assert.That(beamGoverningLoadCases[0][0][1] == null);
        }

        [Test]
        public void BeamCantilever_BeamForces_envelop_max()
        {
            var outModel = CreateAndSolveModel(
                out var fA,
                out var fB,
                out var fC,
                out var length,
                out CroSec_Trapezoid crosec);

            // Beam Bending moment at support of cantilever
            //--
            BeamForces.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC/max",
                new List<double> { 0.0 },
                out List<List<List<Vector3>>> beamForces,
                out List<List<List<Vector3>>> beamMoments,
                out List<List<List<Karamba.Loads.Combination.LoadCase>>> beamGoverningLoadCases,
                out List<List<List<int>>> beamGoverningLoadCaseInds,
                out List<int> elemInds);

            var myA = -fA * length;
            var myB = -fB * length;

            Assert.Multiple(() => {
                Assert.That(beamMoments[0][0][0].Y, Is.EqualTo(myB).Within(1e-8));
                Assert.That(beamMoments[0][0][0].Z, Is.EqualTo(-myA).Within(1e-8));
                Assert.That(beamGoverningLoadCases[0][0][0] == null);
            });
        }

        [Test]
        public void BeamCantilever_Utilization_all()
        {
            Helper.InitIniConfigTest(UnitSystem.SI, false);
            var outModel = CreateAndSolveModel(
                out var fA,
                out var fB,
                out var fC,
                out var length,
                out CroSec_Trapezoid crosec);

            Utilization_Beam.solve(
                outModel,
                new List<string> { "0" },
                new List<Guid>(),
                "LCC",
                3,
                true,
                1,
                1,
                false,
                false,
                out var modelUtils,
                out var modelUtilsLCInds,
                out var msg,
                out var elemInds);

            Assert.That(modelUtils[0][0].util, Is.EqualTo(0.78021323584319591).Within(1e-8));
        }

        // */
    }
}

#endif
