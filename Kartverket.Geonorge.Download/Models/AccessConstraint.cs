namespace Kartverket.Geonorge.Download.Models
{
    public class AccessConstraint
    {
        public static string Restricted = "restricted";
        public static string NorgeDigitalRestricted = "norway digital restricted";

        public string Constraint { get; set; }

        public bool IsOpen() => string.IsNullOrWhiteSpace(Constraint);

        public bool IsRestricted() => !IsOpen();

        public AccessConstraint() { }

        public AccessConstraint(string constraint)
        {
            Constraint = constraint;
        }
    }
}