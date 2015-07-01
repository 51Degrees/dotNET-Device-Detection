![51Degrees](https://51degrees.com/Portals/0/Logo.png "THE Fasstest and Most Accurate Device Detection")

# Device Detection for Microsoft .NET

[Recent Changes](#recent-changes "Review recent major changes") | [.NET Developer Documation](https://51degrees.com/support/documentation/net "Full getting started guide and advanced developer documentation") | [Device Databases](https://51degrees.com/compare-data-options "Different device databases which can be used with 51Degrees device detection")

## What is Device Detection?

Use code like...

```
Request.Browser["IsMobile"]
```

or 

```
Request.Browser["IsTablet"]
```

... from within a web application to determine the type of device requesting the web page server side.

Include...

```
C# https://[YOUR DOMAIN]/51Degrees.features.js?DeviceType&ScreenInchesDiagonal
```

... from Javascript to retrieve device type and physcial screen size information. Use Google Analytics custom dimensions to add this data for more granular analysis.

Use...

```
var detectionProvider = new Provider(StreamFactory.Create("[DATA FILE LOCATION]"));
```

... to use device detection offline to analyse web log files by User-Agent headers.

## What do I need?

The simplest method of deploying 51Degrees device detection to a .NET project is with NuGet. Just search for [51Degrees on NuGet](https://www.nuget.org/packages?q=51degrees "51Degrees Packages on NuGet").

This GitHub repository and NuGet include 51Degrees free Lite device database. The Lite data is updated monthly by our professional team of analysts. 

Data files which are updated weekly and daily, automatically, and with more properties and device combinationsare also available.

[Compare Device Databases](https://51degrees.com/compare-data-options "Compare different data file options for 51Degrees device detection")

## Recent Changes

### Version 3.2.1 Highlights

This release focuses on reducing memory consumption and improving performance when the device data file is used directly from the disk.

**Important Change: The embedded device data has been removed from the assembly and by default placed in the App_Data folder for both web and non-web projects. The solution will not work without the associated data file being provided and the WebProvider.ActiveProvider property can now return null.
</div>

### Major Changes

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
* 51Degrees unit tests are now part of the open source distribution.

### Changes from 3.1.13

* The entity RankedSignatureIndex has been renamed to IntegerEntity along with the associated factories. This is so that the entity can be reused in the new lists for Nodes related to Signatures and Signatures related to Nodes where each list also contains 4 byte integer data types.
* A potential threading problem has been resolved in Profile entity by only referencing the property PropertyIndexToValues rather than its backed private field.
* Cache service method thread start is now synchronised.
* Memory/Profile.cs Init() method has been removed as the ValueIndexes and SignatureIndexes arrays are needed to support other methods and don’t need to be freed.
* Changed the Cache classes AddRecent and ServiceCache methods to prevent multiple service operations in multiple threads.
* Added a ResetCache method to the dataset.
* WebProvider in memory mode now uses a byte array in memory rather than constructing all instances of every entity. This reduces start up time.
* Unit tests have been added for performance, memory and major data error checks.
* V3.2 data format is now supported.
