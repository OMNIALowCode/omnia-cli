namespace Omnia.CLI.Commands.Model.Apply.Data.UI
{
    public enum UIBehaviourType
    {
        Change,
        AfterChange,
        BeforeChange,
        BeforeSave,
        Initialize
    }

    public class UIBehaviour
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Element { get; set; }
        public UIBehaviourType Type { get; set; }
        public string Expression { get; set; }
    }
}