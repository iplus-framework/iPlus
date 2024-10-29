# iPlus Framework

Welcome to the iPlus-Framework repository! This open-source project is designed to empower developers to create flexible, scalable, and efficient software solutions for the industry.

## Table of Contents

- [Introduction](#introduction)
- [Features](#features)
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
- **DCS, MES:** Distributed Control Systems, Manufacturing Execution System. Use the iPlus-MES repository for a out of the box usage.
- **Custom industrial applications:** Any applications/components can be developed and provided as packages that are managed by the runtime.
- **HMI or OEM Software**: Standalone installations. Software for single board computers.
- **ERP**: On Premise ERP-Software
- **Mobile**: Mobile Solutions
- ...

## Getting Started

To get started with the iPlus Framework, follow these steps:

1. **Clone the Repository:**
git clone https://github.com/iplus-framework/iPlus.git

2. **Install a Microsoft SQL-Server Edition:**
https://learn.microsoft.com/en-us/sql/database-engine/install-windows/install-sql-server?view=sql-server-ver16

3. **Restore a iplus-database file**
Download the iplus-database file [on gipSoft's site](https://iplus-framework.com/en/Deploy/Serve?name=iPlusV4Database.zip).

4. **Open project in Microsoft Visual Studio:**
Compile the solution an set gip.iplus.client.exe as start project.

5. **Modify the ConnectionStrings.config file:**
Follow the instructions on [gipSoft's documentation site](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=193d292e-df31-405e-a3e9-f1116846bf86#ItemTextTranslationID_d0551cc7-f767-4790-8ecb-8771836ebac7)

6. Execute gip.iplus.client.exe


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
The following features and bugs, from Microsoft's repositories, prevent the efficient and productive use of iplus-framework:

1. Ability to refresh a context from the database
https://github.com/dotnet/efcore/issues/16491

2. DetectChanges slow in long term contexts
https://github.com/dotnet/efcore/issues/31819

3. Obsolete constructor public StackTrace(Thread targetThread, bool needFileInfo)
https://github.com/dotnet/runtime/issues/80555

4. Improved INotifyCollectionChanged Handling
https://github.com/dotnet/wpf/issues/52

5. DataContractSerializerOperationBehavior: Create a serializer with Resolver
https://github.com/CoreWCF/CoreWCF/pull/512

Please GIVE these issues a THUMBS-UP 👍 so that they can be prioritized and Microsoft developers can finally start implementing them.

## License

The iPlus-framework is licensed under the GPL-V3 License. See the [LICENSE](LICENSE) file for more details.

The commercial edition is intended for end customers for commercial use. See this [LICENSE](https://iplus-framework.com/en/documentation/Read/Index/View/8dec2941-f7ed-4bed-92a5-0e07404e359a?workspaceSchemaID=ab0bc53f-decf-4101-9cee-111b6cbc5b24) file for more details.

## Contact

For questions, suggestions, or feedback, please [contact us directly at](https://iplus-framework.com/en?section=Contact#aedf447c-2a3a-4bfe-9e0a-a9c5740e4f8e).

We look forward to your contributions and hope you enjoy working with the iPlus-framework!
