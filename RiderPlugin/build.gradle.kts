import org.jetbrains.intellij.platform.gradle.IntelliJPlatformType
import org.jetbrains.intellij.platform.gradle.tasks.PrepareSandboxTask

plugins {
    id("java")
    alias(libs.plugins.kotlin)
    alias(libs.plugins.intellijPlatform)
}

group = providers.gradleProperty("pluginGroup").get()
version = providers.gradleProperty("pluginVersion").get()

repositories {
    mavenCentral()
    intellijPlatform {
        defaultRepositories()
    }
}

dependencies {
    intellijPlatform {
        rider(providers.gradleProperty("platformVersion"))
        instrumentationTools()
    }
}

kotlin {
    jvmToolchain(providers.gradleProperty("javaVersion").get().toInt())
}

intellijPlatform {
    pluginConfiguration {
        name = providers.gradleProperty("pluginName")
        version = providers.gradleProperty("pluginVersion")
        ideaVersion {
            sinceBuild = providers.gradleProperty("pluginSinceBuild")
            untilBuild = providers.gradleProperty("pluginUntilBuild")
        }
        vendor {
            name = "CoreGame"
            url = providers.gradleProperty("pluginRepositoryUrl")
        }
    }
    publishing {
        // Local-zip distribution only — no Marketplace publish for now.
    }
    // pluginVerification deliberately omitted — `ides { recommended() }` clashes
    // with Gradle's configuration cache ("Adding a provider of configurations
    // directly to the configuration container is not allowed"). Re-enable when
    // we want to run JetBrains Plugin Verifier against multiple Rider builds.
}

// ----------------------------------------------------------------------------
// .NET backend build — invoked before the IDE sandbox is prepared so the
// plugin .dll lands in the resulting plugin layout under <plugin>/dotnet/.
// ----------------------------------------------------------------------------

val dotnetPluginId = providers.gradleProperty("dotnetPluginId").get()
val dotnetTargetFramework = providers.gradleProperty("dotnetTargetFramework").get()
val dotnetBuildConfiguration = providers.gradleProperty("dotnetBuildConfiguration").get()

val dotnetSrcDir = layout.projectDirectory.dir("src/dotnet")
val dotnetSlnFile = dotnetSrcDir.file("$dotnetPluginId.sln")
val dotnetOutputDir = dotnetSrcDir.dir("$dotnetPluginId/bin/$dotnetBuildConfiguration/$dotnetTargetFramework")

val buildBackend by tasks.registering(Exec::class) {
    group = "build"
    description = "Build the .NET ReSharper backend plugin."
    workingDir = dotnetSrcDir.asFile
    commandLine("dotnet", "build", dotnetSlnFile.asFile.absolutePath, "-c", dotnetBuildConfiguration)
    inputs.dir(dotnetSrcDir).withPropertyName("dotnetSource").withPathSensitivity(org.gradle.api.tasks.PathSensitivity.RELATIVE)
    outputs.dir(dotnetOutputDir).withPropertyName("dotnetOutput")
}

tasks.withType<PrepareSandboxTask>().configureEach {
    dependsOn(buildBackend)
    from(dotnetOutputDir) {
        include("$dotnetPluginId.dll")
        include("$dotnetPluginId.pdb")
        // The sandbox plugin folder is named after `rootProject.name`, not the
        // `pluginConfiguration.name` value. Picking the wrong one leaves the
        // DLL outside the plugin tree and it silently disappears from the zip.
        into("${rootProject.name}/dotnet")
    }
}

// ----------------------------------------------------------------------------
// Convenience: produce the distributable zip via standard `buildPlugin` task.
// `./gradlew buildPlugin` -> build/distributions/SharedMeta-<version>.zip
// ----------------------------------------------------------------------------

// `buildSearchableOptions` boots the IDE headlessly to index Settings entries
// for searchability. Our plugin contributes no Settings UI, and the task
// reliably fails on Rider 2025.3 with "Index: 1, Size: 1". Disabling it is
// the standard recommendation in JetBrains plugin docs when no searchable
// options exist; `prepareSandbox` / `buildPlugin` no longer depend on it.
tasks.named("buildSearchableOptions") {
    enabled = false
}
