# Website text — iPlus-framework

Paste-ready copy for the **iPlus-framework** pages of https://iplus-framework.com.
Each section below maps 1:1 to a section of the website navigation, so you can replace the
existing text block by block. Changed passages are marked with a short note in
`> **Changed:**` blockquotes — delete those notes before publishing.

---

## Welcome

### Welcome to gipSoft!

**Empowering Industry with Open Source Innovation**

At gipSoft, we are on a mission to restore autonomy and freedom in the industrial sector through
our iPlus-framework. In a world increasingly dominated by proprietary systems and corporate
control, we stand as a beacon of independence, offering a powerful open-source software platform
that empowers businesses, integrators and developers alike.

#### Our Vision

We believe that the future of industry lies in open collaboration and shared innovation. Just as
Linux revolutionized the software world, the iPlus-framework is poised to transform industrial
automation by providing a flexible, scalable and efficient solution that breaks free from the
constraints of traditional software models.

#### Why choose the iPlus-framework?

**For end users:** Whether you are in mechanical engineering or any other manufacturing sector,
our framework gives you the tools to regain control over your production processes. With iPlus you
achieve seamless integration, optimize resource utilization and keep full ownership of your data
and your solutions.

**For integrators and developers:** Join us in building a sustainable, community-driven ecosystem.
The iPlus-framework is designed to facilitate collaboration across disciplines, enabling you to
create and extend software solutions that meet the unique needs of your clients while remaining
updateable and flexible.

#### Our unique approach

As a small, financially independent company, we have developed the iPlus-framework entirely on our
own — which positions us uniquely to lead this liberation mission. Our platform combines classical
programming with a low-code environment, offering a hybrid solution that supports both
transaction-based and real-time, time-critical applications.

#### Join the movement

The success of our mission depends on the collective efforts of end users, integrators and
developers. We invite you to join us on this journey towards a more independent and sustainable
industrial future. Together we can create solutions that are not only innovative but also free from
the limitations of proprietary systems.

#### Support our mission

Your support is crucial. Whether by contributing to the framework or to our MES, by developing your
own components, or simply by spreading the word — every effort strengthens our community and brings
us closer to true industrial freedom.

---

## What is the iPlus-framework?

The iPlus-framework is a **low-code software platform** that combines database-based with
object-oriented application development and its presentation layer. It functions as both a
**development and a runtime environment**.

> **Changed:** "an extension of the .NET framework (NET 7)" → **.NET 10**.

Technically it is an extension of Microsoft's **.NET 10** platform, so that service-oriented
client/server applications can be developed more easily, more flexibly and faster.

You can use it to develop

- transaction-based systems,
- SCADA applications,
- mobile solutions,
- time-critical and scalable backend solutions (multitier),
- machine controls with frontend,
- Industrial Internet of Things applications (IIoT) …

… all with a **uniform programming model**. "Uniform" means that it makes no difference whether you
develop a class for server-side or client-side use. A transaction-based form is programmed in
exactly the same way as a communication component or an automation object such as a motor or a
temperature sensor.

You can also provide **your solutions as software components**, which other developers and project
engineers can use to provide their end customers with an integrated overall solution. A project
engineer **does not need any programming skills**, because they only have to **orchestrate the
components via drag & drop** in the iPlus development environment.

> **Changed:** Linux is no longer an IIoT side note — it is a first-class platform.

Server-side components run on **Windows and Linux**, and the iPlus client runs natively on
**Windows, Linux, macOS, Android, iOS and in the web browser** — no emulation layer required.

This new approach enables a huge **increase in productivity** and an **unprecedented ease of use
and simplicity** for developers, project engineers, consultants and end users.

*Download the iPlus-framework brochure!*

---

## Architecture

An object-oriented, component-based and **service-oriented framework** forms the core of the
iPlus-framework. This means that all classes or components are instantiated only through the
configuration in the database. There is no need to compile an entire system, and direct references
between assemblies are not necessary because instances communicate with each other via a neutral
interface.

The communication between the instances takes place either in the local memory space or distributed
over many network nodes. The developer is **completely freed from all network programming**. Which
instances run on which computer can be set with a few mouse clicks in the iPlus environment. This
network-transparent approach enables **unlimited scalability** of the overall system. The integrated
graphical workflow engine is scalable as well. The system is completely **event-driven** in order to
enable **almost real-time** processing and presentation.

Just as the instances (model) are decoupled from each other, the **presentation layer** (view) is
also completely **decoupled** from the model and only saved in the database. The advantage of this
approach is that other people who are not developers can **adapt the UI during runtime** to their
needs without any programming skills and without knowing how the deeper software levels work
(network communication, database access …) — and that according to the WYSIWYG principle. This is
also an enormous advantage for project engineers and consultants, because customization can be
carried out extremely quickly. With the iPlus-framework you receive a SCADA system as a
"by-product", which however is much **more powerful and flexible than classic SCADA systems**,
because it no longer makes a difference whether you want to design a motor, a communication driver,
a workflow class or a transaction-based mask.

---

## Unlimited expandability

