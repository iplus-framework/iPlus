# Skill: Adding a new database field to iPlusV4 (iPlusMES)

This skill describes the complete workflow for adding a **new column to an existing database table** in the iPlus / iPlusMES framework (EF6 Database-First with `*.edmx` + ACClass metadata attributes).

There are **4 steps** that must always be done together:

1. Create a new `dbsync_*.sql` migration script.
2. Add the `[ACPropertyEntity(...)]` attribute to the entity class (`.cs`).
3. Update the `iPlusMESV4.edmx` model (3 places).
4. (Optional) Use the new property in BSO/business code.

---

## Step 1: Create the dbsync SQL script

**Location:** `gip.mes.datamodel\DbScripts\iPlusMES\`

**File naming convention:** `dbsync_yyyy-MM-dd_HH-mm_<username>.sql`
Example: `dbsync_2025-01-10_10-25_aagincic.sql`

### Case A: NOT NULL field (with default value)

A NOT NULL column **cannot be added directly** if the table already contains rows. The script must be done in **3 phases**:

```sql
ALTER TABLE [dbo].[TableName] ADD [NewField] <sqltype> NULL
GO
update [dbo].[TableName] set [NewField] = <default value>
GO
alter table [dbo].[TableName] alter column [NewField] <sqltype> not null
GO
```

Real example (bit field, default 0) — `dbsync_2025-01-10_10-25_aagincic.sql`:

```sql
ALTER TABLE [dbo].[PartslistPos] ADD [KeepBatchCount] bit NULL
GO
update [dbo].[PartslistPos] set [KeepBatchCount] = 0
alter table  [dbo].[PartslistPos] alter column KeepBatchCount bit not null
```

Another example (int field) — `dbsync_2025-11-10_15-00_aagincic.sql`:

```sql
alter table [dbo].[PlanningMRPos] add [Sequence] int null 
GO
update [dbo].[PlanningMRPos] set [Sequence] = 0
GO
alter table PlanningMRPos alter column Sequence int not null
GO
```

### Case B: NULLable field

If the field is nullable, a single statement is enough (no null→not-null conversion needed):

```sql
ALTER TABLE [dbo].[TableName] ADD [NewField] <sqltype> NULL
GO
```

### SQL type mapping (C# → SQL Server)

| C# type | SQL type | EDMX SSDL type | EDMX CSDL type |
|---|---|---|---|
| `double` | `float` | `float` | `Double` |
| `bool` / `bool?` | `bit` | `bit` | `Boolean` |
| `int` / `int?` | `int` | `int` | `Int32` |
| `short` / `short?` | `smallint` | `smallint` | `Int16` |
| `string` | `varchar(n)` / `nvarchar(n)` | `varchar` / `nvarchar` | `String` |
| `Guid` | `uniqueidentifier` | `uniqueidentifier` | `Guid` |
| `DateTime` / `DateTime?` | `datetime` | `datetime` | `DateTime` (Precision="3") |

---

## Step 2: Add the `[ACPropertyEntity]` attribute to the entity class

**Location:** e.g. `gip.mes.datamodel\EntityPartsList\PartslistPos.cs`, `gip.mes.datamodel\EntityPlanningMR\PlanningMRPos.cs`

Add the attribute **always immediately before** the audit-field block (this is the convention):

```csharp
[ACPropertyEntity(496, Const.EntityInsertDate, Const.EntityTransInsertDate)]
```

So the new line goes right after the last "normal" entity property and before `496 InsertDate`:

```csharp
    [ACPropertyEntity(16, nameof(KeepBatchCount), "en{'Keep batch count'}de{'Batchanzahl beibehalten'}", "", "", true)]
    [ACPropertyEntity(496, Const.EntityInsertDate, Const.EntityTransInsertDate)]
    [ACPropertyEntity(497, Const.EntityInsertName, Const.EntityTransInsertName)]
    [ACPropertyEntity(498, Const.EntityUpdateDate, Const.EntityTransUpdateDate)]
    [ACPropertyEntity(499, Const.EntityUpdateName, Const.EntityTransUpdateName)]
```

Rules for the attribute:

- **First argument** = a free index number. Use the next free number of that class (check existing `ACPropertyEntity` indices; 1–9, 11–17 etc. are typically used, never 496–499 which are reserved for audit fields).
- **Second argument** = property name, preferably `nameof(...)` or a string matching the EDMX property name exactly.
- **Third argument** = translation string in the format `"en{'<english>'}de{'<german>'}"`.
- Remaining arguments: `""` (format), `""` (context), `true` (isPropertyEntity).

> Note: The attribute only declares metadata for the AC framework. The actual CLR property is generated from the EDMX (see step 3), so you do **not** write the property body yourself in the entity class.

---

## Step 3: Update `gip.mes.datamodel\iPlusMESV4.edmx`

The EDMX is a single XML file, but the new property must be added in **3 places** (search for the entity name, e.g. `PlanningMRPos`, and find each section):

### 3.1 SSDL — StorageModels (`<Schema xmlns=... Provider="System.Data.SqlClient">`)

```xml
<Property Name="AllocatedQuantityUOM" Type="float" Nullable="false" />
```

Add it inside `<EntityType Name="PlanningMRPos"> ... </EntityType>` in the **StorageModels** section (types are SQL types: `float`, `bit`, `int`, ...).

### 3.2 CSDL — ConceptualModels (`<Schema xmlns=... Namespace="iPlusMESV4_Model">`)

```xml
<Property Name="AllocatedQuantityUOM" Type="Double" Nullable="false" />
```

Add it inside `<EntityType Name="PlanningMRPos"> ... </EntityType>` in the **ConceptualModels** section (types are .NET types: `Double`, `Boolean`, `Int32`, ...).

### 3.3 MSL — Mapping (`<EntitySetMapping Name="PlanningMRPos">`)

```xml
<ScalarProperty Name="AllocatedQuantityUOM" ColumnName="AllocatedQuantityUOM" />
```

Add it inside `<MappingFragment StoreEntitySet="PlanningMRPos">` in the **Mappings** section.

> Tip: To verify you found all 3 places, search the EDMX for an existing field of the same entity (e.g. `KeepBatchCount` in `PartslistPos`) — it must appear exactly 3 times: once in SSDL, once in CSDL, once in MSL.

---

## Step 4: Verify

1. Rebuild `gip.mes.datamodel` — the T4/EDMX code generation must succeed and the new CLR property (e.g. `public double AllocatedQuantityUOM { get; set; }`) must appear in the generated partial class.
2. Run the SQL script against a dev database.
3. Check that the DBSyncer (`gip.core.dbsyncer`) picks up the new script.

---

## Checklist (summary)

- [ ] `dbsync_yyyy-MM-dd_HH-mm_<user>.sql` created in `gip.mes.datamodel\DbScripts\iPlusMES\`
  - [ ] NOT NULL: add NULL → update default → alter to NOT NULL (3 phases)
  - [ ] NULLable: single `ADD ... NULL`
- [ ] `[ACPropertyEntity(<next free index>, nameof(Field), "en{'...'}de{'...'}", "", "", true)]` added **before** the `[ACPropertyEntity(496, Const.EntityInsertDate, ...)]` line
- [ ] EDMX updated in **3 places**: SSDL (`Type="float"`), CSDL (`Type="Double"`), MSL (`ScalarProperty`)
- [ ] Project builds, generated property available
