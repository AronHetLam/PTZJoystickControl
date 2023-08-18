using PtzJoystickControl.Core.Devices;
using System.Dynamic;

namespace PtzJoystickControl.Core.ViscaCommands;
public class ViscaCommandBuilder
{
    private byte _addressByte;
    public byte Address { get => (byte)(_addressByte + 0x0f);
        set {
            if (value >= 0 && value <= 0x08)
                _addressByte = (byte)(0x80 + value);
            else 
                throw new ArgumentOutOfRangeException("Must be between 0x00 and 0x08 inclusive.");
        }
    }

    public ViscaCommandBuilder(byte address) { 
        Address = address;
    }

    private bool ValidateBufferCapacity(byte[] buffer, int index, int cmdLength) {
        return buffer.Length - index >= cmdLength ? true : false;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildPowerCommand(ref byte[] buffer, ref ushort index, Power powerCmd)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;

        // 8x 01 04 00 0X FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)CommandType.Power;
        buffer[index++] = (byte)powerCmd;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildPowerInquiry(ref byte[] buffer, ref ushort index)
    {
        if (!ValidateBufferCapacity(buffer, index, 5)) return false;
        
        // 8x 09 04 00 FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Inquiry;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)InquiryType.Power;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildPanTiltCommand(ref byte[] buffer, ref ushort index, byte panSpeed, byte tiltSpeed, PanDir panDir, TiltDir tiltDir)
    {
        if (!ValidateBufferCapacity(buffer, index, 9)) return false;
        
        // 8x 01 06 01 VV WW XX YY FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.PanTilt;
        buffer[index++] = (byte)CommandType.PanTilt;
        buffer[index++] = panSpeed;
        buffer[index++] = tiltSpeed;
        buffer[index++] = (byte)panDir;
        buffer[index++] = (byte)tiltDir;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildZoomCommand(ref byte[] buffer, ref ushort index, byte zoomCmd)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;
        
        // 8x 01 04 07 xp FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)CommandType.PanTiltZoomAndLimit;
        buffer[index++] = zoomCmd;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildFocusCommand(ref byte[] buffer, ref ushort index, byte focusCmd)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;
        
        // 8x 01 04 08 2p FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)CommandType.Focus;
        buffer[index++] = focusCmd;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildFocusModeCommand(ref byte[] buffer, ref ushort index, FocusMode focusModeCmd)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;
        
        // 8x 01 04 08 2p FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)CommandType.FocusMode;
        buffer[index++] = (byte)focusModeCmd;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildFocusLockCommand(ref byte[] buffer, ref ushort index, FocusLock focusLockCmd)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;
        
        // 8x 0a 04 68 0x FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)CommandCategory.FocusLock;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)CommandType.FocusLock;
        buffer[index++] = (byte)focusLockCmd;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildPresetCommand(ref byte[] buffer, ref ushort index, Preset presetCmd, byte presetCmdNumber)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;
        
        // 8x 01 04 3F 0x pp FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.Camera;
        buffer[index++] = (byte)CommandType.Preset;
        buffer[index++] = (byte)presetCmd;
        buffer[index++] = presetCmdNumber;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }

    /// <returns>False if command doesn't fit inside buffer. </returns>
    public bool BuildPresetRecallSpeedCommand(ref byte[] buffer, ref ushort index, byte presetRecallSpeed)
    {
        if (!ValidateBufferCapacity(buffer, index, 6)) return false;
        
        // 8x 01 06 01 pp FF
        buffer[index++] = _addressByte;
        buffer[index++] = (byte)PacketType.Command;
        buffer[index++] = (byte)CommandCategory.PanTilt;
        buffer[index++] = 0x01;
        buffer[index++] = presetRecallSpeed;
        buffer[index++] = (byte)Terminator.Terminate;

        return true;
    }
}
