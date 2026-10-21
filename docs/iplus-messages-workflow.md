# iPlus Messages — Complete Workflow (Code ↔ Database)

This guide describes the **complete workflow for iPlus error messages**, applicable to
**any BSO, component or manager class** (`ACClass`):

1. Find where messages are used / needed in code
2. Register the message in the database (`ACClassMessage`)
3. Reference the message in the code

The workflow is verified (established on `BSOBatchPlanSchedulerEmsland`,
messages `Error50801`–`Error50821`, database `EmslandV5` — see §7 Reference).

---

## 0. Cheat sheet (TL;DR)

```text
1. Grep the class for "en{ / Messages.ErrorAsync / new Msg( / errorText
2. Keep attribute captions + TranslateText inline; only ERROR messages go to the DB
3. Find ACClassID of the class that raises the message (subclass → subclass ACClass!)
4. Next free global Error<no>: SELECT MAX(...) FROM ACClassMessage; check collisions
5. Write insert script (double all apostrophes!), save as file, get approval, run
   sqlcmd -f 65001
6. In code: // Error<no>  +  Messages.ErrorAsync(this, "Error<no>")
```

---

## 1. Background — how messages work in iPlus

| Table / Class | Purpose |
|---|---|
| `ACClassMessage` | Central message catalog. One row per message, connected to an `ACClass` (the BSO/component class). |
| `ACClassMessage.ACIdentifier` | Message key, e.g. `Error50801` (`Error` + global running number). |
| `ACClassMessage.ACCaptionTranslation` | Message text in the iPlus translation format: `en{'...'}de{'...'}` (more languages can be appended). |
| `ACClassMessage.ACClassID` | FK to `ACClass` — the message belongs to one class. At runtime the key is resolved against the class of the instance (`this`). |

**Two kinds of texts — do not mix them:**

| Kind | Where it lives | Example |
|---|---|---|
| **Error / user messages** (shown at runtime, should be editable/translateable in DB) | `ACClassMessage` table, referenced by key | `Messages.ErrorAsync(this, "Error50801")` |
| **UI captions** (property/method/command labels, design-time) | Inline in attributes — stays in code | `[ACPropertyInfo(510, "...", "en{'Wizard active'}de{'Assistent aktiv'}")]` |

Only the **first kind** is managed via this workflow. This works identically for
every class type: `TACBSO`, `TACComponent`, managers (`...Manager`), PA modules, etc.

---

## 2. Step 1 — Find messages in code

### 2.1 Find all message usages of a class

Search the class file(s) for these patterns:

```text
Messages.ErrorAsync(          → async error popup
Messages.LogMessageMsg(       → logs an existing Msg object
Messages.LogException(        → exception logging (no DB message needed)
new Msg(this, eMsgLevel...)   → explicit Msg creation
errorText = "..."             → error text returned via out parameter
errorTexts.Add("...")         → collected error texts
"en{                          → any inline translation literal
```

PowerShell one-liner (inventory of any class file):

```powershell
Select-String -Path "<path>\MyClass.cs" -Pattern '"en\{' |
    ForEach-Object { "$($_.LineNumber): $($_.Line.Trim())" }
```

For a whole project:

```powershell
Get-ChildItem "<project>" -Recurse -Filter *.cs |
    Select-String -Pattern 'Messages\.ErrorAsync\(' |
    ForEach-Object { "$($_.Filename):$($_.LineNumber): $($_.Line.Trim())" }
```

### 2.2 Decide for each hit

- **Attribute caption** (`[ACPropertyInfo]`, `[ACMethodInfo]`, `[ACClassInfo]`, `[ACMethodCommand]`, …) → **keep inline**, no DB entry.
- **`TranslateText`** (e.g. phase names, dynamic labels) → **keep inline**, no DB entry.
- **Error message shown to the user** (`Messages.ErrorAsync`, `new Msg(...)`, `errorText`) → **register in DB** (step 3) and **reference by key** (step 4).
- **`Messages.LogException`** → no DB entry needed (logs the exception, no catalog text).

### 2.3 Collect the texts

Extract the `en{...}de{...}` literal of every error message, **deduplicate**
(the same text may appear at several call sites — one DB row, several references).

---

## 3. Step 2 — Register the messages in the database

### 3.1 Determine the target ACClass

```sql
SELECT ACIdentifier, ACClassID FROM ACClass WHERE ACIdentifier LIKE '%MyClassName%';
```

The message must be inserted for the class **whose instance raises it** — the
runtime resolves the key against `this`. Rules:

- Work in the BSO/component itself → its own ACClass.
- **Subclass** (e.g. `MySchedulerEmsland` extends `BSOBatchPlanScheduler`) → use the **subclass** ACClass, even if the base class has its own messages.
- Shared helper/manager raising messages for several callers → the class the method belongs to.

