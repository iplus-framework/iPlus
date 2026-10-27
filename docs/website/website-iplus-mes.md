# Website text — iPlus-MES

Paste-ready copy for the **iPlus-MES** pages of https://iplus-framework.com.
Each section below maps 1:1 to a section of the website navigation, so you can replace the
existing text block by block. Changed passages are marked with a short note in
`> **Changed:**` blockquotes — delete those notes before publishing.

---

## What is iPlus-MES?

Which features does a MES (Manufacturing Execution System) need today in order to meet the
challenges of competition in modern industry?

- Optimum **stability, performance and flexibility** must be ensured.
- Complex production processes have to be presented in a way that is **easy to understand for the
  user**.

These requirements are the basis for the development of iPlus-MES. With iPlus-MES you completely
close the gap between the company level (ERP) and the field level at the highest technological
level.

**iPlus-MES** is a product that was developed with the iPlus-framework. It is **standard software**
that is highly configurable and adaptable thanks to the advantages the iPlus-framework brings with
it. That is why it can **be expanded** to different sectors and customer-specific solutions. We have
already covered a large part of the requirements from process technology with our own industry
add-ons.

**If you are an end customer**, you can purchase it directly from us and we help you to introduce
the software. We can also implement your individual wishes, which are not included in the standard,
as a customer add-on.

**If you are an engineering company or an integrator**, you can roll it out with your own customers.
Due to the object- and service-oriented architecture of the iPlus-framework, you can expand and
adapt everything in the MES modules for your industries.

*Here: Download the MES brochure!*

---

## Architecture

The **database model** serves as the base on which various production and business processes can be
depicted. We put great emphasis on creating a completely normalized model that adapts to your
production and IT system landscape, not the other way round. The ease of adaptation can only take
effect if you are using the right "chassis" such as our iPlus-framework.

As an **integrator**, you can also expand the database model for your purposes, or you can use your
own model that reuses parts of our MES model. This freedom is possible because you decide for
yourself on which software levels you want to build:

1. **iPlus-framework.**
2. **iPlus-MES**, which builds on level 1.
3. **Your specially developed industry solution**, which is either built on level 2 or directly on
   level 1.
4. **Your customer solution**, which builds on level 3.

---

## Why iPlus-MES?

iPlus-MES is not dedicated to specific industries but can be used in both process and production
technology. The special structure of our iPlus-framework allows for customized extension of the MES
features by sectoral features. This way, iPlus-MES has come to be in use in various industries.

With its failure-free merging of production cycles, iPlus-MES helps you stay one step ahead of the
competition and achieve perfect utilization of resources and the highest possible efficiency. In
addition, all processes are rendered considerably more transparent.

Read on Wikipedia which benefits a MES offers and why you should consider introducing it in your
factory: https://en.wikipedia.org/wiki/Manufacturing_execution_system

### Facts and figures

- **> 30 installations, > 500 workplaces/users, > 4000 machines and devices**
- **100% successful projects, 100% satisfied customers** (ask us for our references)
- **Best price/performance ratio** of any competitor and best ROI (ask for a non-binding offer)
- **Investment security** through an on-premise license model. Due to the cloud hype of recent
  years, the TCO has increased enormously. Cloud and Industry 4.0 are two completely different
  things.

> "With iPlus-MES you make the right business decision in the long term!"

---

## Usability perfected

The real usability of software depends on its user acceptance — we are aware of that fact. For that
reason we developed iPlus-MES with a **focus on high usability**. Many years of experience and
constant communication with our users have helped us succeed in creating an easy and **intuitive
graphical user interface** that differs vastly from other products on the market.

The **merging of the world of SCADA with the transaction-based world of MES** allows for an
exceptional user experience:

The user can "surf" and **navigate seamlessly, without keyboard entry, between physical objects**
such as machines and modules, **and business objects** like application data, since all features are
linked. With iPlus-MES the user does not have to know all relations and contiguities between data in
order to manage the control system effectively. That shortens training periods considerably.

### Scope of iPlus-MES

- Master data management
- Production and control
- Material management
- Logistics
- Quality management
- Maintenance

---

## Master data

The master data management comes with a basic set of properties that are commonly used in a lot of
industries. Through the iPlus-framework, the **database model** (Entity Framework) can be
**extended** to include more project- or industry-specific properties, relationships and complex
structures **without losing updateability!** Some of the most important master data are:

- Material master (groups, GMP, type of material)
- Quantity units and conversion rules
- Hierarchical model of storage location and stockyard
- Lab and quality master data
- Client and address management
- Staff and visitor administration
- Vehicle and motor pool master data
- RFID/smart card management
- Service and maintenance data
- Workflow management
- Bill of materials / formulas

