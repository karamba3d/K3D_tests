![](banner.jpg?raw=true "Logo")

Karamba3D_tests
===============

These are unit-tests for Karamba3D. 

Deployment
----------

In order to run the tests do the following:
* download and install the latest version of Karamba3D 2.0.0 from 'karamba.dll' from https://github.com/karamba3d/K3D_NightlyBuilds/releases. 
* install the 'NUnit3 Test Adapter' extension for Visual Studio
* download this repository, unpack and open Karamba3D_tests.sln with Visual Studio
* Update the nuget packages in Visual Studio by right-clicking on 'References' and choosing 'Manage NuGet Packages...'.
* In Visual Studio Right-click on KarambaCommon_tests, select 'Properties' from the drop-down-list, go to 'Debug' and set the 
  Working Directory to the Karamba3d installation folder (e.g. C:\Program Files\Rhino 6\Plug-ins\Karamba\).
* Give yourself write rights to the Karamba3d installation folder (first reset the owner, then add effective access)
* Under 'References' refresh the reference to 'karambaCommon' which is found in the Karamba3d installation folder. Make sure that 
  'Copy Local' is set to true for 'karambaCommon' - get there in Visual Studio via left-click on 'karambaCommon' and Properties.
* If you want to run tests via the 'Test Explorer' copy 'karamba.dll' from the Karamba3d installation folder to 'Karamba3D_tests'

How to run the tests:
---------------------

There are two basic methods:
* run directly from Visual Studio via Ctrl+F10 if you want to run to the cursor, or Ctrl+F5 if you want to run all the tests. In this case 'KarambaCommon_tests.exe'
  in 'Karamba3D_tests\KarambaCommon_tests\bin\x64\Debug' gets invoked. In order to work, 'karambaCommon.dll' needs to be present in that directory. This is the 
  reason why it is necessary to set 'Copy Local' to 'true' for 'KarambaCommon.dll'.

  For selecting only some tests change ' #if ALL_TESTS' to e.g. ' #if _ALL_TESTS' at those tests, and change the conditional compilation symbol 'ALL_TESTS' to 
  '_ALL_TESTS' (see Properties/Build).
  
* run tests via the 'Test Explorer' via 'Test/Test Explorer'  from the MSVC menu.
 

Contributing
------------

Please read CONTRIBUTING.md for details on our code of conduct, and the process for submitting pull requests to us.

Authors
-------

* Clemens Preisinger
* Enrico Antolini

See also list of contributors who participated in this project

License
-------

This project is licensed under the MIT License - see the LICENSE.md file for details


