namespace PtzJoystickControl.Core.Devices;

public enum Protocol
{
    Tcp,
    Udp
}

public enum Terminator : byte
{
    Terminate = 0xFF
}

public enum PacketType : byte
{
    Command = 0x01,
    Inquiry = 0x09
}

public enum CommandCategory : byte
{
    Interface = 0x00, 
    Camera = 0x04,
    PanTilt = 0x06,
    FocusLock = 0x0a //This thinks it's special!? maybe custom command from PTZ Optics...
}

public enum CommandType : byte
{
    Power = 0x00,
    PanTilt = 0x01,
    PanTiltAbsolute = 0x02,
    PanTiltRelative = 0x03,
    PanTiltHome = 0x04,
    PanTiltReset = 0x05,
    PanTiltZoomAndLimit = 0x07,
    PanTiltZoomDirect = 0x47,
    Focus = 0x08,
    FocusDirect = 0x48,
    FocusMode = 0x38,
    FocusLock = 0x68, //This thinks it's special!? maybe custom command from PTZ Optics...
    Preset = 0x3F,
}

public enum InquiryType : byte
{
    Power = 0x00
}

public enum ReplyType : byte
{
    Ack = 0x40,
    Complete = 0x50,
    Error = 0x60,
}

public enum Power : byte
{
    On = 0x02,
    Off = 0x03,
    InternalError = 0x04
}

public enum PanDir : byte
{
    Left = 0x01,
    Right = 0x02,
    Stop = 0x03,
}

public enum TiltDir : byte
{
    Up = 0x01,
    Down = 0x02,
    Stop = 0x03,
}

public enum ZoomDir : byte
{
    Stop = 0x00,
    Tele = 0x20,
    Wide = 0x30,
}

public enum FocusDir : byte
{
    Stop = 0x00,
    Far = 0x20,
    Near = 0x30,
}

public enum FocusMode : byte
{
    Auto = 0x02,
    Manual = 0x03,
    Toggle = 0x10,
}

public enum FocusLock : byte //This thinks it's special!? maybe custom command from PTZ Optics...
{
    Lock = 0x02,
    Unlock = 0x03,
}

//Hack: Reverse order to make Recall come first in GUI
public enum Preset : byte
{
    Recall = 0x02,
    Set = 0x01,
    Reset = 0x00,
}

public enum ViscaIPHeaderType : ushort
{
    Command = 0x0100,
    Inquery = 0x0110,
    CommandReply = 0x0111,
    SettingCommand = 0x0120,
    ControlCommand = 0x0200,
    ControlReply = 0x0201,
}
