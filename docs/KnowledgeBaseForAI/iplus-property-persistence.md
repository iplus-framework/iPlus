# iPlus — Where [ACProperty*] Values are Persisted (and how to read them without an instance)

Status: **complete.** Companion to `opcua-browser-bso.md` and `iplus-client-server-components.md`.
Target workspace: `V5/iPlus` (framework). Online docs: https://iplus-framework.com/en/documentation (chapters on
component configuration, see links in the session history).

---

## 1. The four storage locations (do not mix them up!)

| Declared as | Stored in | Written by | Restored by |
|---|---|---|---|
| `[ACPropertyConfig(...)]` (e.g. `IACContainerTNet<T>` / config container, see `S7TCPSession`) | **`ACClassConfig`** table — `LocalConfigACUrl` = property name, `KeyACUrl` = `ACClass.ACConfigKeyACUrl`, value in `Value` (DataContract-serialized) | `ACComponent.SetConfigurationValue()` | `ACConfig`-containers on `ACInit` |
| `[ACPropertyInfo]` on a component (plain property, e.g. `OPCUAClientACSession.EndpointURL`) | **`ACClassProperty.XMLValue`** on the property rows of the instance's `ACClass` node in the application tree (instance property rows reference their declaration via `BasedOnACClassPropertyID`) | the `ACProperty<T>` persistence when the value changes | `ACProperty.ReStoreFromDB() → RestoreRuntimeValue()/RestoreValue()` ([ACProperty.cs](../../gip.core.autocomponent/ACProperty/ACProperty/ACProperty.cs#L637-L646)) |
| same property at **runtime** of a running component instance | **`ACClassTaskValue.XMLValue`** (keyed by `ACClassPropertyID == ACType.ACTypeID`), backup additionally in `XMLValue2` | same mechanism as above | `RestoreRuntimeValue()` / `RestoreBackupedValue()` |
| **Virtual extended property on `ACClassProperty`** (property whose `ConfigACClass`/`ACPropertyEntity` extends the
  `ACClassProperty` entity itself, e.g. `OPCItemConfig` — resolved via `property[configClassName]` /
  `property.ConfigValue`) | **`ACClassProperty.XMLConfig`** — the *column on the property row itself*, serialized by
  `ACPropertyManager.Serialize()` ([ACPropertyManager.cs](../../gip.core.datamodel/ACHelperclasses/EntityTypeExtension/ACPropertyManager.cs#L235)) — **NOT** in `ACClassConfig` rows! | `property[name] = value` (VBEntityObject indexer → `ACPropertyManager.SetACPropertyExtValue` → `Serialize`) | `property[name]` get → `ACPropertyManager.Deserialize()` deserializes `_EntityProperty.XMLConfig` |

**Key facts:**

- A plain `[ACPropertyInfo]`-property on an `ACComponent` is **NOT** stored in `ACClassConfig` — it is stored in
  `ACClassProperty.XMLValue` of the `ACClassProperty` rows that belong to the configured instance (`ACClass` node).
- `ACClassProperty.Value` is just an alias: `get => XMLValue; set => XMLValue = (String)value;`
  ([ACClassProperty.cs](../../gip.core.datamodel/EntityACClass/ACClassProperty.cs#L1631-L1650)).
- `RestoreValue()` ([ACProperty.cs](../../gip.core.autocomponent/ACProperty/ACProperty/ACProperty.cs#L945)) defines the
  serialization: `string` → as-is; `DateTime` → "o", `TimeSpan` → "c"; enums/`IConvertible` → parsed via
  `ACConvert.ChangeType(xmlValue, targetType, ...)`; complex types → `DataContractSerializer` XML.
- **Reading a config of a virtual extended property**: use the established runtime pattern
  `(opcProperty.ACType as ACClassProperty)["OPCItemConfig"]` (see `OPCUAClientACSubscr.InitSubscription`).
  `GetConfigListOfType()` / `GetConfigurationValue()` do **not** see these values — they only scan `ACClassConfig`
  rows, which stay empty for this kind of property. Gate the indexer read on
  `!String.IsNullOrEmpty(c.XMLConfig)` because the plain indexer get has `autoCreate=true` and can create entries
  as a side effect.

## 2. SQL: find the configured values of all OPCUAClientACSession instances

```sql
-- persisted [ACPropertyInfo] values per instance (instance rows carry XMLValue)
select c.ACIdentifier, p.ACIdentifier, p.XMLValue
from ACClassProperty p
inner join ACClass c on c.ACClassID = p.ACClassID
inner join ACClass c1 on c1.ACClassID = c.BasedOnACClassID
where c1.ACIdentifier = 'OPCUAClientACSession' and p.BasedOnACClassPropertyID is not null;

-- in contrast: ACClassConfig rows are only written for [ACPropertyConfig] properties (not used by OPCUAClientACSession)
select c.ACIdentifier, p.XMLConfig
from ACClassConfig p
inner join ACClass c on c.ACClassID = p.ACClassID
inner join ACClass c1 on c1.ACClassID = c.BasedOnACClassID
where c1.ACIdentifier = 'OPCUAClientACSession';
```

## 3. Reading values without a real component instance (client/proxy mode)

When the component only exists as `ACComponentProxy` in the client process, its .NET class is NOT loaded — a cast
or method call would be forwarded to the server. But the configuration data is metadata and fully available on the
client. Proxy-safe recipe (implemented in `BSOOPCUABrowser.FindConfiguredSessions/GetConfigValue`):

1. Walk the component tree and detect the type **by `ACClass` only**:
   `component.ACType` plus `ACClass.ClassHierarchy.Any(c => c.ACIdentifier == <ClassName>)` and `component.IsProxy`.
2. Read the persisted value from the `ACClassProperty` rows:

```csharp
ACClassProperty prop = configClass.ACClassProperty_ACClass
    .Where(c => c.ACIdentifier == propertyACIdentifier && !String.IsNullOrEmpty(c.XMLValue))
    .FirstOrDefault();                     // own rows of the instance node
// …then fall back to configClass.ACClass1_ParentACClass and repeat (class hierarchy)
```

3. Deserialize with the `RestoreValue` rules (string as-is; enum via `Enum.TryParse(xml, true)` with numeric fallback;
   otherwise `ACConvert.ChangeType(xmlValue, targetType, true, Database.GlobalDatabase)`).

## 4. Pitfalls

- **`GetConfigurationValue(name)` is the ACClassConfig path.** It searches `ACType.ConfigurationEntries`
  (`KeyACUrl == ACConfigKeyACUrl`, `LocalConfigACUrl == name`). Wrong storage for plain `[ACPropertyInfo]`
  values — it will return null for them.
- **Configured instance vs. class definition.** The value rows live on the *instance* `ACClass` row of the app tree
  (or are inherited via `ACClass1_ParentACClass`), not on the definition row of `OPCUAClientACSession` itself.
- **Enum default.** If no row/XMLValue exists, the property gets its .NET default (`UserAuthenticationMode` = 0 =
  `Anonymous`) — treat "no row" as "default value".
- **`ACClassTaskValue` vs `ACClassProperty`**: runtime changes of a *running* instance go to
  `ACClassTaskValue` (per started instance/task). A browser that should see the *configured* values must read
  `ACClassProperty.XMLValue` from the `ACClass`, not the live task values.
