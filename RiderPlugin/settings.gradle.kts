pluginManagement {
    repositories {
        gradlePluginPortal()
        mavenCentral()
    }
}

// Foojay: lets `kotlin { jvmToolchain(21) }` auto-download a matching JDK if
// none is locally registered. Without this, Gradle fails on machines where
// the only JDK is Rider's bundled JBR (not in the standard discovery paths)
// with "Cannot find a Java installation matching languageVersion=21".
plugins {
    // 0.7.0 — broadest compatibility (Gradle 7.6+).
    // Newer Foojay (0.8.x) references JvmVendorSpec.IBM_SEMERU which only
    // exists in Gradle ≥ 8.5; Rider's bundled Gradle is sometimes older.
    id("org.gradle.toolchains.foojay-resolver-convention") version "0.7.0"
}

// Note: we deliberately do NOT set RepositoriesMode.FAIL_ON_PROJECT_REPOS here.
// The IntelliJ Platform Gradle Plugin contributes its own repositories via the
// `intellijPlatform { defaultRepositories() }` block in build.gradle.kts, which
// is project-level — making the strict mode incompatible. Letting the build
// script declare repositories is the supported pattern for this plugin.

rootProject.name = "rider-sharedmeta"
