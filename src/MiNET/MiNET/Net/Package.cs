using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Craft.Net.Common;
using MiNET.Utils;

namespace MiNET.Net
{
	public class ObjectPool<T>
	{
		private ConcurrentBag<T> _objects;
		private Func<T> _objectGenerator;

		public ObjectPool(Func<T> objectGenerator)
		{
			if (objectGenerator == null) throw new ArgumentNullException("objectGenerator");
			_objects = new ConcurrentBag<T>();
			_objectGenerator = objectGenerator;
		}

		public T GetObject()
		{
			T item;
			if (_objects.TryTake(out item)) return item;
			return _objectGenerator();
		}

		public void PutObject(T item)
		{
			_objects.Add(item);
		}
	}

	/// Base package class
	public partial class Package : ICloneable
	{
		public  ObjectPool<McpeMovePlayer> MovePool = null;

		public byte Id;

		protected MemoryStream _buffer;
		private BinaryWriter _writer;
		private BinaryReader _reader;
		private Stopwatch _timer = new Stopwatch();

		public Package()
		{
			_buffer = new MemoryStream();
			_reader = new BinaryReader(_buffer);
			_writer = new BinaryWriter(_buffer);
			Timer.Start();
		}

		public Stopwatch Timer
		{
			get { return _timer; }
		}

		public void Write(byte value)
		{
			_writer.Write(value);
		}

		public byte ReadByte()
		{
			return _reader.ReadByte();
		}

		public sbyte ReadSByte()
		{
			return _reader.ReadSByte();
		}

		public void Write(sbyte value)
		{
			_writer.Write(value);
		}

		public void Write(byte[] value)
		{
			if (value == null) return;

			_writer.Write(value);
		}

		public byte[] ReadBytes(int count)
		{
			var readBytes = _reader.ReadBytes(count);
			if (readBytes.Length != count) throw new ArgumentOutOfRangeException();
			return readBytes;
		}

		public void Write(short value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		public short ReadShort()
		{
			return IPAddress.NetworkToHostOrder(_reader.ReadInt16());
		}

		public void Write(ushort value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		public ushort ReadUShort()
		{
			return (ushort) IPAddress.NetworkToHostOrder(_reader.ReadInt16());
		}

		public void Write(Int24 value)
		{
			_writer.Write(value.GetBytes());
		}

		public Int24 ReadLittle()
		{
			return new Int24(_reader.ReadBytes(3));
		}

		public void Write(int value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		public int ReadInt()
		{
			return IPAddress.NetworkToHostOrder(_reader.ReadInt32());
		}

		public void Write(uint value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		public uint ReadUInt()
		{
			return (uint) IPAddress.NetworkToHostOrder(_reader.ReadUInt32());
		}

		public void Write(long value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		private byte[] Reverse(byte[] bytes)
		{
			Array.Reverse(bytes);
			return bytes;
		}

		public long ReadLong()
		{
			return IPAddress.NetworkToHostOrder(_reader.ReadInt64());
		}

		public void Write(ulong value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		public ulong ReadULong()
		{
			return (ulong) IPAddress.NetworkToHostOrder(_reader.ReadInt64());
		}

		public void Write(float value)
		{
			_writer.Write(Reverse(BitConverter.GetBytes(value)));
		}

		public float ReadFloat()
		{
			return BitConverter.ToSingle(BitConverter.GetBytes(_reader.ReadSingle()).Reverse().ToArray(), 0);
		}

		public void Write(string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);

			Write((short) bytes.Length);
			Write(bytes);
		}

		public string ReadString()
		{
			short len = ReadShort();
			return Encoding.UTF8.GetString(ReadBytes(len));
		}

		public MetadataInts ReadMetadataInts()
		{
			MetadataInts metadata = new MetadataInts();
			short count = ReadShort();

			for (byte i = 0; i < count; i++)
			{
				metadata[i] = new MetadataInt(ReadInt());
			}

			return metadata;
		}

		public MetadataSlots ReadMetadataSlots()
		{
			short count = ReadShort();

			MetadataSlots metadata = new MetadataSlots();

			for (byte i = 0; i < count; i++)
			{
				MetadataSlot slot = new MetadataSlot(new ItemStack(ReadShort(), ReadSByte(), ReadShort()));
			}

			return metadata;
		}

		public void Write(MetadataDictionary metadata)
		{
			if (metadata == null)
			{
				Write((short) 0);
				return;
			}

			Write((short) metadata.Count);

			for (byte i = 0; i < metadata.Count; i++)
			{
				{
					MetadataSlot slot = metadata[i] as MetadataSlot;
					if (slot != null)
					{
						Write(slot.Value.Id);
						Write(slot.Value.Count);
						Write(slot.Value.Metadata);

						continue;
					}
				}

				{
					MetadataInt slot = metadata[i] as MetadataInt;
					if (slot != null)
					{
						Write(slot.Value);
						continue;
					}
				}
			}
		}


		protected virtual void EncodePackage()
		{
			_buffer.Position = 0;
			Write(Id);
		}

		private object _bufferSync = new object();

		private bool _isEncoded = false;
		private byte[] _encodedMessage;

		public void Reset()
		{
			_isEncoded = false;
			_encodedMessage = null;
		}

		public virtual byte[] Encode()
		{
			if (_isEncoded) return _encodedMessage;

			lock (_bufferSync)
			{
				EncodePackage();
				_writer.Flush();
				_buffer.Position = 0;
				_encodedMessage = _buffer.ToArray();
			}
			_isEncoded = true;
			return _encodedMessage;
		}

		protected virtual void DecodePackage()
		{
			_buffer.Position = 0;
			Id = ReadByte();
		}

		public virtual void Decode(byte[] buffer)
		{
			_buffer.Position = 0;
			_buffer.SetLength(buffer.Length);
			_buffer.Write(buffer, 0, buffer.Length);
			_buffer.Position = 0;
			DecodePackage();
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public virtual T Clone<T>() where T : Package
		{
			return (T) Clone();
		}
	}
}