### 3.2 Determine the next free message number

The number in `Error<no>` is **globally unique across all classes** (not per class!):

```sql
-- highest existing error number
SELECT MAX(CAST(SUBSTRING(ACIdentifier, 6, 20) AS INT)) AS MaxErrorNo
FROM ACClassMessage
WHERE ACIdentifier LIKE 'Error%' AND ISNUMERIC(SUBSTRING(ACIdentifier, 6, 20)) = 1;

-- check the proposed block for collisions (adjust the list to your block)
SELECT COUNT(*) AS Collisions FROM ACClassMessage
WHERE ACIdentifier IN ('Error50801','Error50802','Error50803');
```

Pick a block with a safety margin (e.g. max + 30) and verify **no collisions**.

### 3.3 Row conventions

| Column | Value | Notes |
|---|---|---|
| `ACClassMessageID` | `NEWID()` | |
| `ACClassID` | target ACClass GUID | see 3.1 |
| `ACIdentifier` | `Error<no>` | varchar(250) |
| `ACIdentifierKey` | `NULL` | existing rows use NULL |
| `ACCaptionTranslation` | `en{'...'}de{'...'}` | **nvarchar literal** (`N'...'`) for umlauts |
| `BranchNo` | `-1` | all branches |
| `InsertName` / `UpdateName` | `'copilot'` or user name | **varchar(20) — max 20 chars!** |
| `InsertDate` / `UpdateDate` | `GETDATE()` | |

**Placeholders** in the text use `{0}`, `{1}`, … (string.Format style) and are
passed as extra arguments at the call site (see 4.3).

Other message kinds use the same table with a different prefix
(`Question<no>` for confirmations — shown via `Messages.QuestionAsync` /
`Msg` with `eMsgLevel.Question`; same numbering scheme per prefix).

### 3.4 Insert script template

⚠️ **T-SQL pitfall:** every single quote inside the text (also the `'` delimiters
of `en{...}` and apostrophes like `don't`) must be **doubled** (`''`).

```sql
-- =====================================================================
-- PROPOSAL (NOT EXECUTED YET - waiting for approval)
-- Registers the error messages of <ClassName> in ACClassMessage.
-- Numbering: continues the global Error<no> scheme (current max: <max>).
-- Proposed block: Error<start> - Error<end> (verified: no collisions).
-- =====================================================================

DECLARE @ACClassID uniqueidentifier = '<ACClassGUID>'; -- <ClassName>

INSERT INTO ACClassMessage (ACClassMessageID, ACClassID, ACIdentifier, ACIdentifierKey,
                            ACCaptionTranslation, BranchNo, InsertName, InsertDate, UpdateName, UpdateDate)
VALUES
-- <MethodName>: <when is it raised>
(NEWID(), @ACClassID, 'Error<no1>', NULL,
 N'en{''<english text with {0} placeholders>''}de{''<german text>''}',
 -1, 'copilot', GETDATE(), 'copilot', GETDATE()),
-- <MethodName>: <when is it raised>
(NEWID(), @ACClassID, 'Error<no2>', NULL,
 N'en{''<english text>''}de{''<german text>''}',
 -1, 'copilot', GETDATE(), 'copilot', GETDATE());
```

**Save the script** next to the project (convention:
`projects/<Project>/bin/Insert_ACClassMessage_<ClassName>.sql`) so the inserts
are reproducible and reviewable — **propose first, execute after approval**.

### 3.5 Execute with UTF-8 encoding

German umlauts require the code page flag:

```powershell
sqlcmd -S . -d <Database> -U <user> -P <password> -f 65001 -i "Insert_ACClassMessage_<ClassName>.sql"
```

### 3.6 Verify

```sql
SELECT COUNT(*) AS Inserted, MIN(ACIdentifier) AS FirstNo, MAX(ACIdentifier) AS LastNo
FROM ACClassMessage
WHERE ACClassID = '<ACClassGUID>' AND InsertName = 'copilot';

SELECT ACIdentifier, ACCaptionTranslation FROM ACClassMessage
WHERE ACClassID = '<ACClassGUID>' AND InsertName = 'copilot'
ORDER BY ACIdentifier;   -- check umlauts!
```

---

## 4. Step 3 — Reference the message in the code

Replace the inline `en{...}de{...}` literal with the message key **string**.
The iPlus message infrastructure resolves the key against the ACClass of `this`.

### 4.1 Direct error popup

```csharp
// Error<no>
Messages.ErrorAsync(this, "Error<no>");
```

### 4.2 Error text via out parameter / collected list

The key is just a string — it flows through and is resolved when shown:

```csharp
// Error<no>
errorText = "Error<no>";
...
errorTexts.Add("Error<no>");
...
Messages.ErrorAsync(this, errorText);   // resolves the key
```

