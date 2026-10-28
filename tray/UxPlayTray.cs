// Tray launcher for UxPlay. Compiled with the csc.exe shipped in .NET Framework 4 (C# 5 syntax only).
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

static class UxPlayTray
{
    const string SettingsKey = @"Software\UxPlayTray";
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    static readonly int[] Delays = { -300, -200, -100, -50, 0, 50, 100, 200, 300, 500 };

    static string root = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "uxplay");
    static string uxplayExe = Path.Combine(root, @"bin\uxplay.exe");
    static string dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "UxPlayTray");
    static string logPath = Path.Combine(dataDir, "uxplay.log");

    static Process proc;
    static StreamWriter log;
    static NotifyIcon tray;
    static ToolStripMenuItem status, delayMenu, noSync, autostart;
    static Icon iconOn = MakeIcon(Color.FromArgb(0, 160, 80)), iconOff = MakeIcon(Color.Gray);

    [STAThread]
    static void Main()
    {
        bool first;
        using (new Mutex(true, "UxPlayTray-single-instance", out first))
        {
            if (!first) return;
            Application.EnableVisualStyles();
            Directory.CreateDirectory(dataDir);

            var menu = new ContextMenuStrip();
            status = new ToolStripMenuItem { Enabled = false };
            menu.Items.Add(status);
            menu.Items.Add(new ToolStripSeparator());

            delayMenu = new ToolStripMenuItem("Décalage du son");
            foreach (int d in Delays)
            {
                int ms = d;
                string label = ms == 0 ? "0 ms (par défaut)" : (ms > 0 ? "Retarder le son de " + ms + " ms" : "Avancer le son de " + (-ms) + " ms");
                var item = new ToolStripMenuItem(label) { Tag = ms };
                item.Click += delegate { SetSetting("DelayMs", ms); SetSetting("NoSync", 0); Restart(); };
                delayMenu.DropDownItems.Add(item);
            }
            delayMenu.DropDownItems.Add(new ToolStripSeparator());
            noSync = new ToolStripMenuItem("Latence minimale, sans synchro avec l'iPad (-async no)");
            noSync.Click += delegate { SetSetting("NoSync", noSync.Checked ? 0 : 1); Restart(); };
            delayMenu.DropDownItems.Add(noSync);
            menu.Items.Add(delayMenu);

            menu.Items.Add("Redémarrer UxPlay", null, delegate { Restart(); });
            autostart = new ToolStripMenuItem("Lancer au démarrage de Windows");
            autostart.Click += delegate { SetAutostart(!autostart.Checked); RefreshMenu(); };
            menu.Items.Add(autostart);
            menu.Items.Add("Ouvrir le journal", null, delegate { if (File.Exists(logPath)) Process.Start(logPath); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Quitter", null, delegate { Stop(); tray.Visible = false; Application.Exit(); });

            tray = new NotifyIcon { ContextMenuStrip = menu, Visible = true };
            menu.Opening += delegate { RefreshMenu(); };

            // Reflect a crash of uxplay.exe in the icon without needing cross-thread events.
            var timer = new System.Windows.Forms.Timer { Interval = 2000 };
            timer.Tick += delegate { RefreshMenu(); };
            timer.Start();

            Start();
            RefreshMenu();
            Application.Run();
        }
    }

    static string BuildArgs()
    {
        // Audio only: no video window; -async makes the iPad delay its picture to match the PC's sound.
        if (GetSetting("NoSync", 0) == 1) return "-vs 0 -async no";
        int delay = GetSetting("DelayMs", 0);
        return "-vs 0 -async" + (delay == 0 ? "" : " " + delay);
    }

    static void Start()
    {
        KillStrays();
        var psi = new ProcessStartInfo(uxplayExe, BuildArgs())
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = dataDir,
        };
        // Use only the bundled GStreamer, even if another GStreamer is installed on the PC.
        psi.EnvironmentVariables["PATH"] = Path.Combine(root, "bin") + ";" + Environment.GetEnvironmentVariable("PATH");
        psi.EnvironmentVariables["GST_PLUGIN_SYSTEM_PATH_1_0"] = Path.Combine(root, @"lib\gstreamer-1.0");
        psi.EnvironmentVariables["GST_PLUGIN_PATH_1_0"] = "";
        psi.EnvironmentVariables["GST_PLUGIN_SCANNER_1_0"] = Path.Combine(root, @"libexec\gstreamer-1.0\gst-plugin-scanner.exe");
        psi.EnvironmentVariables["GST_REGISTRY_1_0"] = Path.Combine(dataDir, "gst-registry.bin");

        try
        {
            log = new StreamWriter(logPath, false) { AutoFlush = true };
            log.WriteLine("> uxplay " + psi.Arguments);
            proc = Process.Start(psi);
            proc.OutputDataReceived += (s, e) => WriteLog(e.Data);
            proc.ErrorDataReceived += (s, e) => WriteLog(e.Data);
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            proc = null;
            MessageBox.Show("Impossible de lancer UxPlay :\n" + uxplayExe + "\n\n" + ex.Message, "UxPlay", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    static void WriteLog(string line)
    {
        if (line == null) return;
        lock (logPath) { try { log.WriteLine(line); } catch (ObjectDisposedException) { } }
    }

    static void Stop()
    {
        if (proc != null && !proc.HasExited) { proc.Kill(); proc.WaitForExit(3000); }
        proc = null;
        if (log != null) lock (logPath) { log.Dispose(); }
    }

    static void Restart() { Stop(); Start(); RefreshMenu(); }

    // A previous tray crash can leave our uxplay.exe running and holding its ports.
    static void KillStrays()
    {
        foreach (var p in Process.GetProcessesByName("uxplay"))
        {
            try { if (string.Equals(p.MainModule.FileName, uxplayExe, StringComparison.OrdinalIgnoreCase)) { p.Kill(); p.WaitForExit(3000); } }
            catch (Exception) { }
        }
    }

    static void RefreshMenu()
    {
        bool running = proc != null && !proc.HasExited;
        int delay = GetSetting("DelayMs", 0);
        bool off = GetSetting("NoSync", 0) == 1;
        string state = running ? "UxPlay actif" : "UxPlay arrêté (voir le journal)";
        string sync = off ? "synchro désactivée" : "décalage son " + (delay > 0 ? "+" : "") + delay + " ms";
        status.Text = state + " – " + sync;
        tray.Text = state;
        tray.Icon = running ? iconOn : iconOff;
        foreach (ToolStripItem i in delayMenu.DropDownItems)
        {
            var m = i as ToolStripMenuItem;
            if (m != null && m.Tag is int) m.Checked = !off && (int)m.Tag == delay;
        }
        noSync.Checked = off;
        using (var k = Registry.CurrentUser.OpenSubKey(RunKey)) autostart.Checked = k != null && k.GetValue("UxPlayTray") != null;
    }

    static void SetAutostart(bool on)
    {
        using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
        {
            if (on) k.SetValue("UxPlayTray", "\"" + Application.ExecutablePath + "\"");
            else k.DeleteValue("UxPlayTray", false);
        }
    }

    static int GetSetting(string name, int dflt)
    {
        using (var k = Registry.CurrentUser.OpenSubKey(SettingsKey))
        {
            object v = k == null ? null : k.GetValue(name);
            return v is int ? (int)v : dflt;
        }
    }

    static void SetSetting(string name, int value)
    {
        using (var k = Registry.CurrentUser.CreateSubKey(SettingsKey)) k.SetValue(name, value, RegistryValueKind.DWord);
    }

    static Icon MakeIcon(Color c)
    {
        using (var bmp = new Bitmap(16, 16))
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var b = new SolidBrush(c)) g.FillEllipse(b, 1, 1, 14, 14);
            g.FillPolygon(Brushes.White, new[] { new Point(6, 4), new Point(12, 8), new Point(6, 12) });
            return Icon.FromHandle(bmp.GetHicon());
        }
    }
}
