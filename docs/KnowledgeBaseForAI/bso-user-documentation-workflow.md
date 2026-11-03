# Documenting a Business Object (BSO) for Users and AI Agents — Recipe

How to create/complete the human-readable documentation **and** the MCP-readable
metadata of an iPlus business object, without searching the whole code base.
Written so a fresh AI agent can execute this task directly.

Reference implementations: [BSOMaterial.cs](../../../iPlusMES/gip.bso.masterdata/Material/BSOMaterial.cs)
and [BSOPartsList.cs](../../../iPlusMES/gip.bso.masterdata/Partslist/BSOPartsList.cs)
(user manuals: `iPlusMES/gip.bso.masterdata/docs/BSOMaterial.md` and `BSOPartslist.md`).

Related: [iplus-designs-and-bso-binding.md](./iplus-designs-and-bso-binding.md)

---

## 1. Locate the BSO

BSOs live in `gip.bso.*` assemblies (e.g. `iPlusMES/gip.bso.masterdata/<Module>/<BSO>.cs`).
Find them with: `grep -l "ACClassInfo" gip.bso.*/**/*.cs` or GitHub code search
`org:iplus-framework <BSOName>`.

## 2. Make the class summary MCP-ready (source of truth)

The **class-level `<summary>`** (and the `ACClassInfo(..., Description = @"...")` attribute)
is what the AI agent reads via the iPlus MCP Server (`get_type_infos`). It must be a complete
**usage recipe**, in this style (copy the pattern from BSOMaterial/BSOPartslist):

1. What the BSO is for (1–2 sentences).
2. Search: `SearchWord` → result list property (`...List`) → `NavigateFirst()` →
   `Current<Entity>` holds the selected record.
3. Edit rule: change properties on `Current<Entity>` only, then call `Save()` before
   navigating or creating new records.
4. `New()` → fill required fields → `Save()`.
5. `Delete()` (note FK constraints) → `Save()`.
6. `Load()` for background changes by other users.
7. Special workflows of this BSO (e.g. `SetMaterialWF`, `NewIntermediateParts`, ...).
8. Link: `Visit https://github.com/search?q=org%3Aiplus-framework%20<BSOName>&type=code ...`

Keep the `ACClassInfo` Description attribute **in sync** with the summary (same content).

**Important — the `Description` property is the MCP channel.** The agent does not read
the C# file; it reads the metadata **imported into the database**. Therefore every
`ACProperty...` and `ACMethod...` attribute must carry a meaningful `Description =`
(and where applicable a localized caption `"en{'...'}de{'...'}"`). These are imported
into `ACClassProperty` / `ACClassMethod` (+ `ACClassText` for translations) and are
exposed to the agent via `get_type_infos` / `get_method_info`. An attribute without a
description is invisible/meaningless to the agent.

**CRITICAL — caption tuple syntax.** A localized caption MUST be
`"en{'English text'}de{'Deutscher Text'}"` — the quotes **inside** the braces are
mandatory. The runtime parser (`Translator.GetUpdatedTranslation`) scans for the
literal markers `{'` and `'}`; a tuple like `"en{English text}de{Deutscher Text}"`
(missing quotes) makes `Substring()` throw
`System.ArgumentOutOfRangeException`. Also never mix (e.g. `de{Text'}` is invalid).

## 3. Collect the facts

* **Properties**: grep `[ACPropertyCurrent|ACPropertySelected|ACPropertyList|ACPropertyInfo]`
  in the BSO file — these are the bindable UI properties (usage index: 10 Current,
  20 Selected, 30 List, 40 Info).
* **Methods + button captions**: grep `[ACMethodInteraction|ACMethodCommand|ACMethodInfo]` —
  the second argument is the **button caption** the user sees
  (e.g. `SetMaterialWF` → `"en{'Assign Materialworkflow'}de{'Materialworkflow zuweisen'}"`).
  This mapping C# method ↔ caption is essential for both the manual and the agent.
