namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public enum DataBehaviourType
    {
        Create,
        Delete,
        Update,
        Read,
        ReadList
    }

    public class DataBehaviour
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DataBehaviourType Type { get; set; }
        public string Expression { get; set; }
    }
}