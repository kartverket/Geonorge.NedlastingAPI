namespace Kartverket.Geonorge.Download.Models
{
    public class AccessConstraint
    {
        public static string Restricted = "restricted";
        public static string NorgeDigitalRestricted = "norway digital restricted";

        public string Constraint { get; set; }
        public string RequiredRole { get; set; }

        public bool IsOpen() => string.IsNullOrWhiteSpace(Constraint);

        public bool IsRestricted() => !IsOpen();

        public AccessConstraint() { }

        public AccessConstraint(string constraint, string requiredRole = null)
        {
            Constraint = constraint;
            RequiredRole = requiredRole;
        }
    }
}