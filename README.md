# Unicorn Overlord Save Editor (Nintendo Switch)

An enhanced, robust save editor for *Unicorn Overlord* on the Nintendo Switch (WPF / .NET 9).

---

## ⚡ Build & Run
* **Visual Studio 2022**: Open `UnicornOverlord.sln` and press **F5** (or **Ctrl+F5**).
* **.NET CLI**:
  ```cmd
  dotnet build -c Release
  dotnet run --project UnicornOverlord
  ```

---

## 🎮 How to Export & Import Saves (Nintendo Switch)

### 1. Requirements
* A CFW Nintendo Switch running **Atmosphere**.
* A save manager installed on Switch: **JKSV** (recommended), **Checkpoint**, or **EdiZon**.

### 2. Dumping Save
1. Launch **JKSV** from the Homebrew menu.
2. Select **Unicorn Overlord** from your game titles list.
3. Press **A** -> **New** -> enter a folder name (e.g. `Backup1`) and confirm.
4. Mount your SD card or connect via USB (DBI MTP / FTP / SD card reader).
5. Locate your save on the SD card at:
   `sdmc:/JKSV/Unicorn_Overlord/Backup1/`
6. Look for `UCSAVEFILE01.DAT` (Slot 1), `UCSAVEFILE02.DAT` (Slot 2), etc. Copy it to your PC.

### 3. Editing with Unicorn Overlord Save Editor
1. Launch the editor.
2. Click **Open Save...** (`Ctrl+O`) and choose your `UCSAVEFILExx.DAT`.
3. *Note: An automatic timestamped backup is generated in the `backup/` directory every time you open or save.*
4. Edit your desired currencies, characters, bonds, items, equipment, and units.
5. Click **Save** (`Ctrl+S`).

### 4. Restoring Save to Switch
1. Copy the edited `UCSAVEFILExx.DAT` back into `sdmc:/JKSV/Unicorn_Overlord/Backup1/` on your SD card.
2. Open **JKSV** on your Switch.
3. Select **Unicorn Overlord**, highlight `Backup1`, and hold **Y** to restore.
4. Launch *Unicorn Overlord* and enjoy!

---

## ✨ Features & Capabilities

### 🪙 Currencies & Progression
* **Gold**: Up to `9,999,999` (with one-click `Max Gold`).
* **Renown & Renown Rank**: Edit Fame points and Rank (Rank E to S).
* **Medals**: Used for promotions and unit size expansions.
* **Coliseum Coins**: Redeemable for top-tier weapons and accessories.
* **Divine Shards**: Redeemable at Ochlys's angel shop.
* **Corne Ash**: Hourglass battle rewinds.
* **True Zenoiran Difficulty**: Instant unlock flag for the permadeath True Zenoiran mode.

### ⚔️ Characters & Progression
* **Instant Level 50**: Max level single character or one-click `All to Lv. 50`.
* **Growth Types 0–16**:
  * All standard types (All-Rounder, Hardy, Offensive, Defensive, Go-Getter, etc.).
  * **Type 10 - All-Rounder+ (Super Growth)**: Hidden stat growth granting +100 to all stats across the board at Lv. 50.
* **Stat Dews**: Edit bonuses for HP, P-Atk, P-Def, M-Atk, M-Def, Initiative, Accuracy, Critical, Guard, Evasion (supports legit max of 5 or custom boosts).
* **Hired Mercenary Names**: Proper name resolution for custom mercenaries.
* **Character Cloning & Army Expansion**:
  * Clone any character or mercenary with one click to expand your army up to the full 500-slot save capacity (bypassing the in-game 64-mercenary Fort cap).
* **Appearance Colors**: Customize character color palettes.
* **Rapport Bonds**:
  * Real partner name resolution for all army relationships.
  * Maximize individual bond (1,000 points & 3 hearts) or one-click `Max All Relationships`.

### 🎒 Inventory & Items
* **Item Count**: Adjust counts up to 999.
* **Add Item**: Add any item with category filters (Consumables, Valuables, Materials).
* **Delete Item**: Safe deletion with memory compaction to prevent gaps or corrupted save data.
* **Batch Max**: Set all consumable items to 99 in one click.

### 🛡️ Equipment (Weapons, Shields & Accessories)
* Filter by **All**, **Weapons**, **Shields**, or **Accessories**.
* Inspect equipped status, current owner, and slot index (`Weapon 1`, `Shield 1`, `Accessory 1..3`).
* **Unequip**: Safely return any equipped item to the bag inventory.
* **Batch Add Missing**:
  * `Add All Weapons`: Adds every weapon in the database to your bag.
  * `Add All Shields`: Adds every shield in the database to your bag.
* **Upgrade All Equipment**: Forges/upgrades all existing equipment (every weapon, shield, and accessory in inventory and equipped, including duplicates) to maximum tier (★).

### 🚩 Squad Units
* Inspect all 10 squads.
* Unlock all 10 squads and set member capacity to 5.

---

## 🛡️ Reliability & Safety
* **Atomic File Saving**: Edits are written to a temporary file first before swapping to ensure your save file is never corrupted if interrupted.
* **Automatic Backups**: Safe timestamped backups are automatically placed in `backup/`.
* **59 Automated Tests**: Validated against real Nintendo Switch save data with 100% pass rate.

---

## Special Thanks
* [turtle-insect](https://github.com/turtle-insect) (Original project author)
* [pauljames80](https://gbatemp.net/members/pj1980.378437/)
* [GBAtemp](https://gbatemp.net/threads/unicorn-overlord-save-editing.650584/)
* [DataSheet](https://docs.google.com/spreadsheets/d/1UXe4nEloKlv14P4H4cOKeJc8R2P1fZW_HaLAuQG96BQ)
