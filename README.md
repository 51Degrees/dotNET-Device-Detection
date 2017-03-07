![51Degrees](https://51degrees.com/DesktopModules/FiftyOne/Distributor/Logo.ashx?utm_source=github&utm_medium=repository&utm_content=home&utm_campaign=net-open-source "THE Fastest and Most Accurate Device Detection") **Device Detection for Microsoft .NET**

[Recent Changes](#recent-changes "Review recent major changes") | [Supported Databases](https://51degrees.com/compare-data-options?utm_source=github&utm_medium=repository&utm_content=home-menu&utm_campaign=net-open-source "Different device databases which can be used with 51Degrees device detection") | [.NET Developer Documention](https://51degrees.com/support/documentation/net?utm_source=github&utm_medium=repository&utm_content=home-menu&utm_campaign=net-open-source "Full getting started guide and advanced developer documentation") | [Available Properties](https://51degrees.com/resources/property-dictionary?utm_source=github&utm_medium=repository&utm_content=home-menu&utm_campaign=net-open-source "View all available properties and values")

<sup>Need [C](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for C") | [Java](https://github.com/51Degrees/Java-Device-Detection "THE Fastest and most Accurate device detection for Java") | [PHP](https://github.com/51Degrees/Device-Detection) | [Python](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for Python") | [Perl](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for Perl") | [Node.js](https://github.com/51Degrees/Device-Detection "THE Fastest and most Accurate device detection for Node.js")?</sup>

**Server Side:** Use code like...

```cs
Request.Browser["IsMobile"]
```

or 

```cs
Request.Browser["IsTablet"]
```

... from within a web application server side to determine the requesting device type.

**Client Side:** Include...

```
https://[YOUR DOMAIN]/51Degrees.features.js?DeviceType&ScreenInchesDiagonal
```

... from Javascript to retrieve device type and physcial screen size information. Use Google Analytics custom dimensions to add this data for more granular analysis.

**Offline:** Use...

```cs
var detectionProvider = new Provider(StreamFactory.Create("[DATA FILE LOCATION]"));
var deviceType = detectionProvider.Match("[YOUR USERAGENT]")["DeviceType"];
```

... to perform offline analysis of web logs with User-Agent headers.

**[Review All Properties](https://51degrees.com/resources/property-dictionary?utm_source=github&utm_medium=repository&utm_content=home-cta&utm_campaign=net-open-source "View all available properties and values")**

## What's needed?

The simplest method of deploying 51Degrees device detection to a .NET project is with NuGet. Just search for [51Degrees on NuGet](https://www.nuget.org/packages?q=51degrees "51Degrees Packages on NuGet").

This GitHub repository and NuGet include 51Degrees free Lite device database. The Lite data is updated monthly by our professional team of analysts. 

Data files which are updated weekly and daily, automatically, and with more properties and device combinationsare also available.

**[Compare Device Databases](https://51degrees.com/compare-data-options?utm_source=github&utm_medium=repository&utm_content=home-cta&utm_campaign=net-open-source "Compare different data file options for 51Degrees device detection")**

## Recent Changes

### Version 3.2.15 - February 2017

* A new fluent builder - DataSetBuilder is now the preferred method to create a DataSet instead of the StreamFactory.  
* The caching policy used by the API can now be customised as required by the application. This allows the developer to replace the 51 degrees cache entirely with their own implementation. If using the 51 degrees cache, this gives the developer the flexibility to make the decision about the memory usage / performance balance rather than having it imposed by the API. By default, the 51 degrees LRU cache will be used, with size values determined by internal testing to give good performance in a wide range of scenarios without using too much memory. Templates are available with size values more suited to specific use cases.

### Version 3.2.14 Highlights

New Lite Data File released for December.
Usage data is now shared securely over HTTPS
getValues now uses a concurrent dictionary to improve performance.

### Version 3.2.11 Highlights

New Lite Data File released for August.

### Version 3.2.6 Highlights

This release focuses on reducing memory consumption and improving performance when the device data file is used directly from the disk.

**Important Change:** _The embedded device data has been removed from the assembly and by default placed in the App_Data folder for both web and non-web projects. The solution will not work without the associated data file being provided and the WebProvider.ActiveProvider property can now return null._

### Changes from 3.2.5

Summary of API changes:

* Provider supports retrieving match results using device ids generated from previous matches.
* The classes to update device data files are now public and can be used to update device data files from non web environments.
* Licence keys are now verified against the 51Degrees public signature before being used to retrieve updates.
* The cache has been upgraded to use a least recently used (LRU) design. This removes the need to service the cache in a background thread, and results in a more predictable performance under load. 
* Duplicate code has been consolidated with a focus on improving documentation and implementing recommendations from code analysis and peer reviews. Testing coverage has been included with initial unit tests for new features.
* Consistent examples have been added in parallel with APIs in other languages. The examples are designed to highlight specific use cases for the API. They relate to example specific documentation on the 51Degrees web site under Support -> Documentation -> .NET. 
* The override to indicate if cookies are supported now defaults to True when the value is unknown. This prevents 3rd party components such as forms authentication from failing where an assumption that cookies are always supported has been made but not verified against the server side browser capabilities.
* The demo web site project no longer includes the 51Degrees.dat file in the project. It is instead copied from the repositories data folder when the project is built.
* The signed assembly is now compiled with "Optimise Code" option enabled.

### Major Changes in Version 3.2

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