**Microsoft Visual Studio®** allows you to develop your **own libraries** on top of the
iPlus-framework. There are only a few rules to comply with in order to set up so-called automation
components. While doing so you can focus entirely on implementing your logic; other complexities
such as network-transparent event processing, asynchronous method calls, access to databases and
persistence of object states remain hidden. This abstraction allows for a **universal programming**
of all classes, including transaction-based business objects such as warehouse management
components, automation objects such as valves and engines, and communication objects such as ERP
connections, mail components or scanners. The same programming model is used for all classes and
**it is always easy to use!**

Minor **project-specific customizations** can be developed straight in the iPlus development
environment without Microsoft Visual Studio®. The development environment consists mainly of a
project configurator, a user interface designer, a workflow designer and a code editor. Compiled
classes from assemblies can be virtually extended by properties, methods and designs in the
object-oriented sense. Therefore it makes no difference whether you add properties to database
tables (Entity Framework) or configure a class for PLC communication with process variables. This
**abstraction and standardization** leads to **better reusability and extensibility** and a
**shorter training time** in existing projects for other people.

Since the **iPlus development environment** is programmed with the iPlus-framework, you can expand
it as well and tailor it to your project requirements, build your own tools to speed up your
day-to-day business and **reduce your period of project development** significantly.

---

## Your solution

How you use the iPlus-framework is completely free.

Either you use the **standard iPlus frontend** or you make your **own product**.

You can also distribute your solution **as a software component** hosted on other iPlus
installations (runtime). This is done purely by configuration in the iPlus development environment
(orchestration), where components from other manufacturers are integrated as well. Thus your product
runs together with other products on the same target system. In addition, your software components
and components from other vendors can communicate with each other without a direct DLL dependency.

Regardless of whether you distribute it component-based or as your own end product, iPlus includes
an **integrated license manager** for commercial use.

---

## Update and upgrade

The iPlus-framework also comes with a built-in **package management system** that helps you export
your libraries, project configuration, designs etc. as an update package. That enables you to
provide your licensees with updates and to supply your customers with modifications tailored to
their project requirements, which you tested in advance on your own development system. It is even
possible to transfer updates directly from one database to another.

The concept of loose coupling of components enables iPlus-framework updates that we deliver to be
installed independently of your package updates.

The iPlus package management system is similar to the use of NuGet, but with the main differences
that

- the software components do not have to be linked together by a final compilation,
- a software package can be purely declarative (no .NET assemblies, only project configuration files
  and GUI description in XAML),
- the packages can be added during runtime (e.g. in a production system).

This has the following advantages:

- **Different parties** (project engineers, consultants, developers, teams) can work independently
  of one another on an overall solution.
- For time-critical systems, the overall solution can sometimes even be **expanded during productive
  operation**. If a suitable time window is then available, the iPlus-framework services can be
  restarted with the fully configured setting.
- **Integrated change request management:** configurations that have been created on test systems
  (local computer, shared test system …) can be transferred to the productive system via
  export/import.

---

## GUI & SCADA

> **Changed:** the UI is no longer WPF-only and MAUI is no longer used. Avalonia UI is the single UI
> technology for desktop, mobile and browser.

The **vector-based 2D/3D user interface** is generated dynamically out of the database (XAML) and
rendered by **Avalonia UI**. Designs can be overwritten to suit the object orientation. The whole
user interface — forms, controls — can be customized during runtime while still remaining
updateable. What's more, it is adaptive and adjusts to all screen resolutions.

Because the UI is defined in the database rather than compiled into the client, **the same design
runs on every platform**: Windows, Linux, macOS, Android, iOS and in the web browser (WebAssembly,
nothing to install). One solution can therefore serve desktop, mobile and browser users
simultaneously — and you can adapt it per customer without branching the code.

- **Hierarchical and consistent alarm system** for all components.
- **Unlimited alarm history** and relationships with application data, e.g. production order, batch,
  material management …
- **Logging of process variables.** Each property of any given instance can be logged event-based
  and displayed in a graph. Such graphs can plot both live and history data on various scales. The
  chart control offers many different setting and presentation options.

### FAQ: Desktop/Mobile vs. Browser?

What we often forget: the actual **purpose** of the browser is to transmit information to **unknown
users**. And the actual purpose of cloud computing is load balancing and scalability when the number
of users of internet services unexpectedly increases. If you know the user — where is the problem in
starting a local app that connects to services via internet/intranet?

The argument of simple application delivery via cloud for enterprise applications versus
on-premises is weak if the **on-premise deployment** is done well and **easy**, without any
installation (one-click). And an MVC pattern with different programming languages and complicated JS
frameworks on server and client side is not the right choice if you want to build **applications for
well-known users** efficiently and **time-saving**. The MVVM pattern of clean separation of view and
model is technically a much better solution.

> **Changed:** the original text argued *against* browser apps, which now contradicts the fact that
> we ship a WebAssembly browser client. The paragraph below resolves that.

