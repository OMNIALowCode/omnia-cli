namespace Omnia.CLI.Commands.Model.Apply.Data.UI
{
    public enum UIEntityBehaviourType
    {
        Action,
        AfterChange,
        BeforeChange,
        BeforeSave,
        Initialize
    }

    public class UIEntityBehaviour
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Attribute { get; set; }
        public UIEntityBehaviourType Type { get; set; }
        public string Expression { get; set; }
    }
}