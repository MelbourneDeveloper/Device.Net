namespace Device.Net
{
    internal class VidPid
    {
        public uint? Vid { get; set; }
        public uint? Pid { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is VidPid vidPid)
            {
                if (!vidPid.Pid.HasValue && !vidPid.Vid.HasValue)
                {
                    return false;
                }

                var isEqual = vidPid.Vid == Vid && vidPid.Pid == Pid;

                return isEqual;
            }

            return false;
        }
    }
}
