// GetHashCode() implementations follow this
// see https://stackoverflow.com/a/263416

using System;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    class DocumentState
    {
        public Document Document { get; private set; }
        public string Name { get; private set; }
        public bool IsDirty { get; private set; }


        public DocumentState(Document document)
        {
            Document = document;
            Name = document.Name;
            IsDirty = document.IsDirty;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            DocumentState other = obj as DocumentState;
            if (other == null)
                return false;

            if (Name != other.Name)
                return false;

            if (IsDirty != other.IsDirty)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Name == null ? 0 : Name.GetHashCode());
                hash = hash * 23 + IsDirty.GetHashCode();
                return hash;
            }
        }
    }
}
