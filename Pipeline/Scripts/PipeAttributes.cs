using UnityEngine;

namespace UVT.Pipe
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class PipeAppendableAttribute : System.Attribute
    {
        public string name;
        public bool attached;
        public bool separate;
        public System.Type sourceType;

        public PipeAppendableAttribute(string name, System.Type sourceType, bool attached, bool separate)
        {
            this.name = name;
            this.attached = attached;
            this.separate = separate;
            this.sourceType = sourceType;
        }
    }


    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class PipeStartingAttribute : System.Attribute
    {
        public string name;
        public bool attached;

        public PipeStartingAttribute(string name, bool attached)
        {
            this.name = name;
            this.attached = attached;
        }
    }

    public class PipeSelectorAttribute : PropertyAttribute
    {
        public System.Type Type;

        public PipeSelectorAttribute(System.Type type)
        {
            this.Type = type;
        }
    }
}