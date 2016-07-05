namespace ManagedBass
{
    public static partial class Bass
    {
        /// <summary>
        /// Enables Airplay Receivers.
        /// </summary>
        public static bool EnableAirplayReceivers(int Receivers)
        {
            return Configure(Configuration.Airplay, Receivers);
        }
        
        const int BassDeviceAirplay = 0x1000000;

        /// <summary>
        /// Retrieves information on an Airplay Receiver.
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <param name="Info">A <see cref="DeviceInfo" /> object to retrieve the information into.</param>
        /// <returns>
        /// If successful, then <see langword="true" /> is returned, else <see langword="false" /> is returned.
        /// Use <see cref="LastError" /> to get the error code.
        /// </returns>
        /// <remarks>
        /// This function can be used to enumerate the available devices for a setup dialog.
        /// Device 0 is always the <see cref="NoSoundDevice"/> device, so if you should start at device 1 if you only want to list real devices.
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// A shared buffer is used for the Airplay receiver name information, which gets overwritten each time Airplay receiver information is requested, so it should be copied if needed.
        /// <see cref="EnableAirplayReceivers"/> can be used to change which of the receiver(s) are used.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        public static bool GetDeviceInfoAirplay(int Device, out DeviceInfo Info) => GetDeviceInfo(Device | BassDeviceAirplay, out Info);

        /// <summary>
        /// Retrieves information on an Airplay Receiver.
        /// </summary>
        /// <param name="Device">The device to get the information of... 0 = first.</param>
        /// <returns>An instance of the <see cref="DeviceInfo" /> structure is returned. Throws <see cref="BassException"/> on Error.</returns>
        /// <remarks>
        /// This function can be used to enumerate the available devices for a setup dialog.
        /// Device 0 is always the <see cref="NoSoundDevice"/> device, so if you should start at device 1 if you only want to list real devices.
        /// <para><b>Platform-specific</b></para>
        /// <para>
        /// A shared buffer is used for the Airplay receiver name information, which gets overwritten each time Airplay receiver information is requested, so it should be copied if needed.
        /// <see cref="EnableAirplayReceivers"/> can be used to change which of the receiver(s) are used.
        /// </para>
        /// </remarks>
        /// <exception cref="Errors.Device">The device number specified is invalid.</exception>
        public static DeviceInfo GetDeviceInfoAirplay(int Device) => GetDeviceInfo(Device | BassDeviceAirplay);
    }
}
