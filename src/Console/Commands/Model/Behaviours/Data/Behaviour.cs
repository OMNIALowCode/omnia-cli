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
        [JsonConverter(typeof(StringEnumConverter))]
        public BehaviourType Type { get; set; }
        public string Expression { get; set; }

        private string GetAttribute()
        {
            switch (Type)
            {
                case BehaviourType.Action:
                    return Name.Substring("On".Length, Name.Length - "PropertyChange".Length - 2);
                // case BehaviourType.Formula:
                //     throw new NotImplementedException();
                // case BehaviourType.BeforeCollectionEntityInitialize:
                //     throw new NotImplementedException();
                default:
                    break;
            }

            return null;

        }
    }
}