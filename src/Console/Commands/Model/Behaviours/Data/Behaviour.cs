using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace Omnia.CLI.Commands.Model.Behaviours.Data
{
    public enum BehaviourType
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

    public class Behaviour
    {
        public string Name { get; set; }
        public string Attribute => GetAttribute();
        public BehaviourType Type { get; set; }
        public string Expression { get; set; }

        private string GetAttribute()
        {
            switch (Type)
            {
                case BehaviourType.Action:
                    return Name.Substring("On".Length, Name.Length - "PropertyChange".Length - 2);
                case BehaviourType.Formula:
                    return Name.Substring("Get".Length, Name.Length - 3);
                case BehaviourType.BeforeCollectionEntityInitialize:
                    return Name.Substring("Before".Length, Name.Length - "EntityInitialize".Length - 6);
                default:
                    break;
            }

            return null;

        }
    }
}