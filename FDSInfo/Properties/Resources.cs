using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace FDSInfo.Properties
{
  [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
  [DebuggerNonUserCode]
  [CompilerGenerated]
  internal class Resources
  {
    private static ResourceManager resourceMan;
    private static CultureInfo resourceCulture;

    internal Resources()
    {
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (FDSInfo.Properties.Resources.resourceMan == null)
          FDSInfo.Properties.Resources.resourceMan = new ResourceManager("FDSInfo.Properties.Resources", typeof (FDSInfo.Properties.Resources).Assembly);
        return FDSInfo.Properties.Resources.resourceMan;
      }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get
      {
        return FDSInfo.Properties.Resources.resourceCulture;
      }
      set
      {
        FDSInfo.Properties.Resources.resourceCulture = value;
      }
    }
  }
}
