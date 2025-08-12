# iPlus-Framework

Welcome to the iPlus-Framework repository! This open-source project is designed to empower developers to create flexible, scalable, and efficient software solutions for the industry.
![myimage-alt-tag](https://github.com/iplus-framework/iPlus-Docs/blob/main/Images/EngineeringEnv.jpg)

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
- [What you can create with it](#what-you-can-create-with-it)
- [Technology](#Technology)
- [Getting Started](#getting-started)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)

## Introduction

The iPlus Framework is a hybrid solution that combines classical programming with a low-code platform, addressing the needs of developers and integrators who aim to create updateable, individualized software solutions. Our mission is to provide an alternative to proprietary systems, offering a platform that empowers users to maintain control and independence throughout the entire software lifecycle.

In today's fast-paced technological landscape, businesses often find themselves dependent on large corporations for software solutions, leading to a loss of autonomy. The iPlus Framework counters this trend by offering an open-source software platform that encourages decentralized collaboration and innovation. By licensing our software under the GPL, we ensure that any solutions built with our framework remain open and accessible, fostering a community-driven approach to software development.

## Features

The iPlus Framework is designed to meet the complex requirements of modern software development, particularly in industrial environments. Key features include:

- **Hybrid Programming Model:** Combines traditional coding with low-code capabilities, allowing for maximum development freedom while maintaining a consistent programming paradigm.
- **Component-Based Architecture:** Enables easy extension and customization of the framework to meet specific industry needs, supporting both transaction-based and real-time, time-critical solutions.
- **Declarative Programming:** Focuses on minimizing maintenance and increasing compatibility, ensuring that software changes and extensions can be carried out during runtime.
- **Seamless Integration:** Bridges the gap between ERP and field levels (IT/OT). Bridges the gap between database oriented programming, network programming and unified UI programming.
- **Simulation and Real-Time intervention:** Allows for the mapping and simulation of processes and object states. Advanced system diagnostics during runtime and changing object states via network.
- **Features:** Compositum pattern, network transparent programming, Remote-Properties, synchronous- and asynchronous RMI over network, Object-Persistance, Messages/Alarms, Entity framework, Virtual inheritance, UI-designer, scripting, packaging, multithreading, workflow-engine, report engine, software package management, customization...more on our website

## What you can create with it
- **SCADA:** Out of the box. With builtin PLC-Connections, OPC-UA-Client and OPC-UA-Server
- **DCS, MES:** Distributed Control Systems, Manufacturing Execution System. Use the [iPlus-MES repository](https://github.com/iplus-framework/iPlusMES) for a out of the box usage in combination with [iplus-MES-Mobile](https://github.com/iplus-framework/Mobile).
- **Custom industrial applications:** Any applications/components can be developed and provided as packages that are managed by the runtime.
- **HMI or OEM Software**: Standalone installations. Software for single board computers with linux based operating systems (armbian, raspbian).
- **ERP**: On Premise ERP-Software
- **Mobile**: Mobile Solutions.
- ...

## Technology
- Build on .NET Platform: .NET Core, Entity Framework, WCF, WPF
- Serverside without UI: Windows, Linux
- Clientside with UI: Windows. On Linux/Android currently with WINE, but in the future with Avalonia XPF (see library exception in our GPL-v3 license) also on Linux, Android, macOS, iOS and Web-Browser.

## Getting Started
To get started with the iPlus Framework, follow these steps:

1. **Clone the Repository:**  
git clone https://github.com/iplus-framework/iPlus.git

2. **Install a Microsoft SQL-Server Edition:**  
https://learn.microsoft.com/en-us/sql/database-engine/install-windows/install-sql-server?view=sql-server-ver16

3. **Restore a iplus-database file**  
Restore the SQL Server backup file located in the "Database" folder.

4. **Open project in Microsoft Visual Studio:**
  - Modify the ConnectionStrings.config file: Follow the instructions on [gipSoft's documentation site](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=193d292e-df31-405e-a3e9-f1116846bf86#ItemTextTranslationID_d0551cc7-f767-4790-8ecb-8771836ebac7)
  - Compile the solution an set gip.iplus.client.exe as start project.
  - For building ef-models follow the instructions on [EF core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
  - dotnet tool update --global dotnet-ef  (optional version e.g.: --version 10.0.0-preview.6.25358.103)

5. **Execute gip.iplus.client.exe**  
  - Login with user "superuser" and password "superuser" to **start as a client**. Login with user "00" and password "00" to **start as a service**. After starting the service, your local IP is remembered and you can start a second instance an logon as client to establish the Client-Server-Connection. If you have problems, that the Client doesn't connect with the server read the [network configuration section](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=a7424f03-cbff-4f68-90eb-262b3fdcba1c#ItemTextTranslationID_bf788469-8a2f-4a7c-ad78-2710960096dc). For [running on Linux read here](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=9df14383-a4e5-4a3d-81d5-5dccf80d6c4e).
  - *Important: Every time you pull a newer version from GitHub, you should **hold down the CTRL key** while clicking on the [login button in the login window](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=ca1a96f0-233d-4e6c-9cfc-d1db683e4be3#ItemTextTranslationID_e24a12c4-649d-4903-abf0-8366ed656ca1) so that your local databases are updated.* Otherwise, the application will not be able to start.
  - 🤗 Have fun trying it out and watch our videos to get started. However, we recommend [online training](https://iplus-framework.com/en?section=Support%20%26%20Training#d57ccb45-9050-41cb-a177-9e8c05028931).


## Contributing

We welcome contributions from the community! 

Are you passionate about open-source development and eager to make a real impact in the world of industrial software? The gipSoft is looking for talented developers like you to help us push the boundaries of innovation and autonomy in software solutions.

By contributing to the iPlus-framework, you'll be part of a dynamic community dedicated to creating flexible, scalable, and efficient software that empowers businesses to thrive in the era of Industry 4.0. Your expertise and creativity will directly influence the evolution of our platform, helping to solve complex technical challenges and drive the future of manufacturing technology.

**Why Contribute?**

**Make an Impact:** Your contributions will help shape a powerful tool that supports businesses worldwide in achieving greater autonomy and efficiency.
Collaborate and Innovate: Work alongside a passionate community of developers and industry experts, sharing knowledge and driving innovation.

**Revenue Sharing:** As a token of our appreciation, contributors will receive a share of the revenue generated from the commercial edition of the iPlus framework, recognizing your valuable input and dedication.
Join us on GitHub and be part of a project that values collaboration, innovation, and shared success. Together, we can build a more sustainable and independent future for industrial software.

Contribute today and let's create something amazing together!
More about our [rewarding model](https://iplus-framework.com/en/documentation/Read/Index/View/a4100937-4d88-487d-ab3b-e599412e2a2f?workspaceSchemaID=ab0bc53f-decf-4101-9cee-111b6cbc5b24).

## Limitations
We have a [fork of entity framework core](https://github.com/iplus-framework/efcore/tree/ef_for_iPlus_compilable) that fixes the following issues:
1. Ability to refresh a context from the database
https://github.com/dotnet/efcore/issues/16491  
This point is problematic if you are working with long-term database contexts and already loaded entity objects and relationships to other objects have to be reloaded from the database. An example would be if you change a workflow on the client side that was already in use on the server side and it has to be reloaded.

2. DetectChanges slow in long term contexts
https://github.com/dotnet/efcore/issues/31819  
This point is problematic when you work with long-term database contexts and larger amounts of data, because the system then becomes slow.

3. Reverting changes
https://github.com/dotnet/efcore/issues/14594

If you want to use **iPlus for productive operation** use this release 9.0 fork and **branch "ef_for_iPlus_compilable"** instead of the official nuget packages. Open the [Directory.Build.props](https://github.com/iplus-framework/iPlus/blob/main/Directory.Build.props) file and set **UseEFCoreForkIPlus to True to** include the compiled ef-assemblies.

5. Obsolete constructor public StackTrace(Thread targetThread, bool needFileInfo)
https://github.com/dotnet/runtime/issues/80555  
This point is more of an important "nice to have". Pausing threads, analyzing the stack trace and then letting them continue running is used to [analyze deadlocks](https://iplus-framework.com/en/documentation/Read/Index/View/6a220db6-a767-40bb-bf95-395e4a289881?chapterID=a09687ec-ede3-4cba-827c-478c74b16b1a#ItemTextTranslationID_0b319477-1658-403a-bbac-bb6da719415b) in multi-threaded applications and to be able to perform good [system diagnostics](https://iplus-framework.com/en/documentation/Read/Index/View/6a220db6-a767-40bb-bf95-395e4a289881?chapterID=d45d9a09-c4d9-4cdf-8a83-910c5fdb99ba#ItemTextTranslationID_08c43200-1b1d-437b-9a1b-846d14e820d7) so that you can identify which program parts and objects are responsible for higher system loads.

6. Improved INotifyCollectionChanged Handling
https://github.com/dotnet/wpf/issues/52  
We have a workaround, but it should still be resolved by MS.

7. DataContractSerializerOperationBehavior: Create a serializer with Resolver
https://github.com/CoreWCF/CoreWCF/pull/512  
We have a workaround, but it should still be resolved by MS.

## Documentation

For now, please use the [documentation on our website](https://iplus-framework.com/en/documentation/Home/Schema/View/bce1702a-7637-4b98-83db-01a9d7a3a156).

We are working on transferring the content to the [iPlus.docs on gitHub](https://github.com/iplus-framework/iPlus.docs) and ask for your patience.

## License

The iPlus-framework is licensed under the GPL-V3 License. See the [LICENSE](LICENSE) file for more details.

The commercial edition is intended for end customers for commercial use. See this [LICENSE](https://iplus-framework.com/en/documentation/Read/Index/View/8dec2941-f7ed-4bed-92a5-0e07404e359a?workspaceSchemaID=ab0bc53f-decf-4101-9cee-111b6cbc5b24) file for more details.

## Contact

For questions, suggestions, or feedback, please send us a e-mail to **info[at]iplus-framework.com** or [via contact form](https://iplus-framework.com/en?section=Contact#aedf447c-2a3a-4bfe-9e0a-a9c5740e4f8e).
We look forward to your contributions and hope you enjoy working with the iPlus-framework!
