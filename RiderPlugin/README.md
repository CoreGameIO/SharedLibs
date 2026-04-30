# SharedMeta — Rider plugin

Bridges `[MetaService]` interface methods with every code-generated counterpart so that
**Find Usages** walks both sides in one pass.

## What it does

For every `[MetaMethod]` on a `[MetaService]` interface, SharedMeta.Generator emits
client-side methods in five generated layouts:

| Generator                         | Generated counterpart                              |
| --------------------------------- | -------------------------------------------------- |
| `SimplifiedApiClientGenerator`    | `*ApiClient.{Name}Async / {Name}Sync / {Name}Signal` |
| `QueryClientGenerator`            | `*EntityQueryApi.{Name}Async`                      |
| `ContextInjectionGenerator`       | `{I}EntityCaller.{Name}Async` (interface)          |
|                                   | + `*EntityRecorder` / `*EntityReplayer` / `*LocalEntityCaller` (impls) |

Each generated method carries `[GeneratedFromMetaMethod(typeof(IFoo), "Bar")]`. The
plugin reads that attribute (no naming-convention guessing, no namespace prediction)
and tells ReSharper's Find Usages to expand the search root with the matching
counterparts. Result: **Find Usages on the interface method** surfaces every call site
in the project, including those that go through cross-entity callers in arbitrary
namespaces.

In addition the plugin contributes two entries to Rider's **Ctrl+Shift+G "Navigate To"**
popup:

* on a generated client method — *"MetaService Method"* jumps to the originating
  `[MetaMethod]` on the interface;
* on a `[MetaMethod]` — *"Generated Client Method"* jumps to a counterpart.

Standard Rider Ctrl+Click / Go to Implementation behaviour is left untouched (the
override that briefly tried to inject targets there in 0.3.x was reverted because it
replaced rather than augmented the default targets).

## Compatibility

* **Rider 2025.3.4** (build `253.*`)
* **Java 21**
* **.NET SDK 8.0+**
* **SharedMeta ≥ 0.16.0** — plugin 0.2 reads the `[GeneratedFromMetaMethod]` attribute
  emitted by the 0.16.0 generators. Older generated code is invisible to the plugin
  (regenerate against ≥ 0.16.0).

## Install in Rider

A ready-to-install zip is committed under [`dist/`](dist/) — use it if you don't
want to set up Java/Gradle:

1. **Settings → Plugins → ⚙ → Install Plugin from Disk…**
2. Pick `dist/rider-sharedmeta-X.Y.Z.zip` (latest version in the folder)
3. Restart Rider

## Build from source (only if you change plugin code)

```bash
cd SharedLibs/RiderPlugin
./gradlew buildPlugin
# Output: build/distributions/rider-sharedmeta-X.Y.Z.zip
# Promote to the tracked slot when ready to ship:
cp build/distributions/rider-sharedmeta-*.zip dist/
```

The Gradle build invokes `dotnet build` for the ReSharper backend automatically and
wraps the resulting DLL into the plugin zip under `rider-sharedmeta/dotnet/`.

## Run from sources (dev loop)

```bash
./gradlew runIde
```

Spawns a sandboxed Rider instance with the plugin loaded — useful for iterating on
backend code without reinstalling.

## Project layout

```
RiderPlugin/
├── build.gradle.kts                  IntelliJ Platform 2.x build, wires .NET → sandbox
├── settings.gradle.kts
├── gradle.properties                 plugin/SDK versions
├── gradle/libs.versions.toml         plugin catalog
├── src/main/kotlin/                  empty stub (front-end has no logic)
├── src/main/resources/META-INF/
│   └── plugin.xml                    plugin manifest
└── src/dotnet/
    ├── Directory.Build.props         RiderSdkVersion = 2025.3.4
    ├── Directory.Packages.props      opts out of parent CPM
    ├── ReSharperPlugin.SharedMeta.sln
    └── ReSharperPlugin.SharedMeta/
        ├── ZoneMarker.cs                     ReSharper activation zones
        ├── DiagLog.cs                        opt-in file logging (env-var gated)
        ├── MetaServiceMatcher.cs             attribute-driven interface ↔ generated mapping
        ├── MetaMethodSearcherFactory.cs      Find Usages bridge
        └── MetaServiceNavigationProvider.cs  Ctrl+Shift+G popup contributor
```

`MetaServiceMatcher` is the single source of truth for the SharedMeta naming/attribute
contract — it inspects `[MetaService]`, `[MetaMethod]`, and `[GeneratedFromMetaMethod]`
attributes and finds counterparts in either direction.

`MetaMethodSearcherFactory` subclasses `DomainSpecificSearcherFactoryBase` and overrides
only `IsCompatibleWithLanguage` (C# only) and `GetRelatedDeclaredElements` (returns the
matching counterparts). This is what makes Find Usages bidirectional.

`MetaServiceNavigationProvider` implements `INavigateFromHereImportantProvider` and
contributes the *"MetaService Method"* / *"Generated Client Method"* entries to the
Ctrl+Shift+G "Navigate To" popup.

`DiagLog` writes structured timestamps to `%TEMP%/sharedmeta-rider-plugin.log` only
when the environment variable `SHAREDMETA_RIDER_PLUGIN_DEBUG=1` is set at Rider start.
Useful for diagnosing why the plugin's hooks aren't firing.

## Out of scope (planned for later)

* **Right-click → Go To submenu integration.** That submenu is built from a static
  IntelliJ-frontend action tree, not from ReSharper backend providers. The
  documented `BackendAction` proxy + C# `[Action]`/`IExecutableAction` pattern was
  attempted in 0.4.x but never registered the C# handler in Rider 2025.3 (verified
  via DiagLog — the action class's static constructor is never invoked, indicating
  the assembly is silently filtered from the action catalog despite all the
  documented prerequisites). The next iteration will move to a pure-Kotlin frontend
  Action with its own RD-protocol model into the backend.
* **Inline "N usages" code-vision counter.** The hover popup shows the correct merged
  count via Find Usages, but the inline indicator above each method declaration is
  computed by `ReferencesCodeInsightsProvider` (an internal ReSharper provider) which
  doesn't consult `GetRelatedDeclaredElements`. Subclassing it is fragile across Rider
  releases.
* Triggers (`[Trigger]`) and subscribers (`SubscriberInterfaces` on `[MetaService]`).
* Gutter icons.
* JetBrains Marketplace publishing — local zip only.

## Known caveats

* **Old generated code is invisible.** Anything compiled against SharedMeta < 0.16.0
  has no `[GeneratedFromMetaMethod]` attribute and the plugin can't bridge it.
  Regenerate against the new SharedMeta version.
* The ReSharper SDK Find Usages API can shift between Rider releases. If a future
  Rider build breaks `DomainSpecificSearcherFactoryBase` or `AnnotatedMembersEx`,
  the fix is local to `MetaServiceMatcher.cs` / `MetaMethodSearcherFactory.cs`.
* The parent `SharedLibs/Directory.Packages.props` enables Central Package Management.
  The plugin opts out via a local `Directory.Packages.props` so it can pin its own
  Rider SDK version independently of other SharedLibs projects.
