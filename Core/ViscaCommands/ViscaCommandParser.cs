using PtzJoystickControl.Core.Devices;

namespace PtzJoystickControl.Core.ViscaCommands;

public static class ViscaCommandParser
{
    public static int ParseReplyViscaAddress(byte[] buffer, out int startIndex)
    {
        throw new NotImplementedException();
    }

    public static void ParseReply(ViscaDeviceBase viscaDevice, byte[] buffer, int startIndex)
    {
        
        if (buffer.Length - startIndex < 2) throw new ArgumentException("Reply min length is 2 bytes.");

        //byte socket = (byte)(buffer[startIndex] & 0x0F); // not used
        byte replyType = (byte)(buffer[startIndex++] & 0xF0);

        switch (replyType)
        {
            case (byte)ReplyType.Ack:
                if (buffer[startIndex++] != (byte)Terminator.Terminate)
                    throw new Exception("Invalid response. Ack not terminated.");
                viscaDevice.Acked = true;
                viscaDevice.LastReceiveTime = DateTime.UtcNow;
                break;
            case (byte)ReplyType.Complete:
                if (buffer[startIndex] != (byte)Terminator.Terminate)
                    if(viscaDevice.InquiryReplyParser != null)
                        viscaDevice.InquiryReplyParser(viscaDevice, buffer, startIndex);
                viscaDevice.Acked = true;
                viscaDevice.Completed = true;
                viscaDevice.LastReceiveTime = DateTime.UtcNow;
                break;
            case (byte)ReplyType.Error:
                break;
            default:
                break;
        }

    }

    public static void ParsePowerInquiryReply(ViscaDeviceBase viscaDevice, byte[] buffer, int startIndex)
    {
        if (buffer.Length - startIndex < 2) throw new ArgumentException("Expected min 2 bytes");
        var value = (Power)buffer[startIndex++];
        if(buffer[startIndex++] != (byte)Terminator.Terminate) {
            viscaDevice.PowerState = null;
            throw new Exception("Invalid response. PowerInquiry not terminated.");
        }
        viscaDevice.PowerState = Enum.IsDefined(value) ? value : null;
    }
}
