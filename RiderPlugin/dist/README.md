# Pre-built plugin zips

Drop-in installable artifacts of the SharedMeta Rider plugin. The latest committed
`rider-sharedmeta-*.zip` here is what users without a Java/Gradle toolchain should
install — see the install instructions in the [parent README](../README.md).

When a new version ships, replace (don't append) the previous zip and bump the
version in `gradle.properties` so the repo always carries exactly one ready-to-use
build alongside the source.

To rebuild manually after a source change:

```bash
cd ..
./gradlew buildPlugin                   # produces build/distributions/rider-sharedmeta-X.Y.Z.zip
cp build/distributions/rider-sharedmeta-*.zip dist/   # promote to the tracked slot
```
