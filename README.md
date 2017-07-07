## Introduction

The CityScape Spectrum Observatory interface (http://cityscape.cloudapp.net) is an enhanced version of the original interface (http://observatory.microsoftspectrum.com), funded by NSF, the goal is to produce a research-grade Spectrum Monitoring infrastructure. Led by U. Washington and Shared Spectrum Co. with continuing support from Microsoft; it seeks to provide a persistent, metro-scale distributed RF spectrum monitoring infrastructure using commodity Software Defined Radios such as USRPs, in conjunction with a robust web interface to enable acquisition of reliable I-Q and spectrum usage information. The stations continually upload RF sensor data (using current instrument settings) that is stored on public cloud such as Microsoft Azure. Station data - both time-averaged power spectrum density (p.s.d) charts (past 2 years) and raw I-Q (past week) is available for viewing/download by the spectrum engineering research community.

Source codes of the project are published under Apache 2.0 license.

## Frequently Asked Questions (FAQs):


**Q. How can I contribute to CityScape spectrum observatory?**

Below are the ways you can contribute to CityScape Spectrum Observatory.
1. SETUP YOUR OWN STATION. By registering a new station at your location and configure it to upload the spectrum data to the Microsoft Azure cloud so the data can be made available to public for analysis.
2. RESEARCH AND ANALYSIS. Be a part of the community of like minded individuals by sharing information and serve the communitys research more better 
3. IDEAS AND FEEDBACK. Your feedback is very important for us to make things a little better and improve the research and analysis. Your active contribution with our team always helps us to find ways and improve.
4. CREATE NEW FEATURES. Contribute to CityScape Spectrum Observatory open sources by implementing new features, onboarding variety of RF sensors, sharing the issues and fixing bugs

**Q. What is included in the open source code base?**

The code base includes all of the CityScape Spectrum Observatory source code modules required to build the software for measurement stations, data processing and the web site. For complete details, please read through the see the source code and it corresponding documentation.

**Q. How do I contribute to code?**

You can contribute to code including features, bug fixes and tests. Please refer to our documentation on contributing to spectrum observatory software to learn on how to get started.  We laid out few simple coding guidelines that would help to have a similar style across the modules.

**Q. What features can be contributed to the sources?**

We encouage any feature that would make the spectrum observatory a more valuable resouce. We will do regular updates to the project, so any new feature you create can be leveraged by the entire community.

**Q. What are the system requirements for coding the CityScape Spectrum Observatory?**

Windows 8.1 or later PC

**Q. What Devices (RF Sensors) are currently supported?**

Below are the devices that we have tested with the current release of the CityScape Spectrum Observatory:
1. USRP N200 & N210: http://www.ettus.com/product/category/USRP-Networked-Series
2. USRP N200 with UBX40 daughterboard

Other USRP sensors should work as well, but have not been tested. Also, adding new RF sensors is a simple process the requires only creating an interface between the existing scanner and the RF sensor that you would like to support.

**Q. What is a Microsoft account and why do I need it?**

You can find more information on a Microsoft account here: http://www.microsoft.com/en-us/account/default.aspx.  You need to register with the CiytScape Spectrum Observatory site to access services like "Register a New Station"..

**Q. What software required to build and write code for cityscape spectrum observatory?**

You would need the following software:
1. Visual Studio 2013 or above
2. Azure 2.3 SDK
3. WiX Installer 3.7 or above

**Q. Any special instructions to build the code?**

Before proceeding with building of the code, let us explain few pre-requisties for successful build of the code.
1. Create a nuget package using package manager console from visual studio for the 
build, please download and install the Microsoft Azure 2.3 SDK and WiX installer. Once you open the spectrum observatory solution in the visual studio, the first build will initiate downloading the nuget packages and asks to build it once more time. Subsequently builds should go fine and you can start debugging the code locally. [internal nuget package instructions, live sdk, other settings changes, certificate]