* **Designs (UI)**: designs are rows in `ACClassDesign` (see designs guide §1), not XAML
  files. The BSO screen is **nested**: `Mainlayout` embeds `*Explorer`/`*MainForm`;
  `MainForm` embeds the tab designs (`*TabPartslistMain`, `*TabComponents`,
  `*TabIntermediate`, `*TabWorkflow`, `*TabDetails`, `*TabBOM`, ...); each tab embeds
  list/form designs (`*ComponentList`, `*ComponentForm`, ...). Read them to know which
  tab contains which fields and buttons. Inspect read-only via SQL on the customer DB
  (e.g. `IPCDrivesV5` on `gipDLVmSQL1.incus`):

  ```bash
  # -y 0 is mandatory: without it sqlcmd truncates nvarchar(max) at 256 bytes!
  sqlcmd -S gipDLVmSQL1.incus -d IPCDrivesV5 -U gip -P ... -No -y 0 \
    -Q "SET NOCOUNT ON; SELECT CAST(XMLDesign2 AS nvarchar(max)) FROM ACClassDesign \
        WHERE ACClassID=(SELECT ACClassID FROM ACClass WHERE ACIdentifier='BSOPartslist') \
        AND ACIdentifier='MainForm';" -o MainForm.xaml
  ```

  ```sql
  -- list designs of a BSO (XMLDesign = WPF, XMLDesign2 = Avalonia)
  SELECT d.ACIdentifier, datalength(d.XMLDesign) AS lenWpf, datalength(d.XMLDesign2) AS lenAva
  FROM ACClassDesign d JOIN ACClass c ON c.ACClassID = d.ACClassID
  WHERE c.ACIdentifier = 'BSOPartslist' ORDER BY d.ACIdentifier;
  ```

  Useful extractions from a design file:
  * `VBContent="*Name"` → embedded sub-designs (the tab structure)
  * `VBContent="PropertyName"` / `"Selected\Child"` → bound fields on screen
  * `VBContent="!MethodName"` → buttons (caption = method translation, see above)
  * `ACCaption="..."` → static headers

  Button captions can also be queried directly:

  ```sql
  SELECT t.ACIdentifier, t.ACCaptionTranslation FROM ACClassText t
  JOIN ACClass c ON c.ACClassID = t.ACClassID
  WHERE c.ACIdentifier = 'BSOPartslist';
  ```

* **Existing end-user docs**: https://iplus-framework.com/en/documentation/Home/Schema/View/e0703b3b-647a-46a6-8c53-8706e9cb128b
  — search for the module; reuse/complete its content.

## 4. Write the user manual

Create `iPlusMES/gip.bso.<module>/docs/<BSOName>.md` with this template:

```markdown
# <Title>
> Business object: `<BSOName>` (Assembly `gip.bso.<module>`, Menu: **...**)
## Definition
## Usage / Concepts (domain terms table)
## Screen layout (tabs from the designs, nested TabItems!)
## Step-by-step tasks (numbered, **button captions** in bold)
## Field reference (table, field labels as shown in the UI)
## For AI agents (MCP)  — short note pointing to the class summary
```

**Language rules for the manual (human-readable!):**

* Use the **captions the user sees on screen** (from `ACMethod*` captions and the
  designs), **never C# identifiers**: write **"Assign Materialworkflow"**, not
  `SetMaterialWF`; **"New Component"**, not `NewPartslistPos`.
* Field labels as shown in the UI ("Target Quantity", not `TargetQuantityUOM`).
* C# method/property names belong only in the class summary (§2) and, if helpful,
  in a short "For AI agents (MCP)" appendix at the end of the manual.

## 5. Verify

* The summary and `ACClassInfo.Description` are identical in content.
* Every public workflow method of the BSO is mentioned in the manual.
* The manual compiles as Markdown; relative links work.
* Add the new doc to this KnowledgeBase index if a new module was covered.
