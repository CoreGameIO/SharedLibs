package com.coregame.sharedmeta.rider

import com.intellij.openapi.actionSystem.ActionManager
import com.intellij.openapi.actionSystem.ActionUpdateThread
import com.intellij.openapi.actionSystem.AnAction
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.actionSystem.CommonDataKeys
import java.io.File
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

private val LogPath = File(System.getProperty("java.io.tmpdir"), "sharedmeta-rider-plugin.log")
private val LogEnabled = System.getenv("SHAREDMETA_RIDER_PLUGIN_DEBUG") == "1"
private val Ts = DateTimeFormatter.ofPattern("HH:mm:ss.SSS")

private fun frontendLog(line: String) {
    if (!LogEnabled) return
    try {
        LogPath.appendText("[${LocalDateTime.now().format(Ts)} kotlin] $line\n")
    } catch (_: Throwable) { /* swallow */ }
}

/**
 * Lets users reach the SharedMeta navigation entries from Right-click → **Go To**
 * (and from the global Navigate menu) without learning the Ctrl+Shift+G shortcut.
 *
 * The actual jump-to-target entries — *MetaService Method* / *Generated Client
 * Method* — are contributed on the .NET backend by `MetaServiceNavigationProvider`
 * (`INavigateFromHereImportantProvider`). That provider only feeds the dynamic
 * Ctrl+Shift+G "Navigate To" popup; its entries do **not** end up in the static
 * IntelliJ-side Right-click → Go To submenu, which is built from a fixed action
 * tree on the JVM frontend and does not consult ReSharper's dynamic providers.
 *
 * This action sidesteps the problem by re-invoking the same popup the user could
 * already reach with Ctrl+Shift+G — which gets us the SharedMeta entries plus
 * every other Rider navigation, one extra menu hop away from a direct jump.
 *
 * **Why not direct one-click navigation?** The popup entry's <code>actionId</code>
 * (`GoToSharedMetaServiceMethod` …) is purely a backend-internal marker; the
 * frontend `ActionManager` has no handle on the backend `ContextNavigation`
 * `Execution` delegate, so we cannot invoke it without a custom RD model.
 * Verified empirically in 0.5.5 — searching frontend `ActionManager` for those
 * ids returns null. A direct-jump variant is tracked as a future iteration.
 */
class SharedMetaGoToAction : AnAction() {

    /** Update on the background thread — no PSI/RD calls happen here. */
    override fun getActionUpdateThread(): ActionUpdateThread = ActionUpdateThread.BGT

    /**
     * Hide the entry outside of a C# editor context. The host popup is
     * cross-language so it would still open in JS / XAML / etc., but the
     * SharedMeta entries inside it only ever materialise on a C# symbol.
     */
    override fun update(e: AnActionEvent) {
        val file = e.getData(CommonDataKeys.PSI_FILE)
        e.presentation.isEnabledAndVisible =
            file != null && (file.name.endsWith(".cs") || file.fileType.name.equals("C#", ignoreCase = true))
    }

    override fun actionPerformed(e: AnActionEvent) {
        frontendLog("SharedMetaGoToAction.actionPerformed place=${e.place}")

        val actionManager = ActionManager.getInstance()

        // Confirmed via runtime id-dump in 0.5.3:
        //   ReSharperNavigateTo -> com.jetbrains.rider.actions.impl.RiderNavigateFromHereAction
        // is the action that opens the same "Navigate To" popup users get from
        // Ctrl+Shift+G. The legacy candidates below are intentional fallbacks
        // in case JetBrains renames it again — `GotoRelated` deliberately stays
        // off the list because it opens a different (less useful) popup.
        val candidates = listOf(
            "ReSharperNavigateTo",
            "Resharper.ContextNavigateTo",
            "ContextNavigateTo",
            "JetBrains.ReSharper.Actions.ContextNavigateTo",
        )
        for (id in candidates) {
            val target = actionManager.getAction(id) ?: continue
            frontendLog("  invoking '$id' (${target::class.qualifiedName})")
            target.actionPerformed(e)
            return
        }
        frontendLog("  NO candidate matched — popup not opened")
    }
}
