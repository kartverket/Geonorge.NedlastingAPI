namespace Kartverket.Geonorge.Download.Models
{
    public class AccessConstraint
    {
        public string Constraint { get; set; }

        public bool IsOpen() => string.IsNullOrWhiteSpace(Constraint);

        public bool IsRestricted() => !IsOpen();
    }
}