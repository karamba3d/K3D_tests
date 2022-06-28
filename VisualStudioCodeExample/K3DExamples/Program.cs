// ############################################
//
// In order to make the example run:
// 1.) execute 'dotnet add package KarambaCommonForVSCode' on the VSCode console.
// 2.) build the project using 'dotnet build' or 'dotnet build -c release'
// 3.) copy the contents of the folder 
//     %userprofile%\.nuget\packages\karambacommonforvscode\2.2.0.142\contentFile\any\any
//     to the project's 'bin\Debug\net6.0' ot 'bin\Release\net6.0'-folder.
// 4.) run the program using 'dotnet run' or 'dotnet run -c release'
//     You should see the expected and calculated displacement in meter of the cantilever beam.
//
// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//
// Please contact me in case you know how
// to make the nuget-package automatically
// copy the files.
//
// ############################################

using Karamba.CrossSections;
using Karamba.Geometry;
using Karamba.Loads;
using Karamba.Materials;
using Karamba.Supports;
using Karamba.Utilities;
using KarambaCommon;

var k3d = new Toolkit();
var logger = new MessageLogger();
double length = 4.0;
var p0 = new Point3(0, 0, 0);
var p1 = new Point3(length, 0, 0);
var axis = new Line3(p0, p1);

// get a cross section from the cross section table in the folder 'Resources'
var crosec = new CroSec_Trapezoid(
    "family",
    "name",
    "country",
    null,
    Material_Default.Instance().steel,
    20,
    10,
    10);
    
crosec.Az = 1E10; // make it stiff in shear

// create the cantilever beam
var beams = k3d.Part.LineToBeam(
new List<Line3> { axis },
new List<string>() { "B1" },
new List<CroSec>() { crosec },
logger,
out var out_points);

// create supports
var supports = new List<Support>
{
    k3d.Support.Support(p0, new List<bool>() { true, true, true, true, true, true }),
};

// create a Point-load
const int fz = 10;
var loads = new List<Load> { k3d.Load.PointLoad(p1, new Vector3(0, 0, -fz)), };

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

// calculate Th.I response
model = k3d.Algorithms.AnalyzeThI(
    model,
    out var out_max_disp,
    out var out_g,
    out var out_comp,
    out message);

var mE = crosec.material.E();
var cI = crosec.Iyy;
var maxDispTarg = fz * Math.Pow(length, 3) / 3 / mE / cI;

Console.WriteLine("Target Value    :" + maxDispTarg);
Console.WriteLine("Calculated Value:" + out_max_disp[0]);
