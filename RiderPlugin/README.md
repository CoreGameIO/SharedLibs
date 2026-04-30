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
        ├── ZoneMarker.cs                  ReSharper activation zones
        ├── MetaServiceMatcher.cs          attribute-driven interface ↔ generated mapping
        └── MetaMethodSearcherFactory.cs   Find Usages bridge
```

`MetaServiceMatcher` knows three attribute FQNs (`MetaService`, `MetaMethod`,
`GeneratedFromMetaMethod`) and uses ReSharper's `AnnotatedMembersEx` index for the
forward direction (interface → generated). The reverse direction (generated → interface)
is a single attribute read on the generated method.

`MetaMethodSearcherFactory` subclasses `DomainSpecificSearcherFactoryBase` and overrides
only `IsCompatibleWithLanguage` (C# only) and `GetRelatedDeclaredElements` (returns the
matching counterparts).

## Out of scope (planned for later)

* **Inline "N usages" code-vision counter.** The hover popup shows the correct merged
  count via Find Usages, but the inline indicator above each method declaration is
  computed by `ReferencesCodeInsightsProvider` (an internal ReSharper provider) which
  doesn't consult `GetRelatedDeclaredElements`. Subclassing it is fragile across Rider
  releases — deferred until we judge the cosmetic gap worth the maintenance cost.
* **Explicit "Go to MetaService method" / "Go to ApiClient method" actions.** Standard
  Find Usages plus Ctrl+Click on the call site cover most navigation today.
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
