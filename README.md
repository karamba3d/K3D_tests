![](banner.jpg?raw=true "Logo")

Karamba3D_tests
===============

These are unit-tests for Karamba3D. 

Deployment
----------

In order to run the tests do the following:
* Install the 'NUnit3 Test Adapter' extension for Visual Studio
* Download this repository, unpack and open Karamba3D_tests.sln with Visual Studio
* Rebuild the solution.
* The license that comes as part of this repo is a Trial-license. In case you want to use your Cloud-license start Rhino and execute 'Karamba3DGetLicense' in the Rhino command window before starting the tests. 

The folder 'VisualStudioCodeExample' contains a console application that shows how to work with KarambaCommon in **Visual Studio Code**. Read the remarks in Program.cs for details. 

How to run the tests:
---------------------

There are two basic methods:
* Run directly from Visual Studio via Ctrl+F10 if you want to run to the cursor, or Ctrl+F5 if you want to run all the tests. In this case 'KarambaCommon_tests.exe'
  in 'Karamba3D_tests\KarambaCommon_tests\bin\x64\Debug' gets invoked. 
  
  For selecting only some tests change ' #if ALL_TESTS' to e.g. ' #if _ALL_TESTS' at those tests, and change the conditional compilation symbol 'ALL_TESTS' to 
  '_ALL_TESTS' (see Properties/Build).
  
* Run tests via the 'Test Explorer' via 'Test/Test Explorer'  from the MSVC menu.
 
Special notice on handling licenses
-----------------------------------
For executing the tests it is sufficient to use a trial license. The license file is searched for in the folder 'KarambaCommon_tests\bin\x64\Debug\License\' by default. So the license folder needs to
be placed alongside 'KarambaCommon.dll' like in the Karamba installation folder. If the 'License'-folder can not be found the license type defaults to 'trial'. In case you want to create tests
which go beyond the limits of the trial-version copy your license.lic-file from the Karamba installation folder to the corresponding location in the bin-folder of the tests.

If you want to use your Zoo-server or Zoo cloud license for testing you have to start Rhino and acquire the license via the 'Karamba3DGetLicense'-command, then run the tests.

Contributing
------------

Please read [CONTRIBUTING.md](https://github.com/karamba3d/K3D_tests/blob/master/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

Authors
-------

* Clemens Preisinger
* Enrico Antolini

See also list of contributors who participated in this project

License
-------

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/karamba3d/K3D_tests/blob/master/LICENSE.md) file for details


