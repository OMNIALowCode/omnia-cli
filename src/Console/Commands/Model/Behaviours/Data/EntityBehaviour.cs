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
        public string Attribute => GetAttribute();
        public EntityBehaviourType Type { get; set; }
        public string Expression { get; set; }

        private string GetAttribute()
        {
            switch (Type)
            {
                case EntityBehaviourType.Action:
                    return Name.Substring("On".Length, Name.Length - "PropertyChange".Length - 2);
                case EntityBehaviourType.Formula:
                    return Name.Substring("Get".Length, Name.Length - 3);
                case EntityBehaviourType.BeforeCollectionEntityInitialize:
                    return Name.Substring("OnBefore".Length, Name.Length - "EntityInitialize".Length - 8);
                default:
                    break;
            }

            return null;

        }
    }
}