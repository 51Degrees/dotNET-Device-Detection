![51Degrees](https://51degrees.com/Portals/0/Logo.png "THE Fasstest and Most Accurate Device Detection")

# Device Detection for Microsoft .NET

## Version 3.2.1 Highlights

The design focus of the release is to reduce memory consumption when the data file used directly from the disk, and to improve performance.

Important Change: The embedded device data has been removed from the assembly and by default placed in the App_Data folder for both web and non-web projects. The solution will not work without the associated data file being provided and the WebProvider.ActiveProvider property can now return null.

* Embedded data has been removed from the assembly and now must be provided from the App_Data folder.
* .NET 3.5 is not supported in this release in order to use memory mapped files and simplify overriding default browser capabilities.
* In stream mode entity data properties that can allocate large arrays only initialise these arrays when needed.
* Caches used with stream operation are now fixed memory size and serviced via the thread pool.
* Automatic update processes uses temporary files rather than main memory to verify integrity of updated files prior to using them.
* Temporary files are now created in the App_Data/51Degrees folder of the web application rather than a UNC path or the master data file folder.
* Values associated with Profiles are now retrieved using a more efficient algorithm.
* DataSet.Properties collection now has a string accesser to make retrieving properties by name simpler.
* Web sites using memory mode use a byte array to improve start up time.
* Version 3.2 data file formats are supported in parallel with version 3.1 data files.

