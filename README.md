# README #

### What is this repository for? ###

* This repo contains the Pelco Event Injector. 
* The Event Injector is used to inject events provided by an Event Agent (Event Injector Plug-in) into VideoXpert.
* The Event Injector utilizes the [VxSdk](http://pdn.pelco.com/content/videoxpert-sdk) and the [C# Wrapper](https://github.com/pelcointegrations/VxSDK-Samples/tree/master/CSharpSample/CSharpWrapper) to communicate with VideoXpert.
* Latest Version: [2.0.0.0](https://schneider-electric.box.com/s/d37c4b4wcdx7nywdvoewb2zh4g5e6tu9)

### How do I install the latest build? ###

* Install .NET 4.6.2
* Install Visual Studio 2013 x86/x64 C++ runtime
* Install Visual Studio 2015 x86/x64 C++ runtime
* Install the Microsoft Message Queue (MSMQ) Server Core
* No need to install...
* ...MSMQ Active Directory Domain Services Integration
* ...MSMQ HTTP Support
* ...MSMQ Triggers
* ...Multicasting Support
* Download the latest release by clicking on the "Latest Version" above
* Go through the installation wizard

### How do I build the repo ###

* Install .NET 4.6.2
* Install Visual Studio 2013 x86/x64 C++ runtime
* Install Wix
* Install Visual Studio 2015
* Clone the repo
* Build VxEventInjector.sln