The variety of the aforementioned master data illustrates how wide-ranging iPlus-MES is. Its one
specific feature, however, is its production technology. Through an ingenious method we call
**"strict entity separation with progressive concretization"**, iPlus-MES can depict any production
process.

Basic components are:

- **Graphic process workflows** with inheritable parameters and routing rules. iPlus workflows are
  universally applicable — for instance for production control, interface communication and business
  processes — and can be created or programmed by the end user on different levels of abstraction.
- **Material workflows** for modelling flows of materials and relating them with process workflows.
- **Bills of material** related to material workflows and process workflows.

---

## Production and control

We continued our principle of "strict entity separation with progressive concretization" in
production:

- **Production order.** All process parameters, routing rules and material compositions defined in
  the master data can be refined and concretized using inheritance mechanisms. In addition, batch
  plans and prioritization rules can be set up. After the start of both manual and timed production,
  all data that are part of the production order can be monitored live — such as the amounts used
  and produced, including their lot and batch designation, and the graphic record of production.
  Long-term storage of production data is nearly unlimited, i.e. only limited by the maximum capacity
  of the database.
- **Process control and visualization.** There is no automation process in iPlus-MES that runs
  hardcoded. The system control uses workflows that are instantiated dynamically in the background.
  Each workflow node has its own status and acts autonomously in a large network of so-called
  automation components, which are structured and connected similar to the internet. Having its own
  defined task or function, each automation component comes with different features and in varying
  states, and in turn triggers other automation components it is related to. This more or less
  indirectly creates an organized process flow.

  The process flow and the complete process status are graphically represented in the visualization
  and process control. Depending on the authorization level of the user, the cycle and every
  individual status can be managed down to the smallest detail via the user interface in order to
  keep complete control over production. Using iPlus you never experience pending processes that
  have to be fixed by a programmer, since iPlus functions as development, runtime and diagnostic
  environment all in one.
- **Simulation.** Most PPS systems are based on the MRP II approach and traditional capacity
  planning. That reinforces a basic source of error: the planning calculations are done with
  predefined algorithms and do not take into account the status of all machines in a facility. These
  other systems do not control the machines, they only treat them as a black box.

  > "This approach does not meet the requirements of Industry 4.0."

  Rather, the same logic that manages the system control or MES has to be implemented in the
  planning algorithm as well. And this is where the unique feature of iPlus-MES comes into play: with
  its process control logic already implemented in the automation components and its process cycles
  managed via workflows, iPlus-MES allows you to run production in fast motion in a mirrored testing
  system. This way all automation components are put into simulation mode and the production system
  is run in a "sandbox" of sorts. The simulation result reflects the real production process and
  helps you recognize shortages in resources in advance.

---

## Material management

**Warehouse management**

- With or without lot designation
- Minimum and maximum reporting and tolerance limit
- Monitoring of expiration dates
- Material reservation
- Accounting history
- Various inventory views
- Inventory
- Daily, weekly, monthly and annual reports

**Procurement**

- Contract management
- Order management
- Delivery notes

**Sales**

- Contract management
- Order processing
- Delivery notes
- Accounting

Warehouse management is often situated in a grey area between the fields of responsibility of ERP
(enterprise resource planning) and MES systems. Intersections between systems are common in this
area. Integration into both systems is considerably facilitated and sped up the more flexible the
respective data models are and the more clear-cut the separation between the software layers is.
iPlus-MES takes a lead role in the implementability of such scenarios.

---

## Logistics

- **Guest admittance and on-site list**
- **RFID and smart card technology**
- **Route planning**
- **Order picking** in production, incoming goods and outgoing goods, and load planning
- **Acceptance of goods and loading control** — semi- and fully automatic
- **Material transports**

Like warehouse management, logistics is an interdisciplinary matter. Planning tasks are commonly
carried out on ERP level, while operations are performed with MES. iPlus affords you the freedom of
adapting features to best suit your company's needs.

In the end, system control is the task of iPlus-MES — you can count on that. As in production, all
processes are managed via workflows and, as a result, can be customized. Transport routes tend to be
very complex in big production facilities containing many conveying elements and junctions. This
often poses a challenge to programmers trying to implement all that in the PLC (programmable logic
controller) program. Especially if new junctions are built in mechanically, costs might explode,
since a PLC is not originally meant to manage such tasks.

That is the reason why the iPlus-framework comes with an **intelligent route management** system by
default. The whole relations network (from/to) that connects the objects (nodes) is configured via
the user interface. Based on routing rules set by the user in the workflows, the best routes are
determined prior to the start of each material transportation process and uploaded dynamically to
the PLC control. Clients are now able to modify and expand their facilities how and whenever they
like, without having to rely on the help of a programmer for the complex adaptation of the
programming.

---

## Quality management

