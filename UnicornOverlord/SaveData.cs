using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnicornOverlord
{
	internal class SaveData
	{
		private static readonly SaveData mThis = new SaveData();
		private string mFileName = string.Empty;
		private byte[]? mBuffer = null;
		private readonly Encoding mEncode = Encoding.UTF8;

		public uint Adventure { get; set; } = 0;
		public bool IsDirty { get; set; } = false;
		public bool IsLoaded => mBuffer != null && mBuffer.Length > 0;
		public string FileName => mFileName;
		public int FileSize => mBuffer?.Length ?? 0;

		private SaveData()
		{ }

		public static SaveData Instance()
		{
			return mThis;
		}

		public bool Open(string filename)
		{
			if (!File.Exists(filename)) return false;

			byte[] buffer;
			try
			{
				buffer = File.ReadAllBytes(filename);
			}
			catch
			{
				return false;
			}

			if (buffer.Length < 16) return false;

			string header = mEncode.GetString(buffer, 4, 4);
			if (header != "UCSD") return false;

			mBuffer = buffer;
			mFileName = filename;
			IsDirty = false;

			try
			{
				Backup();
			}
			catch
			{
				// Backup failure should not block opening the file
			}

			return true;
		}

		public bool Save()
		{
			if (string.IsNullOrEmpty(mFileName) || mBuffer == null) return false;

			string tempFile = mFileName + ".tmp";
			try
			{
				File.WriteAllBytes(tempFile, mBuffer);
				File.Move(tempFile, mFileName, overwrite: true);
				IsDirty = false;
				return true;
			}
			catch
			{
				if (File.Exists(tempFile))
				{
					try { File.Delete(tempFile); } catch { }
				}
				return false;
			}
		}

		public bool SaveAs(string filename)
		{
			if (mBuffer == null) return false;
			mFileName = filename;
			return Save();
		}

		public void Import(string filename)
		{
			if (string.IsNullOrEmpty(mFileName)) return;
			if (!File.Exists(filename)) return;

			try
			{
				mBuffer = File.ReadAllBytes(filename);
				IsDirty = true;
			}
			catch { }
		}

		public void Export(string filename)
		{
			if (mBuffer == null) return;
			try
			{
				File.WriteAllBytes(filename, mBuffer);
			}
			catch { }
		}

		public uint ReadNumber(uint address, uint size)
		{
			if (mBuffer == null || size == 0 || size > 4) return 0;
			address = CalcAddress(address);
			if (address + size > mBuffer.Length) return 0;

			uint result = 0;
			for (int i = 0; i < size; i++)
			{
				result += (uint)mBuffer[address + i] << (i * 8);
			}
			return result;
		}

		public byte[] ReadValue(uint address, uint size)
		{
			byte[] result = new byte[size];
			if (mBuffer == null) return result;
			address = CalcAddress(address);
			if (address + size > mBuffer.Length) return result;
			Array.Copy(mBuffer, address, result, 0, size);
			return result;
		}

		// 0 to 7.
		public bool ReadBit(uint address, uint bit)
		{
			if (bit > 7 || mBuffer == null) return false;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return false;
			byte mask = (byte)(1 << (int)bit);
			return (mBuffer[address] & mask) != 0;
		}

		public string ReadText(uint address, uint size)
		{
			if (mBuffer == null) return "";
			address = CalcAddress(address);
			if (address + size > mBuffer.Length) return "";

			byte[] tmp = new byte[size];
			for (uint i = 0; i < size; i++)
			{
				if (mBuffer[address + i] == 0) break;
				tmp[i] = mBuffer[address + i];
			}
			return mEncode.GetString(tmp).Trim('\0');
		}

		public void WriteNumber(uint address, uint size, uint value)
		{
			if (mBuffer == null || size == 0 || size > 4) return;
			address = CalcAddress(address);
			if (address + size > mBuffer.Length) return;

			bool changed = false;
			for (uint i = 0; i < size; i++)
			{
				byte b = (byte)(value & 0xFF);
				if (mBuffer[address + i] != b)
				{
					mBuffer[address + i] = b;
					changed = true;
				}
				value >>= 8;
			}
			if (changed) IsDirty = true;
		}

		// 0 to 7.
		public void WriteBit(uint address, uint bit, bool value)
		{
			if (bit > 7 || mBuffer == null) return;
			address = CalcAddress(address);
			if (address >= mBuffer.Length) return;
			byte mask = (byte)(1 << (int)bit);
			byte oldVal = mBuffer[address];
			if (value) mBuffer[address] = (byte)(mBuffer[address] | mask);
			else mBuffer[address] = (byte)(mBuffer[address] & ~mask);
			if (mBuffer[address] != oldVal) IsDirty = true;
		}

		public void WriteText(uint address, uint size, string value)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size > mBuffer.Length) return;
			byte[] tmp = mEncode.GetBytes(value);
			Array.Resize(ref tmp, (int)size);
			Array.Copy(tmp, 0, mBuffer, address, size);
			IsDirty = true;
		}

		public void WriteValue(uint address, byte[] buffer)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + buffer.Length > mBuffer.Length) return;
			Array.Copy(buffer, 0, mBuffer, address, buffer.Length);
			IsDirty = true;
		}

		public void Fill(uint address, uint size, byte number)
		{
			if (mBuffer == null) return;
			address = CalcAddress(address);
			if (address + size > mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[address + i] = number;
			}
			IsDirty = true;
		}

		public void Copy(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size > mBuffer.Length) return;
			if (to + size > mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				mBuffer[to + i] = mBuffer[from + i];
			}
			IsDirty = true;
		}

		public void Swap(uint from, uint to, uint size)
		{
			if (mBuffer == null) return;
			from = CalcAddress(from);
			to = CalcAddress(to);
			if (from + size > mBuffer.Length) return;
			if (to + size > mBuffer.Length) return;
			for (uint i = 0; i < size; i++)
			{
				byte tmp = mBuffer[to + i];
				mBuffer[to + i] = mBuffer[from + i];
				mBuffer[from + i] = tmp;
			}
			IsDirty = true;
		}

		public List<uint> FindAddress(string name, uint index)
		{
			List<uint> result = new List<uint>();
			if (mBuffer == null) return result;
			for (; index < mBuffer.Length; index++)
			{
				if (mBuffer[index] != name[0]) continue;

				int len = 1;
				for (; len < name.Length; len++)
				{
					if (index + len >= mBuffer.Length) break;
					if (mBuffer[index + len] != name[len]) break;
				}
				if (len >= name.Length) result.Add(index);
				index += (uint)len;
			}
			return result;
		}

		private uint CalcAddress(uint address)
		{
			return address + Adventure;
		}

		private void Backup()
		{
			if (string.IsNullOrEmpty(mFileName) || !File.Exists(mFileName)) return;

			DateTime now = DateTime.Now;
			string? saveDir = Path.GetDirectoryName(mFileName);
			if (string.IsNullOrEmpty(saveDir))
			{
				saveDir = AppContext.BaseDirectory;
			}

			string path = Path.Combine(saveDir, "backup");
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			string backupFile = Path.Combine(path, $"{now:yyyy-MM-dd HH-mm-ss} {Path.GetFileName(mFileName)}");
			File.Copy(mFileName, backupFile, true);
		}
	}
}
