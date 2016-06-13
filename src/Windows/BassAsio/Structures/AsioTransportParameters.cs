using System.Runtime.InteropServices;

namespace ManagedBass.Asio
{
    /// <summary>
    /// Used with <see cref="BassAsio.Future" /> and the Transport selector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class AsioTransportParameters
    {
        /// <summary>
        /// One of the <see cref="AsioTransportCommand"/> values (other values might be available).
        /// </summary>
        public AsioTransportCommand command;

        /// <summary>
        /// Number of samples data.
        /// </summary>
        public long SamplePosition;

        /// <summary>
        /// Track Index
        /// </summary>
        public int Track;

        /// <summary>
        /// 512 Tracks on/off
        /// </summary>
        public int[] TrackSwitches = new int[16];

        /// <summary>
        /// Max 64 characters.
        /// </summary>
        public string Future = string.Empty;
    }

    /// <summary>
    /// Asio Transport Command to be used with <see cref="AsioTransportParameters"/>.
    /// </summary>
    public enum AsioTransportCommand
    {
        /// <summary>
        /// Start
        /// </summary>
        Start = 1,

        /// <summary>
        /// Stop
        /// </summary>
        Stop = 2,

        /// <summary>
        /// Locate
        /// </summary>
        Locate = 3,

        /// <summary>
        /// Punch in
        /// </summary>
        PunchIn = 4,

        /// <summary>
        /// Punch out
        /// </summary>
        PunchOut = 5,

        /// <summary>
        /// Arm on
        /// </summary>
        ArmOn = 6,

        /// <summary>
        /// Arm off
        /// </summary>
        ArmOff = 7,

        /// <summary>
        /// Monitor on
        /// </summary>
        MonitorOn = 8,
        
        /// <summary>
        /// Monitor off
        /// </summary>
        MonitorOff = 9,

        /// <summary>
        /// Arm
        /// </summary>
        Arm = 10,

        /// <summary>
        /// Monitor
        /// </summary>
        Monitor = 11
    }
}