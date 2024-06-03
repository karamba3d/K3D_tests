![](banner.jpg?raw=true "Logo")

Karamba3D_tests
===============

These are unit-tests for Karamba3D. 

Deployment
----------

In order to run the tests in **Visual Studio** do the following:
* Download this repository, unpack and open Karamba3D_tests.sln with Visual Studio
* Rebuild the solution.
* The license that comes as part of the Karamba3D nuget-package is a Trial-license. In case you want to use your Cloud-license start Rhino and execute 'Karamba3DGetLicense' in the Rhino command window before starting the tests. 
* Select Test/Test Explorer from the menu, run the tests

In order to run the tests in **Visual Studio Code** do the following:
* Open the folder 'KarambaCommon_tests'.
* In the Terminal execute 'dotnet build' or 'dotnet test' to build or test.


Special notice on handling licenses
-----------------------------------
For executing the tests it is sufficient to use a trial license. The license file is searched for in the folder 'KarambaCommon_tests\bin\Debug\net7.0-windows\win-x64\License' by default. So the license folder needs to
be placed alongside 'KarambaCommon.dll' like in the Karamba installation folder. If the 'License'-folder can not be found the license type defaults to 'trial'. In case you want to create tests
which go beyond the limits of the trial-version copy your license.lic-file from the Karamba installation folder to the corresponding location in the bin-folder of the tests.

If you want to use your Zoo-server or Zoo cloud license for testing you have to start Rhino and acquire the license via the 'Karamba3DGetLicense'-command, then run the tests.

Contributing
------------

Your contributions in the form of code, suggestions, feature requests or critique are highly welcome!

Please read [CONTRIBUTING.md](https://github.com/karamba3d/K3D_tests/blob/master/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

Authors
-------

* Clemens Preisinger
* Enrico Antolini

See also list of contributors who participated in this project

License
-------

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/karamba3d/K3D_tests/blob/master/LICENSE.md) file for details

What's new - 03/06/2024
-----------------------
* The project file has been updated to the new SDK-style.
* The tests link to version 3.1.40531 of Karamba3D.
* Many tests were added.

