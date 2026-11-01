namespace BBQKing.Restoration
{
    /// <summary>Where a job stands, as shown on the jobs board.</summary>
    public enum JobStatus
    {
        /// <summary>An earlier job must be done first.</summary>
        Locked,
        /// <summary>Ready to work on.</summary>
        Available,
        Done,
    }
}
