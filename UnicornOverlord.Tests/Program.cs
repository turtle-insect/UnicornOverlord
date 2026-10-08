using System;
using System.IO;
using System.Linq;
using UnicornOverlord;

namespace UnicornOverlord.Tests
{
	internal class Program
	{
		private static int mTestsPassed = 0;
		private static int mTestsFailed = 0;

		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("=================================================");
			Console.WriteLine(" Unicorn Overlord Save Editor - Automated Test Suite");
			Console.WriteLine("=================================================");

			try
			{
				TestDatabaseLoading();
				TestGrowthTypes();
				TestItemUpgradedBinaryFormat();
				TestRealSaveFileOperations();
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"\n[UNHANDLED EXCEPTION] {ex}");
				Console.ResetColor();
				mTestsFailed++;
			}

			Console.WriteLine("\n=================================================");
			Console.ForegroundColor = mTestsFailed == 0 ? ConsoleColor.Green : ConsoleColor.Red;
			Console.WriteLine($" Test Results: {mTestsPassed} Passed, {mTestsFailed} Failed");
			Console.ResetColor();
			Console.WriteLine("=================================================");

			Environment.Exit(mTestsFailed == 0 ? 0 : 1);
		}

		private static void Assert(bool condition, string testName, string details = "")
		{
			if (condition)
			{
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine($" [PASS] {testName} {details}");
				Console.ResetColor();
				mTestsPassed++;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($" [FAIL] {testName} {details}");
				Console.ResetColor();
				mTestsFailed++;
			}
		}

		private static void TestDatabaseLoading()
		{
			Console.WriteLine("\n--- Testing Database Loading (Info.cs) ---");
			var info = Info.Instance();

			Assert(info.Item.Count >= 800, "Item database count >= 800", $"Actual: {info.Item.Count}");
			Assert(info.Class.Count >= 70, "Class database count >= 70", $"Actual: {info.Class.Count}");
			Assert(info.Name.Count >= 700, "Name database count >= 700", $"Actual: {info.Name.Count}");
			Assert(info.Kind.Count >= 500, "Kind database count >= 500", $"Actual: {info.Kind.Count}");

			Assert(info.GetClassName(1) == "Lord", "Class ID 1 is Lord", $"Actual: '{info.GetClassName(1)}'");
			Assert(info.GetClassName(2) == "High Lord", "Class ID 2 is High Lord", $"Actual: '{info.GetClassName(2)}'");

			Assert(info.GetItemName(3).Contains("Medals", StringComparison.OrdinalIgnoreCase) ||
				   info.GetItemName(3).Contains("勲章") ||
				   info.GetItemName(3).Contains("勋章"),
				   "Item ID 3 is Medals/Honors", $"Actual: '{info.GetItemName(3)}'");

			Assert(info.GetItemCategory(282) == "Weapons", "Item ID 282 categorized as Weapons", $"Actual: '{info.GetItemCategory(282)}'");
			Assert(info.GetItemCategory(642) == "Shields", "Item ID 642 categorized as Shields", $"Actual: '{info.GetItemCategory(642)}'");
			Assert(info.GetItemCategory(784) == "Accessories", "Item ID 784 categorized as Accessories", $"Actual: '{info.GetItemCategory(784)}'");
			Assert(info.GetItemCategory(8) == "Consumables", "Item ID 8 categorized as Consumables", $"Actual: '{info.GetItemCategory(8)}'");
			Assert(info.GetItemCategory(3) == "Currency / Tokens", "Item ID 3 categorized as Currency / Tokens", $"Actual: '{info.GetItemCategory(3)}'");
		}

		private static void TestGrowthTypes()
		{
			Console.WriteLine("\n--- Testing Growth Types ---");
			Assert(GrowthTypeInfo.All.Count >= 17, "Growth types list has >= 17 entries", $"Actual: {GrowthTypeInfo.All.Count}");

			var super = GrowthTypeInfo.Get(10);
			Assert(super.ID == 10 && super.Name.Contains("Super"), "Growth Type 10 is Super Growth", $"Actual: '{super.DisplayText}'");

			var hardy = GrowthTypeInfo.Get(1);
			Assert(hardy.ID == 1 && hardy.Name == "Hardy", "Growth Type 1 is Hardy", $"Actual: '{hardy.DisplayText}'");
		}

		private static void TestItemUpgradedBinaryFormat()
		{
			Console.WriteLine("\n--- Testing Item Upgraded Binary Format (+16 high nibble & +17 zero) ---");
			string mockPath = Path.Combine(AppContext.BaseDirectory, "MOCK_SAVE.DAT");
			try
			{
				byte[] buffer = new byte[65536];
				System.Text.Encoding.ASCII.GetBytes("UCSD").CopyTo(buffer, 4);
				File.WriteAllBytes(mockPath, buffer);

				bool opened = SaveData.Instance().Open(mockPath);
				Assert(opened, "Mock SaveData Open returned true");

				uint itemAddr = Util.ItemBaseAddress; // 0xA0
				var item = new Item(itemAddr);
				item.ID = 282; // Bronze Sword (Type 5: Weapons)
				item.Status = 5; // 0x05 (un-upgraded weapon)

				Assert(!item.Upgraded, "Initial weapon is not upgraded", $"Byte 16: 0x{SaveData.Instance().ReadNumber(itemAddr + 16, 1):X2}");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 16, 1) == 0x05, "Byte 16 is 0x05 for un-upgraded weapon");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 17, 1) == 0x00, "Byte 17 is 0x00 (padding)");

				// Test Upgrading: should set byte 16 to 0x15 and keep/set byte 17 as 0x00
				item.Upgraded = true;
				Assert(item.Upgraded, "Item reports upgraded is true");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 16, 1) == 0x15, "Byte 16 is 0x15 (high nibble 1, low nibble 5)", $"Actual: 0x{SaveData.Instance().ReadNumber(itemAddr + 16, 1):X2}");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 17, 1) == 0x00, "Byte 17 is 0x00", $"Actual: 0x{SaveData.Instance().ReadNumber(itemAddr + 17, 1):X2}");
				Assert(item.Status == 0x15, "32-bit Status is 0x15 (21)", $"Actual: {item.Status}");

				// Test reverting upgrade
				item.Upgraded = false;
				Assert(!item.Upgraded, "Item reports upgraded is false after reset");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 16, 1) == 0x05, "Byte 16 is reset to 0x05", $"Actual: 0x{SaveData.Instance().ReadNumber(itemAddr + 16, 1):X2}");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 17, 1) == 0x00, "Byte 17 remains 0x00", $"Actual: 0x{SaveData.Instance().ReadNumber(itemAddr + 17, 1):X2}");

				// Test resilience against corrupted save (where old bug wrote 1 to byte 17 and left byte 16 as 0x05)
				SaveData.Instance().WriteNumber(itemAddr + 16, 1, 0x05);
				SaveData.Instance().WriteNumber(itemAddr + 17, 1, 0x01);
				Assert(!item.Upgraded, "Item with bugged byte 17=0x01 correctly reports false because byte 16 is 0x05");

				// Upgrading repairs both byte 16 to 0x15 and byte 17 back to 0x00
				item.Upgraded = true;
				Assert(item.Upgraded, "Item upgraded is true after repair");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 16, 1) == 0x15, "Repaired byte 16 is 0x15");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 17, 1) == 0x00, "Repaired byte 17 is cleaned back to 0x00");

				// Verify persistence on disk
				SaveData.Instance().Save();
				SaveData.Instance().Open(mockPath);
				var reloadedItem = new Item(itemAddr);
				Assert(reloadedItem.Upgraded, "Reloaded item from disk retains upgraded status");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 16, 1) == 0x15, "Reloaded disk byte 16 is 0x15");
				Assert(SaveData.Instance().ReadNumber(itemAddr + 17, 1) == 0x00, "Reloaded disk byte 17 is 0x00");
			}
			finally
			{
				try { if (File.Exists(mockPath)) File.Delete(mockPath); } catch { }
			}
		}

		private static void TestRealSaveFileOperations()
		{
			Console.WriteLine("\n--- Testing Real Save File Operations ---");
			string sourceSave = @"C:\Users\Hunterkasa\Downloads\0.13\backup\2026-09-29 15-59-04 UCSAVEFILE02.DAT";
			if (!File.Exists(sourceSave))
			{
				Console.WriteLine($"[WARN] Sample save file '{sourceSave}' not found, skipping real save tests.");
				return;
			}

			// Copy to temporary file for isolated safe testing
			string tempSave = Path.Combine(AppContext.BaseDirectory, "TEST_UCSAVEFILE02.DAT");
			File.Copy(sourceSave, tempSave, overwrite: true);

			var vm = new ViewModel();
			vm.SuppressDialogs = true;
			bool opened = SaveData.Instance().Open(tempSave);
			Assert(opened, "SaveData Open returned true for UCSAVEFILE02.DAT");
			Assert(SaveData.Instance().IsLoaded, "SaveData IsLoaded is true");

			vm.Initialize();

			// 1. Verify Basic properties
			Assert(vm.Basic.Money == 4818731, "Original Gold is 4,818,731", $"Actual: {vm.Basic.Money}");
			Assert(vm.Basic.Fame == 5000, "Original Renown is 5,000", $"Actual: {vm.Basic.Fame}");
			Assert(vm.Basic.SaveSlot == 2, "Save slot is 2", $"Actual: {vm.Basic.SaveSlot}");
			Assert(vm.Basic.PlayTimeSeconds == 7693, "Play time is 7693 seconds", $"Actual: {vm.Basic.PlayTimeFormatted}");
			Assert(vm.Basic.Medals == 7504, "Original Medals count is 7,504", $"Actual: {vm.Basic.Medals}");
			Assert(vm.Basic.DivineShards == 7, "Original Divine Shards count is 7", $"Actual: {vm.Basic.DivineShards}");

			// 2. Verify Characters
			Assert(vm.Characters.Count == 30, "Character count is 30", $"Actual: {vm.Characters.Count}");
			var alain = vm.Characters[0];
			Assert(alain.DisplayName == "Alain", "Slot 0 character is Alain", $"Actual: '{alain.DisplayName}'");
			Assert(alain.ClassName == "High Lord", "Alain's class is High Lord", $"Actual: '{alain.ClassName}'");
			Assert(alain.Lv == 5, "Alain's level is 5", $"Actual: {alain.Lv}");

			var scarlett = vm.Characters[1];
			Assert(scarlett.DisplayName == "Scarlett", "Slot 1 character is Scarlett", $"Actual: '{scarlett.DisplayName}'");

			var merc = vm.Characters[9];
			Assert(merc.IsMercenary, "Slot 9 character is identified as Mercenary");
			Assert(merc.DisplayName.Contains("Liberation Knight"), "Slot 9 mercenary name resolved as 'Liberation Knight'", $"Actual: '{merc.DisplayName}'");

			// 3. Verify Bonds
			Assert(alain.Bonds != null && alain.Bonds.Count > 0, "Alain has bond relationships", $"Count: {alain.Bonds?.Count}");
			var chloeBond = alain.Bonds?.FirstOrDefault(b => b.ID == 9);
			Assert(chloeBond != null, "Found Alain's bond with Chloe (ID 9)");
			if (chloeBond != null)
			{
				Assert(chloeBond.PartnerName == "Chloe", "Chloe's bond has partner name 'Chloe'", $"Actual: '{chloeBond.PartnerName}'");
				Assert(chloeBond.Value == 900, "Chloe's bond points is 900 (not overflowed 66436)", $"Actual: {chloeBond.Value}");
				Assert(chloeBond.HeartLevel == 1, "Chloe's bond heart level is 1", $"Actual: {chloeBond.HeartLevel}");
			}

			// 4. Verify Items & Equipment
			Assert(vm.Items.Count == 39, "Item count is 39", $"Actual: {vm.Items.Count}");
			Assert(vm.Equipments.Count == 174, "Equipment count is 174", $"Actual: {vm.Equipments.Count}");

			var eq0 = vm.Equipments[0];
			Assert(eq0.IsEquipped, "Equipment 0 is equipped");
			Assert(eq0.EquippedCharacterId == 5, "Equipment 0 is equipped by Alain (ID 5)", $"Actual: {eq0.EquippedCharacterId}");
			Assert(eq0.EquipSlotName == "Accessory 2", "Equipment 0 slot is Accessory 2", $"Actual: '{eq0.EquipSlotName}'");

			// 5. Test Modifications
			Console.WriteLine("\n--- Testing Save Modifications ---");
			vm.Basic.Money = 9999999;
			vm.Basic.Medals = 8888;
			alain.SetMaxLevel();
			alain.SetMaxDews(5);
			alain.GrowthType1 = 10; // All-Rounder+
			alain.GrowthType2 = 10; // All-Rounder+
			if (chloeBond != null) chloeBond.Maximize();

			bool saved = SaveData.Instance().Save();
			Assert(saved, "SaveData Save returned true");

			// Reload and verify
			bool reloaded = SaveData.Instance().Open(tempSave);
			Assert(reloaded, "SaveData Reload returned true");
			vm.Initialize();

			Assert(vm.Basic.Money == 9999999, "Reloaded Gold is 9,999,999", $"Actual: {vm.Basic.Money}");
			Assert(vm.Basic.Medals == 8888, "Reloaded Medals is 8,888", $"Actual: {vm.Basic.Medals}");
			var reloadedAlain = vm.Characters[0];
			Assert(reloadedAlain.Lv == 50, "Reloaded Alain Level is 50", $"Actual: {reloadedAlain.Lv}");
			Assert(reloadedAlain.HPPlus == 5, "Reloaded Alain HPPlus dew is 5", $"Actual: {reloadedAlain.HPPlus}");
			Assert(reloadedAlain.GrowthType1 == 10, "Reloaded Alain Growth Type 1 is 10 (Super)", $"Actual: {reloadedAlain.GrowthType1}");
			Assert(reloadedAlain.GrowthType2 == 10, "Reloaded Alain Growth Type 2 is 10 (Super)", $"Actual: {reloadedAlain.GrowthType2}");

			var reloadedChloeBond = reloadedAlain.Bonds?.FirstOrDefault(b => b.ID == 9);
			Assert(reloadedChloeBond?.Value == 1000, "Reloaded Chloe bond is 1000", $"Actual: {reloadedChloeBond?.Value}");
			Assert(reloadedChloeBond?.HeartLevel == 3, "Reloaded Chloe bond hearts is 3", $"Actual: {reloadedChloeBond?.HeartLevel}");

			// Test Character Cloning (expanding army up to 500 capacity)
			Console.WriteLine("\n--- Testing Character Cloning ---");
			int preCloneCount = vm.Characters.Count;
			vm.SelectedCharacter = alain;
			vm.CloneCharacterCommand.Execute(alain);
			Assert(vm.Characters.Count == preCloneCount + 1, "Character count increased by 1 after clone", $"Actual: {vm.Characters.Count}");
			var clonedCh = vm.Characters[preCloneCount];
			Assert(clonedCh.DisplayName == alain.DisplayName, "Cloned character shares name with source", $"Actual: '{clonedCh.DisplayName}'");
			Assert(clonedCh.SlotIndex == (uint)preCloneCount, "Cloned character has correct slot index", $"Actual: {clonedCh.SlotIndex}");

			// 6. Test Item Deletion & Array Compaction
			Console.WriteLine("\n--- Testing Item Deletion & Array Compaction ---");
			int origItemCount = vm.Items.Count;
			var itemToDelete = vm.Items[5];
			uint deletedId = itemToDelete.ID;
			uint deletedAddr = itemToDelete.MemoryAddress;

			vm.DeleteItemCommand.Execute(itemToDelete);
			Assert(vm.Items.Count == origItemCount - 1, "Item count decreased by 1 after delete", $"Actual: {vm.Items.Count}");

			// Save and reload to verify disk buffer integrity
			SaveData.Instance().Save();
			SaveData.Instance().Open(tempSave);
			vm.Initialize();
			// 7. Test Unit Operations
			Console.WriteLine("\n--- Testing Unit Operations ---");
			vm.UnlockAllUnitsCommand.Execute(null);
			Assert(vm.Units.Count == 10, "10 units loaded", $"Actual: {vm.Units.Count}");
			Assert(vm.Units.All(u => u.Capacity == 5 && u.Valid), "All 10 units unlocked and set to 5 members");

			// 8. Test Batch Equipment Add
			Console.WriteLine("\n--- Testing Batch Operations ---");
			int preWeaponsEqCount = vm.Equipments.Count;
			vm.AddAllWeaponsCommand.Execute(null);
			Assert(vm.Equipments.Count > preWeaponsEqCount, "Equipments count increased after AddAllWeapons", $"Before: {preWeaponsEqCount}, After: {vm.Equipments.Count}");

			// Persist and verify reload
			SaveData.Instance().Save();
			SaveData.Instance().Open(tempSave);
			vm.Initialize();
			Assert(vm.Units.All(u => u.Capacity == 5 && u.Valid), "Reloaded units retain 5 members and valid status");
			Assert(vm.Equipments.Count > preWeaponsEqCount, "Reloaded equipment retains batch added items");

			// 9. Test Upgrade All Equipment
			Console.WriteLine("\n--- Testing Upgrade All Equipment ---");
			vm.UpgradeAllEquipmentCommand.Execute(null);
			Assert(vm.Equipments.Count > 0, "Equipment exists in inventory", $"Count: {vm.Equipments.Count}");
			Assert(vm.Equipments.All(e => e.Upgraded), "All equipment items in inventory and equipped are upgraded (★)");

			// Persist and verify reload
			SaveData.Instance().Save();
			SaveData.Instance().Open(tempSave);
			vm.Initialize();
			Assert(vm.Equipments.All(e => e.Upgraded), "All reloaded equipment items retain upgraded status (★)");

			// Cleanup
			try { File.Delete(tempSave); } catch { }
		}
	}
}
