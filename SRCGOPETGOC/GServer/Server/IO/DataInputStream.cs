using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gopet.IO
{
    public class DataInputStream
    {
        public DataInputStream()
        {

        }

        public DataInputStream(sbyte[] data)
        {
            this.buffer = data;
        }

        public sbyte readSByte()
        {
            if (this.posRead < this.buffer.Length)
            {
                return this.buffer[this.posRead++];
            }
            this.posRead = this.buffer.Length;
            return 0;
        }

        public sbyte readsbyte()
        {
            return this.readSByte();
        }

        public sbyte readByte()
        {
            return this.readSByte();
        }

        public void mark(int readlimit)
        {
            this.posMark = this.posRead;
        }

        public void reset()
        {
            this.posRead = this.posMark;
        }

        public byte readUnsignedByte()
        {
            return convertSbyteToByte(this.readSByte());
        }

        public short readShort()
        {
            short num = 0;
            for (int i = 0; i < 2; i++)
            {
                num = (short)(num << 8);
                num |= (short)(255 & (int)this.buffer[this.posRead++]);
            }
            return num;
        }

        public ushort readUnsignedShort()
        {
            ushort num = 0;
            for (int i = 0; i < 2; i++)
            {
                num = (ushort)(num << 8);
                num |= (ushort)(255 & (int)this.buffer[this.posRead++]);
            }
            return num;
        }

        public int readInt()
        {
            int num = 0;
            for (int i = 0; i < 4; i++)
            {
                num <<= 8;
                num |= (255 & (int)this.buffer[this.posRead++]);
            }
            return num;
        }

        /// <summary>
        /// Đọc độ dài mảng do client gửi, kèm chặn trên và chặn dưới.
        ///
        /// <para>Luôn dùng hàm này thay cho <c>new T[readInt()]</c>. Client là
        /// bên không đáng tin: một <c>int</c> cỡ 2 tỷ làm máy chủ ném
        /// <see cref="OutOfMemoryException"/>, và với dữ liệu được phát lại cho
        /// cả khu vực (như đường đi trong <c>sendMove</c>) thì còn thành đòn
        /// khuếch đại — một gói vào, N gói ra.</para>
        /// </summary>
        /// <param name="min">Số phần tử tối thiểu hợp lệ.</param>
        /// <param name="max">Số phần tử tối đa hợp lệ.</param>
        /// <param name="context">Tên trường, dùng cho thông báo lỗi.</param>
        /// <exception cref="IOException">Độ dài nằm ngoài khoảng cho phép.</exception>
        public int readArrayLength(int min, int max, string context)
        {
            int length = readInt();
            if (length < min || length > max)
            {
                throw new IOException(
                    $"Độ dài mảng không hợp lệ cho '{context}': {length} (cho phép {min}..{max})");
            }

            return length;
        }

        public long readlong()
        {
            long num = 0L;
            for (int i = 0; i < 8; i++)
            {
                num <<= 8;
                num |= (long)(255 & (int)this.buffer[this.posRead++]);
            }
            return num;
        }

        public bool readBool()
        {
            return (int)this.readSByte() > 0;
        }

        public bool readBoolean()
        {
            return (int)this.readSByte() > 0;
        }

        public string readString()
        {
            short num = this.readShort();
            byte[] array = new byte[(int)num];
            for (int i = 0; i < (int)num; i++)
            {
                array[i] = convertSbyteToByte(this.readSByte());
            }
            UTF8Encoding utf8Encoding = new UTF8Encoding();
            return utf8Encoding.GetString(array);
        }

        public string readStringUTF()
        {
            short num = this.readShort();
            byte[] array = new byte[(int)num];
            for (int i = 0; i < (int)num; i++)
            {
                array[i] = convertSbyteToByte(this.readSByte());
            }
            UTF8Encoding utf8Encoding = new UTF8Encoding();
            return utf8Encoding.GetString(array);
        }

        public string readUTF()
        {
            return this.readStringUTF();
        }

        public int read()
        {
            if (this.posRead < this.buffer.Length)
            {
                return (int)this.readSByte();
            }
            return -1;
        }

        public int read(ref sbyte[] data)
        {
            if (data == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = this.readSByte();
                if (this.posRead > this.buffer.Length)
                {
                    return -1;
                }
                num++;
            }
            return num;
        }

        public int readz(sbyte[] data)
        {
            if (data == null)
            {
                return 0;
            }
            int num = 0;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = this.readSByte();
                if (this.posRead > this.buffer.Length)
                {
                    return -1;
                }
                num++;
            }
            return num;
        }

        public void readFully(ref sbyte[] data)
        {
            if (data == null || data.Length + this.posRead > this.buffer.Length)
            {
                return;
            }
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = this.readSByte();
            }
        }

        public int available()
        {
            return this.buffer.Length - this.posRead;
        }

        public static byte convertSbyteToByte(sbyte var)
        {
            return var.toByte();
        }

        public static byte[] convertSbyteToByte(sbyte[] var)
        {
            byte[] array = new byte[var.Length];
            for (int i = 0; i < var.Length; i++)
            {
                array[i] = var[i].toByte();
            }
            return array;
        }

        public void Close()
        {
            this.buffer = null;
        }

        public void close()
        {
            this.buffer = null;
        }

        public bool readbool()
        {
            return readBoolean();
        }

        public sbyte[] buffer;

        private int posRead;

        private int posMark;
    }
}
