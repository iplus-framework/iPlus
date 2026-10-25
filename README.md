# iPlus-Framework

Welcome to the iPlus-Framework repository! This open-source project is designed to empower developers to create flexible, scalable, and efficient software solutions for the industry.
![myimage-alt-tag](https://github.com/iplus-framework/iPlus-Docs/blob/main/Images/EngineeringEnv.jpg)

## Table of Contents

- [iPlus-Framework](#iplus-framework)
  - [Table of Contents](#table-of-contents)
  - [Introduction](#introduction)
  - [Features](#features)
  - [What you can create with it](#what-you-can-create-with-it)
  - [Technology](#technology)
  - [Getting Started](#getting-started)
  - [Contributing](#contributing)
  - [Entity Framework Core](#entity-framework-core)
  - [Documentation](#documentation)
  - [Releases](#releases)
  - [License](#license)
  - [Contact](#contact)

## Introduction

**iPlus is an open-source industrial software platform that combines classical programming with a
low-code environment.** It is both a development and a runtime environment: you build
components, orchestrate them in a designer, and the running system instantiates them from the
database — no final compilation, no redeployment to change a screen or a workflow.

It is built for developers and integrators who need to deliver **updateable, individualized
software** to industrial customers without losing control of the software lifecycle. The same
component model covers transaction-based business objects, SCADA/automation objects,
communication drivers and the user interface — so a warehouse form, a motor and an OPC-UA
connection are all programmed the same way.

Our mission is to offer an alternative to proprietary systems: a platform that keeps end users,
integrators and developers independent of a single vendor. The framework is licensed under the
**GPL-3.0**, so every solution built on it stays open and inspectable, and a community-driven
ecosystem can grow around it. A **commercial edition** is available for productive use, and
contributors share in its revenue.

### Highlights

- 🖥️ **One UI, every platform** — Avalonia UI runs on Windows, Linux, macOS, Android, iOS and in
  the browser. The same XAML design, stored in the database, renders everywhere.
- 🧩 **Component-based and network-transparent** — instances communicate over a neutral interface;
  you never write network code. Distribution across machines is a configuration detail.
- ⚡ **Change at runtime** — designs, workflows and packages can be modified in a productive
  system and become active without a rebuild.
- 🏭 **SCADA included** — PLC connections, OPC-UA client and server, alarms, logging and charts
  come as a by-product of the architecture.
- 📦 **Package management** — ship libraries, configuration and designs as updateable packages,
  independently of framework updates.
- 🔐 **Integrated license management** — issue and enforce licenses for your own products on top
  of the framework.

## Features

iPlus is designed for the complex requirements of modern industrial software development.

### Programming model

- **Hybrid programming** — classical C# code and low-code configuration in one consistent model.
- **Uniform programming model** — a server-side class, a client-side form, a communication driver and
  an automation object are all written the same way.
- **Component-based architecture** — extend and customize by inheritance, by new components, or by
  declarative configuration; supports transaction-based and real-time, time-critical solutions alike.
- **Network transparency** — synchronous and asynchronous RMI, remote properties and events across
  process and machine boundaries, without any network programming.
- **Virtual inheritance** — extend compiled classes with properties, methods and designs in the
  object-oriented sense, without touching the original assembly.
- **Declarative programming** — minimize maintenance and maximize compatibility; changes and
  extensions can be applied during runtime.

### Runtime and operations

- **Object persistence** — entities and object states are persisted and reloaded transparently.
- **Messages and alarms** — hierarchical, consistent alarm system for all components, with unlimited
  alarm history and relations to application data (orders, batches, materials).
- **Property logging** — every property of any instance can be logged event-based and plotted live
  or historically.
- **Workflow engine** — ISA-S88 (DIN EN 61512) oriented, scalable and event-driven.
- **Diagnostics** — runtime system diagnostics, deadlock analysis and object-state inspection over
  the network.
- **Multithreading** — built-in threading model for time-critical processing.

### Tooling

- **UI designer** — WYSIWYG design of forms and controls, stored in the database as XAML.
- **Workflow designer** and **project configurator**.
- **Code editor** and **scripting** — project-specific customizations without Visual Studio.
- **Report engine**.
- **Package management** — export libraries, configuration and designs as updateable packages.
- **Integrated license management** for commercial products built on the framework.

### Integration

- **IT/OT bridge** — connects the ERP level with the field level.
- **PLC connections**, **OPC-UA client** and **OPC-UA server** built in.
- **Entity Framework Core** data access, with our own optimized packages (see
  [Entity Framework Core](#entity-framework-core)).

## What you can create with it

| Solution | What iPlus gives you |
|---|---|
| **SCADA** | Out of the box — built-in PLC connections, OPC-UA client and server, alarms, logging and charts. |
| **DCS / MES** | Distributed control and manufacturing execution systems. Use the [iPlus-MES repository](https://github.com/iplus-framework/iPlusMES) for an out-of-the-box MES, together with [iPlus-MES-Mobile](https://github.com/iplus-framework/Mobile). |
| **Custom industrial applications** | Any application or component can be developed and shipped as a package managed by the runtime. |
| **HMI / OEM software** | Standalone installations, including single-board computers running Linux (Armbian, Raspbian). |
| **ERP** | On-premise ERP software. |
| **Mobile solutions** | Native Android and iOS apps from the same component model and designs. |
| **Web applications** | The same client running in the browser via WebAssembly — no installation for the user. |

Because the UI is stored in the database rather than compiled into the client, **one solution can
serve desktop, mobile and browser users simultaneously** — and you can adapt it per customer
without branching the code.

## Technology

- **Runtime:** .NET 10 — one code base for server, desktop, mobile and browser.
- **Server side (no UI):** Windows and Linux. Runs as a console host or as a Windows/Linux service.
- **Client side (UI):** **Avalonia UI** is the primary UI technology and is fully functional on:
  - Windows (x64)
  - Linux (x64)
  - macOS
  - Android
  - iOS
  - Web browser (WebAssembly — nothing to install)
- **Data access:** Entity Framework Core, using our own optimized `iPlus.EntityFrameworkCore.*` packages (see [Entity Framework Core](#entity-framework-core)).
- **Communication:** network-transparent RMI, WCF/CoreWCF, OPC-UA client and server, PLC drivers.
- **UI definition:** XAML stored in the database and rendered at runtime — the same design runs on desktop, mobile and browser without rework.

> The WPF client remains in the repository for Windows-only and legacy deployments, but Avalonia UI is the strategic UI platform and the one used for all cross-platform releases.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A Microsoft SQL Server edition (Express, Developer or Standard) — on Windows or Linux, or in Docker
- Optional: Visual Studio 2022+, JetBrains Rider or VS Code

### 1. Clone the repository

```bash
git clone https://github.com/iplus-framework/iPlus.git
```

### 2. Install SQL Server

Follow the [Microsoft SQL Server installation guide](https://learn.microsoft.com/en-us/sql/database-engine/install-windows/install-sql-server?view=sql-server-ver16).
On Linux, the official container image is the quickest route — see the
[evaluation bundle](deploy/docker/README.md), which brings up SQL Server, the iPlus service and
the browser client in one step.

### 3. Restore an iPlus database

Restore the SQL Server backup file located in the `Database` folder.

### 4. Configure the connection string

Edit `ConnectionStrings.config` in the project you want to start. The exact steps are described in
[gipSoft's documentation](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=193d292e-df31-405e-a3e9-f1116846bf86#ItemTextTranslationID_d0551cc7-f767-4790-8ecb-8771836ebac7).

### 5. Build and run

Pick the client that matches your target platform. All of them share the same code and designs.

```bash
# Desktop (Windows, Linux, macOS)
dotnet run --project gip.iplus.client.avui.Desktop

# Browser (WebAssembly)
dotnet run --project gip.iplus.client.avui.Browser

# Android
dotnet run --project gip.iplus.client.avui.Android

# Server service without UI
dotnet run --project gip.iplus.console -- /U00 /P00
```

The Avalonia UI solution is [iplusAvaloniaUI.slnx](iplusAvaloniaUI.slnx); the full solution is
[iplus.slnx](iplus.slnx).

> **No WINE required.** The Avalonia UI client runs natively on Linux and macOS. WINE is only
> needed if you want to run the legacy WPF client on Linux.

### 6. Log in

- **`superuser` / `superuser`** — start as a **client**.
- **`00` / `00`** — start as a **service**.

After starting the service, your local IP is remembered; start a second instance and log on as a
client to establish the client-server connection. If the client cannot connect, read the
[network configuration section](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=a7424f03-cbff-4f68-90eb-262b3fdcba1c#ItemTextTranslationID_bf788469-8a2f-4a7c-ad78-2710960096dc).

> **Important:** every time you pull a newer version from GitHub, **hold down the CTRL key** while
> clicking the [login button](https://iplus-framework.com/en/documentation/Read/Index/View/b00675a8-718c-4c13-9d6d-5e751397ac5f?chapterID=ca1a96f0-233d-4e6c-9cfc-d1db683e4be3#ItemTextTranslationID_e24a12c4-649d-4903-abf0-8366ed656ca1)
> so that your local databases are updated. Otherwise the application will not start.

### 7. Build the EF models (optional)

Only needed if you change the data model:

```bash
dotnet tool update --global dotnet-ef
```

See the [EF Core tools documentation](https://learn.microsoft.com/en-us/ef/core/cli/dotnet).

🤗 Have fun trying it out and watch our videos to get started. We also recommend
[online training](https://iplus-framework.com/en?section=Support%20%26%20Training#d57ccb45-9050-41cb-a177-9e8c05028931).


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

## Entity Framework Core

iPlus is built on Entity Framework Core, but long-lived, network-transparent contexts need
capabilities that upstream EF Core does not provide. Rather than asking you to patch or
fork EF Core yourself, **we maintain and publish our own EF Core packages** that contain
the fixes:

| Package | Purpose |
|---|---|
| `iPlus.EntityFrameworkCore` | Core, with the fixes below |
| `iPlus.EntityFrameworkCore.Relational` | Relational layer |
| `iPlus.EntityFrameworkCore.SqlServer` | SQL Server provider |
| `iPlus.EntityFrameworkCore.Proxies` | Lazy-loading proxies |
| `iPlus.EntityFrameworkCore.Abstractions` | Abstractions |
| `iPlus.EntityFrameworkCore.Design` / `.Tools` | Design-time tooling |

The fixes these packages carry:

1. **Refresh a context from the database** — [dotnet/efcore#16491](https://github.com/dotnet/efcore/issues/16491)  
   Essential for long-term contexts: already-loaded entities and their relationships can be
   reloaded from the database. For example, when a workflow changed on the client has to be
   reloaded on the server.
2. **Fast `DetectChanges` in long-term contexts** — [dotnet/efcore#31819](https://github.com/dotnet/efcore/issues/31819)  
   Without this, long-lived contexts with large amounts of data become progressively slower.
3. **Reverting changes** — [dotnet/efcore#14594](https://github.com/dotnet/efcore/issues/14594)

### Using them

The packages are consumed from nuget.org and are **enabled by default** — a fresh clone
builds against them with no extra setup. The switch lives in
[build/EntityFramework.Build.props](build/EntityFramework.Build.props):

```xml
<UseEFCoreForkIPlus>True</UseEFCoreForkIPlus>
```

Set it to `False` only if you explicitly want the stock `Microsoft.EntityFrameworkCore.*`
packages. The version is pinned centrally in
[Directory.Packages.props](Directory.Packages.props) via `EFCorePackageVersionFork`.

> **Note:** you no longer need to clone the EF Core fork. The packages are published on
> nuget.org, so `dotnet restore` is all that is required.

### Upstream issues we work around

These are handled inside iPlus, so they do not affect you as a user:

- **Obsolete `StackTrace(Thread, bool)` constructor** — [dotnet/runtime#80555](https://github.com/dotnet/runtime/issues/80555).
  Used for [deadlock analysis](https://iplus-framework.com/en/documentation/Read/Index/View/6a220db6-a767-40bb-bf95-395e4a289881?chapterID=a09687ec-ede3-4cba-827c-478c74b16b1a#ItemTextTranslationID_0b319477-1658-403a-bbac-bb6da719415b)
  and [system diagnostics](https://iplus-framework.com/en/documentation/Read/Index/View/6a220db6-a767-40bb-bf95-395e4a289881?chapterID=d45d9a09-c4d9-4cdf-8a83-910c5fdb99ba#ItemTextTranslationID_08c43200-1b1d-437b-9a1b-846d14e820d7).
- **`INotifyCollectionChanged` handling** — [dotnet/wpf#52](https://github.com/dotnet/wpf/issues/52).
- **`DataContractSerializerOperationBehavior` resolver** — [CoreWCF/CoreWCF#512](https://github.com/CoreWCF/CoreWCF/pull/512).

## Documentation

For now, please use the [documentation on our website](https://iplus-framework.com/en/documentation/Home/Schema/View/bce1702a-7637-4b98-83db-01a9d7a3a156).

We are working on transferring the content to the [iPlus.docs on gitHub](https://github.com/iplus-framework/iPlus.docs) and ask for your patience.

## Releases

Prebuilt binaries are published as GitHub Releases. The release channels, artifact
layout, local build commands and the CI pipeline are documented in
[docs/release-process.md](docs/release-process.md).

To produce a release bundle locally:

```bash
./build/publish-release.sh --platform desktop-linux
```

An evaluation bundle with SQL Server, the iPlus service and the browser client is
available in [deploy/docker/](deploy/docker/README.md).

## License

iPlus is **dual-licensed**:

| Edition | License | Intended for |
|---|---|---|
| **Community** | [GPL-3.0](LICENSE) | Development, evaluation and non-commercial use |
| **Commercial** | Commercial license | Productive, commercial operation |

The source code is the same in both editions — the commercial edition adds a runtime license,
support and the right to use the software commercially. See the
[license conditions](https://iplus-framework.com/en/documentation/Read/Index/View/8dec2941-f7ed-4bed-92a5-0e07404e359a?workspaceSchemaID=ab0bc53f-decf-4101-9cee-111b6cbc5b24)
and the [dual licensing overview](https://iplus-framework.com/en?section=Dual%20Licensing#807d323a-35ba-4896-b330-b1cf3d012a0b).

### Building products on iPlus

You can build and sell your own products on top of iPlus. The framework ships an integrated
license manager so you can issue and enforce licenses for your own components and products, while
the runtime itself is licensed per concurrent user.

- **GPL packages and commercial runtime do not mix.** A package published under GPL-3.0 cannot be
  loaded into a commercially licensed runtime, and vice versa. This keeps the licensing model
  unambiguous for you and your customers.
- **No developer dongle required.** Development and license issuing are software-based; you sign
  your product licenses with your own key pair.

## Contact

For questions, suggestions, or feedback, please send us a e-mail to **info[at]iplus-framework.com** or [via contact form](https://iplus-framework.com/en?section=Contact#aedf447c-2a3a-4bfe-9e0a-a9c5740e4f8e).
We look forward to your contributions and hope you enjoy working with the iPlus-framework!
