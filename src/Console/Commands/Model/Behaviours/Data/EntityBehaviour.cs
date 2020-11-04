using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public enum EntityBehaviourType
    {
        Action,
        AfterChange,
        BeforeChange,
        Formula,
        BeforeSave,
        AfterSave,
        Initialize,
        BeforeCollectionEntityInitialize
    }

    public class EntityBehaviour
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Attribute { get; set; }
        public EntityBehaviourType Type { get; set; }
        public string Expression { get; set; }

    }
}