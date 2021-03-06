using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Confix.Authoring
{
    public class Variable
    {
        [Key]
        public Guid Id { get; set; }

        public VariableState State { get; set; }

        public string Name { get; set; }

        public bool IsSecret { get; set; }

        public string? Namespace { get; set; }
    }

    public class VariableValue
    {
        public Guid Id { get; set; }

        public VariableKey Key{ get; set; }

        public string Value { get; set; }

        public VariableEncryptionInfo? Encryption { get; set; }
    }

    public class VariableEncryptionInfo
    {
        public string KeyProvider { get; set; }

        public string Key { get; set; }

        public string Algorithm { get; set; }
    }

    public class VariableKey
    {
        public Guid VariableId { get; set; }

        public Guid? ApplicationId { get; set; }

        public Guid? PartId { get; set; }

        public Guid? EnvironmentId { get; set; }
    }

    public enum VariableState
    {
        Active,
        Deprecated
    }
}