That said, we do not force you to choose. The iPlus client is available as a **native desktop and
mobile app** *and* as a **browser app** from the same code base and the same designs. Use the native
client where you know your users and want maximum performance and hardware access; use the browser
client where zero-install delivery to occasional or external users matters more.

*More about MVVM*

---

## Workflows

- **ISA-S88** (DIN EN 61512).
- **Graphic workflow designer** for programming all kinds of process sequences. This allows you to
  model your own business processes of any kind (e.g. production processes, logistic processes,
  machine controls …).
- **Graphic program sequence history.** All steps of a process that are performed when executing a
  workflow or batch are logged along with their parameters, enabling you to view the audit trail as
  a Gantt chart in the log viewer. A comfortable and extensive search system assists users in
  spotting and improving process issues.
- **Varied options for analysis and logging.** In the object explorer (live debugger) the object
  status of all instances can be visualized and controlled along with their settings on the server
  or client.

---

## PLC

- **OPC-UA, Modbus-TCP, RFC1006** (Simatic®) drivers.
- **Converters and adapters.** All engineering offices use different ways of programming their PLC.
  We offer you the perfect means to adapt the iPlus-framework quickly and easily to the PLC model
  you are using, in order to reduce the period of project development significantly.
- **Complete Beckhoff® TwinCAT3® integration.** Our iPlus automation component model has also been
  implemented in TwinCAT3® in order to allow you to represent your iPlus project almost one-to-one
  in TwinCAT®. The built-in TwinCAT project synchronizer generates or synchronizes your whole iPlus
  project with the TwinCAT® project. After synchronization is complete, you only need to program the
  remaining project-specific logic. Using our special TwinCAT communication driver, communication
  with all objects works plug-and-play and there is nothing left for you to configure. The iPlus
  visualization is constantly updated in the millisecond range owing to the event-based programming
  technology. With iPlus you can save 80% of the time that would be spent programming your PLC and
  100% of the configuration time.

---

## Additional features

- **XAML-based graphical report generator.** Reports can be generated from databases as well as
  from the whole available object model, using component live data, serializable objects etc.
- **Multilingualism.** As many languages as desired can be added. It allows you to translate
  functions as well as property values of application tables.
- **Group-based rights management.** A group-based rights management system allows you to assign
  read and write access for all objects, properties and methods. What does this mean in concrete
  terms? No matter what type of class or component is registered in the iPlus development
  environment, it is up to the rights management — whether you want to administer a power-on command
  for a motor instance, a posting method in a warehouse management class, the property of an entity
  object (table field), or access to a process variable of a configured PLC driver. Everything is
  consistent thanks to the abstraction of the iPlus-framework!
- **Role-based online documentation system.** Documentation can be represented and linked according
  to different roles.
- **Corporate design.** You can adapt the style and design of iPlus to suit your design standards
  and your company's corporate identity.

---

## Technology (new section — recommended)

> **Changed:** the website currently has no consolidated technology section. Adding one makes the
> platform story much clearer and is the single biggest improvement you can make.

| Layer | Technology |
|---|---|
| **Runtime** | .NET 10 — one code base for server, desktop, mobile and browser |
| **Server side (no UI)** | Windows and Linux; console host or Windows/Linux service |
| **Client side (UI)** | **Avalonia UI** — Windows, Linux, macOS, Android, iOS and web browser (WebAssembly) |
| **Data access** | Entity Framework Core, with our own optimized `iPlus.EntityFrameworkCore.*` packages |
| **Communication** | Network-transparent RMI, WCF/CoreWCF, OPC-UA client and server, PLC drivers |
| **UI definition** | XAML stored in the database and rendered at runtime |

**Entity Framework Core.** Long-lived, network-transparent contexts need capabilities that upstream
EF Core does not provide, so we maintain and publish our own EF Core packages containing the fixes:
refreshing a context from the database ([dotnet/efcore#16491](https://github.com/dotnet/efcore/issues/16491)),
fast `DetectChanges` in long-term contexts ([dotnet/efcore#31819](https://github.com/dotnet/efcore/issues/31819))
and reverting changes ([dotnet/efcore#14594](https://github.com/dotnet/efcore/issues/14594)). The
packages are published on nuget.org and enabled by default — **no fork clone is required**, a plain
`dotnet restore` is all you need.

> The WPF client remains available for Windows-only and legacy deployments, but Avalonia UI is the
> strategic UI platform and the one used for all cross-platform releases.

---

## Dual Licensing (no change needed)

The existing page is consistent with the repositories and needs no correction:

- **Community Edition** — GPL-V3, free forever, 12% revenue share to contributors.
- **Commercial Edition** — 720 € one-time fee per user.

Two points worth adding, because they are new and relevant to integrators:

- **GPL packages and a commercial runtime do not mix.** A package published under GPL-3.0 cannot be
  loaded into a commercially licensed runtime, and vice versa. This keeps the licensing model
  unambiguous for you and your customers.
- **No developer dongle required.** Development and license issuing are software-based; you sign
  your product licenses with your own key pair. The framework ships an integrated license manager so
  you can issue and enforce licenses for your own components and products, while the runtime itself
  is licensed per concurrent user.
