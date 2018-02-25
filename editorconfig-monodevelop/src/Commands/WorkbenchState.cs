// GetHashCode() implementations follow this
// see https://stackoverflow.com/a/263416

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace EditorConfig.Addin
{
    class WorkbenchState
    {
        public List<DocumentState> DocumentStates { get; private set; }
        public Document ActiveDocument { get; private set; }


        public WorkbenchState()
        {
            DocumentStates = new List<DocumentState>();
        }

        public void Reset()
        {
            DocumentStates.Clear();
            ActiveDocument = null;
        }

        public void Save()
        {
            DocumentStates =
                (from document
                 in IdeApp.Workbench.Documents
                 select new DocumentState(document)
                ).ToList();
            ActiveDocument = IdeApp.Workbench.ActiveDocument;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            WorkbenchState other = obj as WorkbenchState;
            if (other == null)
                return false;

            if (!DocumentStates.SequenceEqual(other.DocumentStates))
                return false;

            if (ActiveDocument == null)
            {
                if (other.ActiveDocument != null)
                    return false;
            }
            else
            {
                if (!ActiveDocument.Equals(other.ActiveDocument))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + DocumentStates.GetHashCode();
                hash = hash * 23 + (ActiveDocument == null ? 0 : ActiveDocument.GetHashCode());
                return hash;
            }
        }
    }
}
