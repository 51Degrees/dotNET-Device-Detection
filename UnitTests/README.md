![51Degrees](https://51degrees.com/Portals/0/Logo.png "THE Fastest and Most Accurate Device Detection")**Unit Tests**

**Important:** Unit tests are provided for both code and data files. Enterprise and Premium data files needed to be placed in the Data folder of this repository in order for all tests to be executed. These files can be obtained from 51Degrees.

[Premium and Enterprise Data](https://51degrees.com/compare-data-options "Different device databases which can be used with 51Degrees device detection")

## Unit Tests

**Memory:** monitor memory consumption during device matching reporting on results compared to maximum tolerances. Uses GC.GetTotalMemory method to monitor memory so is imprecise and no substitute for using Visual Studio's profiler.

**Performance:** executeds tests both in single threaded and multithreaded operation against the dataset reporting on average detection times in different scenarios compared to maximum tolerances.

**API:** tests the API with limited good and bad data. Used to gross error checking.

**HTTP Headers:** uses multiple HTTP headers used by browsers such as Opera Mini to identify the device.

**Meta Data:** retrieves all meta data from the data set reporting any exceptions.