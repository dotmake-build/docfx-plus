using System;

namespace DotMake.DocfxPlus.Cli.Shfb
{
    /// <summary>
    /// This public enumerated type defines the API parent mode for a conceptual
    /// topic.
    /// </summary>
    [Serializable]
    public enum ApiParentMode
    {
        /// <summary>Not a parent to the API content</summary>
        None,
        /// <summary>Insert the API content before this element</summary>
        InsertBefore,
        /// <summary>Insert the API content after this element</summary>
        InsertAfter,
        /// <summary>Insert the API content as a child of this element</summary>
        InsertAsChild
    }
}
