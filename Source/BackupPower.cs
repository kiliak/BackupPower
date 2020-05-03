using Verse;
using UnityEngine;
using HarmonyLib;

namespace BackupPower
{
	public class BackupPower : Mod
	{
		public static Settings Settings { get; private set; }
		public BackupPower(ModContentPack content) : base(content)
		{
			// initialize settings
			Settings = GetSettings<Settings>();

#if DEBUG
			Harmony.DEBUG = true;
#endif
			Harmony harmony = new Harmony("Fluffy.BackupPower");
			harmony.PatchAll();

		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			base.DoSettingsWindowContents(inRect);
			GetSettings<Settings>().DoWindowContents(inRect);
		}

		public override string SettingsCategory()
		{
			return "Backup Power";
		}
	}
}