- **Batch monitoring and tracking.** Barcodes and QR codes, RFID technology, scanner connection,
  label printing etc. are no challenge for iPlus-MES. Manual processes and user guidance in material
  supply, lot recommendation and lot validation are carried out by means of workflow technology.
  This way iPlus-MES does not miss one lot and no lot ends up in the wrong product/batch. With
  materials transportation and processing completed, each batch can be traced completely both
  forward and backward. Batch tracing is such an integral part of iPlus-MES that you can initiate
  tracing from anywhere in the program.
- **Laboratory support.** Automated sampling, connection to laboratory equipment and lab systems,
  lab order templates such as the definition of measurement and limit values, linkage to material
  management and batch tracing.
- **Production data acquisition** — order-related and independent of orders.
- **Reporting and alarm handling.** Order- and machine-related analysis. The alarm history is linked
  to order data. All alarm limit values are freely definable and can be linked to the application
  data. Alarm messenger service (smartphone notification).

---

## OEE & Maintenance

- **Maintenance orders and plans**
  - Maintenance orders
  - Role-based
  - Event- or time-based
  - Help system with maintenance instructions
  - Maintenance via mobile app
  - Spare parts management
- **Sensor-based condition monitoring**
  - Predictive (AI)
- **OEE**
  - Sensor/counter- or posting-based
  - Order-related

---

## Development and training courses

No MES system contains all the features that you as a client might need, since production technology
is very multi-faceted. That is why it is important to use a system that is expandable to suit your
requirements, rather than adjusting your requirements to go with the system. At the same time,
updateability has to be retained in order for you to constantly operate on the latest status.

**Customized add-on programming** in iPlus-MES works exactly like that.

Please **do not hesitate** to **contact us** for an add-on solution tailored to your wishes —
complete with source code. Additionally, we offer training courses so that you yourself can
implement a suitable solution in the iPlus-framework and manage everything independently.

**If you are an engineering company or integrator**, we also offer **training** courses, so that you
yourself can implement a suitable solution in the iPlus-framework and manage everything
independently.

**If you are an end customer**, we also offer **training**, so you and your employees know how to use
iPlus-MES in daily operations.

---

## Technology (new section — recommended)

> **Changed:** the MES page currently has no technology or platform section at all. Adding one makes
> the product story consistent with the framework page and answers the most common question
> ("what does it run on?").

iPlus-MES inherits its entire technology stack from the iPlus-framework:

| Layer | Technology |
|---|---|
| **Runtime** | .NET 10 — one code base for server, desktop, mobile and browser |
| **Server side (no UI)** | Windows and Linux; console host or Windows/Linux service |
| **Client side (UI)** | **Avalonia UI** — Windows, Linux, macOS, Android and web browser (WebAssembly) |
| **Data access** | Entity Framework Core, with our own optimized `iPlus.EntityFrameworkCore.*` packages |
| **Communication** | Network-transparent RMI, WCF/CoreWCF, OPC-UA client and server, PLC drivers |
| **UI definition** | XAML stored in the database and rendered at runtime |

Because the UI is defined in the database rather than compiled into the client, **the same MES
designs run on desktop, mobile and in the browser** — one solution, no separate code base, and no
installation for browser users.

> **Note:** the framework also ships an iOS client head. An iPlus-MES iOS head can be added the same
> way if you need it.

**Entity Framework Core.** iPlus-MES inherits its data access from the iPlus-framework, which is
built on Entity Framework Core. Long-lived, network-transparent contexts need capabilities that
upstream EF Core does not provide, so we maintain and publish our own EF Core packages containing
the fixes: refreshing a context from the database
([dotnet/efcore#16491](https://github.com/dotnet/efcore/issues/16491)), fast `DetectChanges` in
long-term contexts ([dotnet/efcore#31819](https://github.com/dotnet/efcore/issues/31819)) and
reverting changes ([dotnet/efcore#14594](https://github.com/dotnet/efcore/issues/14594)). The
packages are published on nuget.org and enabled by default — **no fork clone is required**, a plain
`dotnet restore` is all you need.

---

## Dual Licensing (no change needed)

The existing page is consistent with the repositories and needs no correction:

- **Community Edition** — GPL-V3, free forever, 12% revenue share to contributors.
- **Commercial Edition** — 720 € one-time fee per user.

Two points worth adding, because they are new and relevant to integrators:

- **GPL packages and a commercial runtime do not mix.** A package published under GPL-3.0 cannot be
  loaded into a commercially licensed runtime, and vice versa.
- **No developer dongle required.** Development and license issuing are software-based; you sign
  your product licenses with your own key pair. The framework ships an integrated license manager so
  you can issue and enforce licenses for your own add-ons and products, while the runtime itself is
  licensed per concurrent user.
