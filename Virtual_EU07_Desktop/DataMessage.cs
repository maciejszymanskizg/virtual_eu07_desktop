using System;
using System.Drawing.Text;
using System.Numerics;
using System.Security.Cryptography;

public class DataMessage
{
    public enum MessageType : byte
    {
        MESSAGE_TYPE_INVALID_MESSAGE = 0x0,
        MESSAGE_TYPE_REQUEST_DATA = 0x1,
        MESSAGE_TYPE_RESPONSE_DATA = 0x2,
        MESSAGE_TYPE_SEND_DATA = 0x4,
        MESSAGE_TYPE_CONFIRM_DATA = 0x8
    }

    public class MessageHeader
    {
        private UInt32 magic;
        private UInt16 message_type;
        private UInt16 number_of_items;

        public static UInt32 MessageHeaderMagic = 0x39fd47ca;

        public MessageHeader()
        {
            this.magic = MessageHeaderMagic;
            this.message_type = (UInt16)MessageType.MESSAGE_TYPE_INVALID_MESSAGE;
            this.number_of_items = 0;
        }

        public MessageHeader(MessageType message_type)
        {
            this.magic = MessageHeaderMagic;
            this.message_type = (UInt16)message_type;
            this.number_of_items = 0;
        }

        public MessageHeader(UInt16 message_type, UInt16 number_of_items)
        {
            this.magic = MessageHeaderMagic;
            this.message_type = message_type;
            this.number_of_items = number_of_items;
        }

        public MessageHeader(byte[] buffer)
        {
            this.magic = BitConverter.ToUInt32(buffer, 0);
            this.message_type = BitConverter.ToUInt16(buffer, sizeof(UInt32));
            this.number_of_items = BitConverter.ToUInt16(buffer, sizeof(UInt32) + sizeof(UInt16));
        }

        public byte[] GetRawData()
        {
            byte[] buffer = new byte[sizeof(UInt32) + 2 * sizeof(UInt16)];

            Buffer.BlockCopy(BitConverter.GetBytes(this.magic), 0, buffer, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(this.message_type), 0, buffer, sizeof(UInt32), sizeof(UInt16));
            Buffer.BlockCopy(BitConverter.GetBytes(this.number_of_items), 0, buffer, sizeof(UInt32) + sizeof(UInt16), sizeof(UInt16));

            return buffer;
        }

        public MessageType GetMessageType()
        {
            return (MessageType)this.message_type;
        }

        public int GetNumberOfItems()
        {
            return this.number_of_items;
        }

        public UInt32 GetMagic()
        {
            return this.magic;
        }

        public static int GetMessageHeaderSize()
        {
            return (sizeof(UInt32) + 2 * sizeof(UInt16));
        }

        public void IncreaseNumberOfItems()
        {
            this.number_of_items++;
        }
    }

    public struct MessageItem
    {
        private UInt32 id;
        private UInt32 value;

        public MessageItem(UInt32 id, UInt32 value)
        {
            this.id = id;
            this.value = value;
        }
        public MessageItem(byte[] buffer)
        {
            this.id = BitConverter.ToUInt32(buffer, 0);
            this.value = BitConverter.ToUInt32(buffer, sizeof(UInt32));
        }

        public byte[] GetRawData()
        {
            byte[] buffer = new byte[2 * sizeof(UInt32)];

            Buffer.BlockCopy(BitConverter.GetBytes(this.id), 0, buffer, 0, sizeof(UInt32));
            Buffer.BlockCopy(BitConverter.GetBytes(this.value), 0, buffer, sizeof(UInt32), sizeof(UInt32));

            return buffer;
        }

        public UInt32 GetId()
        {
            return this.id;
        }
        public UInt32 GetValue()
        {
            return this.value;
        }

        public static int GetMessageItemSize()
        {
            return (2 * sizeof(UInt32));
        }
    }

    private List<MessageItem> message_items;
    private MessageHeader header;

    public DataMessage(MessageType message_type)
    {
        this.header = new MessageHeader(message_type);
        this.message_items = new List<MessageItem>();
    }

    public DataMessage(byte[] buffer)
    {
        this.header = new MessageHeader(buffer);
        this.message_items = new List<MessageItem>();

        if (header.GetMagic() == MessageHeader.MessageHeaderMagic)
        {
            for (int i = 0; i < (int) header.GetNumberOfItems(); i++)
            {
                this.message_items.Add(new MessageItem(buffer.Skip(MessageHeader.GetMessageHeaderSize() + MessageItem.GetMessageItemSize()).ToArray()));
            }
        }
        else
        {
            Console.WriteLine(String.Format("Invalid magic value in header (expected {0:x} actual {1:x}", MessageHeader.MessageHeaderMagic, header.GetMagic()));
        }
    }

    public List<MessageItem> GetMessageItems()
    {
        return this.message_items;
    }

    public byte[] getRawData()
    {
        byte[] buffer = new byte[MessageHeader.GetMessageHeaderSize() + this.header.GetNumberOfItems() * MessageItem.GetMessageItemSize()];

        Buffer.BlockCopy(this.header.GetRawData(), 0, buffer, 0, MessageHeader.GetMessageHeaderSize());

        for (uint i = 0; i < this.message_items.Count; i++)
        {
            Buffer.BlockCopy(this.message_items[(int) i].GetRawData(), 0, buffer, (int) (MessageHeader.GetMessageHeaderSize() + i * MessageItem.GetMessageItemSize()), MessageItem.GetMessageItemSize());
        }

        return buffer;
    }

    public int getRawDataSize()
    {
        return (MessageHeader.GetMessageHeaderSize() + this.message_items.Count * MessageItem.GetMessageItemSize());
    }

    public MessageType GetMessageType()
    {
        return this.header.GetMessageType();
    }

    public void AddMessageItem(MessageItem message_item)
    {
        this.message_items.Add(message_item);
        this.header.IncreaseNumberOfItems();
    }
}
