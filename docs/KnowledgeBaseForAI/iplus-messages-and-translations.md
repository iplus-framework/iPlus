# iPlus: Messages and Translations (ACClassMessage / Msg with TranslID)

*(2026, BSOOPCUABrowser session)*

## Why not hardcode `en{...}de{...}` in ShowMsg

Inline `en{'...'}de{'...'}` strings in code are not translatable by end users.
Messages that users see (MsgBox, MsgBoxList, alarms) must be stored as
**ACClassMessage** rows in the database, so they can be translated to different
languages via BSOiPlusStudio → BSOTranslation (`gip.bso.iplus/BSOiPlus/BSOTranslation.cs`).

At runtime the translation is resolved by the *class* (ACClass) the message belongs to:

```csharp
new Msg(this, eMsgLevel.Error, ClassName, "MethodName", 0, "Error50410");
new Msg(this, eMsgLevel.Warning, ClassName, "AddToSubscription", 0, "Info50416",
        CurrentSubscriptionClass.ACIdentifier, skipped);          // with string.Format args
```

`Msg` → `source.Root.Environment.TranslateMessage(source, translID, args)`
(`gip.core.autocomponent/Environment.cs`) → `ACClass.GetMessage(acIdentifier)`
→ `ACClassMessage.GetTranslation(VBLanguageCode)` → parses the `en{...}de{...}` tuple.
If the message is missing, the text becomes `*No Translation* <ID>`.

Reference usages: `ACPrintManager.cs` (Info50078), `BSOOPCUABrowser.cs`
(Error50410–Error50417, Warning50414, Info50415/50416 — see helper `ShowTranslMsg`).

## Identifier conventions

`ACClassMessage.GetUniqueNameIdentifier(db, prefix, isSystem)`
(`gip.core.datamodel/EntityACClass/ACClassMessage.cs`):

| isSystem | Range        | Example    | Usage |
|----------|--------------|------------|-------|
| false    | 50000–99999  | `Error50410` | developer/customizing messages |
| true     | 00001–49999  | `Error00005` | framework system messages |

`Msg(source, level, className, methodName, sourceRowID, translID)` sets
`ACIdentifier = "<methodName>(<sourceRowID>)"` — that is only the *call site* tag,
the translation lookup uses `TranslID`.

## ACCaptionTranslation tuple format

Stored in `ACClassMessage.ACCaptionTranslation`, plain `varchar(max)` in table `ACClassMessage`:

```
en{'No OPC UA session selected.'}de{'Keine OPC-UA-Session ausgewählt.'}
```

Format rules are enforced by `TranslationTupleHelper` (`gip.core.datamodel.shared/ACHelperclasses/Translator.cs`),
which splits on `{'` and `'}`:

* Quotes around each text are **mandatory** (`en{Text}` without `{'` does not parse!).
* `{0}`-style placeholders are fine (formatted with `string.Format` at display time).
  **Gotcha:** `Environment.TranslateMessageLC` catches `FormatException` silently and returns the
  *unformatted* template — a placeholder/argument-count mismatch (e.g. 3 placeholders, 2 args)
  is only visible as unparsed `{0}`/`{1}` text in the message. Keep placeholder count and the
  `params object[]` list in sync.
* **Gotcha:** an apostrophe *directly followed by `{`* — e.g. `to '{0}'.` — introduces a
  spurious `{'` pair separator and the text is silently dropped. Write `to {0}.` instead.
  Apostrophes elsewhere ("doesn't") are harmless. Check any row with
  `ACCaptionTranslation LIKE '%[a-z]''[a-z]%'`-style queries before assuming it parses.

## How to create a message (3 ways)

1. **Interactive:** BSOiPlusStudio → edit class → "New Message" (see `BSOiPlusStudio_5_Translation.cs`
   → `NewMsg` flow: `GetUniqueNameIdentifier` + `NewACObject` + set ACIdentifier/ACCaption + save).
2. **SQL:** idempotent upsert script — see `docs/sql-scripts/BSOOPCUABrowser_Messages.sql`
   (uses `IF NOT EXISTS … INSERT … ELSE UPDATE`, `InsertName/UpdateName = 'GIP'`, `BranchNo = 0`).
3. **MCP dev tool:** `dev_add_message` (`gip.core.webservices/Components/MCP/MCPIPlusDevTools.cs`)
   — generates the next free identifier for a level prefix and stores the tuple;
   `dev_get_messages` lists existing ones. Both honor `saveChanges=false` + `dev_save_changes`.

## Rule of thumb

* Fixed, user-visible texts → **DB message + TranslID** (translate via BSOTranslation).
* Dynamic/technical texts (exception messages, file names) → plain `Msg` / `en{...}de{...}` inline is acceptable.
* Helper pattern (see `BSOOPCUABrowser.ShowTranslMsg`):

```csharp
private void ShowTranslMsg(eMsgLevel level, string translID, params object[] parameter)
{
    if (Messages == null)
        return;
    Messages.MsgAsync(new Msg(this, level, ClassName, "ShowMsg", 0, translID, parameter));
}
```