### 4.3 Explicit Msg with parameters (base framework pattern)

```csharp
// Error<no>
Msg msg = new Msg(this, eMsgLevel.Error, GetACUrl(), "<MethodName>", <line>, "Error<no>", arg0, arg1);
```

The extra arguments fill the `{0}`, `{1}` placeholders of the DB text.

### 4.4 Conventions

- Put a `// Error<no>` comment on the line before each call (same style as the base framework).
- One DB row per **distinct text** — reference the same key from multiple call sites.
- Keep the mapping table (number ↔ text ↔ call sites) in the insert script as SQL comments.
- When you later add NEW messages to the same class, continue the numbering block of that class and append them to the same insert script (or a new dated one).

---

## 5. Complete generic example

**Code before** (any BSO):

```csharp
Messages.ErrorAsync(this,
    "en{'Please select a facility!'}de{'Bitte einen Lagerplatz auswählen!'}");
```

**DB row** (ACClass of that BSO, next free number — here `Error51234`):

```sql
(NEWID(), @ACClassID, 'Error51234', NULL,
 N'en{''Please select a facility!''}de{''Bitte einen Lagerplatz auswählen!''}',
 -1, 'copilot', GETDATE(), 'copilot', GETDATE()),
```

**Code after:**

```csharp
// Error51234
Messages.ErrorAsync(this, "Error51234");
```

**With parameter** — DB text `en{'Facility {0} is not valid!'}de{'Lagerplatz {0} ist ungültig!'}`:

```csharp
// Error51235
Messages.ErrorAsync(this, "Error51235", false, facilityNo);
```

---

## 6. Pitfalls checklist

- [ ] **varchar(20)**: `InsertName`/`UpdateName` are varchar(**20**) — longer names truncate (error).
- [ ] **Apostrophes**: double all `'` inside the T-SQL nvarchar literal (`don't` → `don''t`, `en{'` → `en{''`).
- [ ] **Umlauts**: execute with `sqlcmd -f 65001` (UTF-8), otherwise `ä/ö/ü` break.
- [ ] **N-prefix**: always `N'...'` for `ACCaptionTranslation`.
- [ ] **Global numbering**: `Error<no>` is unique across ALL classes — always check max + collisions.
- [ ] **Right ACClass**: the key resolves against the ACClass of the instance raising the message — for subclasses use the subclass ACClass.
- [ ] **Deduplicate**: same text at several call sites = one DB row.
- [ ] **Don't touch captions**: attribute captions and `TranslateText` literals stay inline in code.
- [ ] **Propose first**: save the script as a file, let the DB owner review/approve, then execute.

---

## 7. Reference implementation (framework classes)

Complete examples **in the iPlus framework itself** — classes whose messages are
registered in `ACClassMessage` and referenced by key in code:

| Pattern | Class | Example |
|---|---|---|
| `new Msg(...)` with key + parameter | `iPlusMES/gip.mes.facility/Manager/ACPickingManager.cs` (≈ line 1478) | `new Msg(this, eMsgLevel.Error, nameof(ACPickingManager), nameof(ValidateStart), 20, "Error50765", picking.PickingNo)` — DB row `Error50765` (text with `{0}`) |
| `new Msg(...)` with key + parameter | `iPlusMES/gip.mes.facility/Manager/FacilityManagerBooking.cs` (≈ line 2415) | `new Msg(this, eMsgLevel.Error, nameof(ReassignLotToFacilityCharges), nameof(FacilityManager), 30, "Error50761", saveChangesMsg.DetailsAsText)` — DB row `Error50761` |
| `Messages.ErrorAsync(this, "Error<no>")` | `iPlusMES/gip.bso.manufacturing/Manufacturing/BSOMaterialPreparation.cs` (≈ line 950) | `Messages.ErrorAsync(this, "Error50657")` |
| `Messages.ErrorAsync(this, "Error<no>", false, args...)` with `{0}` placeholders | `iPlusMES/gip.bso.manufacturing/Manufacturing/BSOFacilityInventory.cs` (≈ line 2299) | `Messages.ErrorAsync(this, "Error50560", false, ...FacilityNo, ...FacilityName, ...)` |
| Many keys in one class (numbering block per class) | `iPlusMES/gip.bso.manufacturing/Manufacturing/WorkCenterSelector/BSOManualWeighing.cs` | `Error50283`, `Error50285`, `Error50288`–`Error50292`, … |

To see the DB rows of one of these classes:

```sql
SELECT m.ACIdentifier, m.ACCaptionTranslation
FROM ACClassMessage m
JOIN ACClass a ON a.ACClassID = m.ACClassID
WHERE a.ACIdentifier = 'ACPickingManager'
ORDER BY m.ACIdentifier;
```
