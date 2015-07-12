<p>
<img src="https://51degrees.com/DesktopModules/FiftyOne/Distributor/Logo.ashx" title="THE Fastest and Most Accurate Device Detection"/>**Device Detection Unit Tests**
</p>

**Important:** Unit tests are provided for both code and data files. Enterprise and Premium data files needed to be placed in the Data folder of this repository in order for all tests to be executed. These files can be obtained from 51Degrees.

**[Get Premium and Enterprise Data](https://51degrees.com/compare-data-options?utm_source=github&utm_medium=repository&utm_content=unit-tests&utm_campaign=net-open-source "Different device databases which can be used with 51Degrees device detection")**

## Unit Test Categories

**Memory:** monitor memory consumption during device matching reporting on results compared to maximum tolerances. Uses GC.GetTotalMemory method to monitor memory so is imprecise and no substitute for using Visual Studio's profiler.

**Performance:** executeds tests both in single threaded and multithreaded operation against the dataset reporting on average detection times in different scenarios compared to maximum tolerances.

**API:** tests the API with limited good and bad data. Used to gross error checking.

**HTTP Headers:** uses multiple HTTP headers used by browsers such as Opera Mini to identify the device.

**Meta Data:** retrieves all meta data from the data set reporting any exceptions.