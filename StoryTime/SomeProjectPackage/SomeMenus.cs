using System;
using System.ComponentModel.Design;

namespace SOME.SomeProjectPackage
{
    /// <summary>
    /// CommandIDs matching the commands defined symbols in PkgCmd.vsct
    /// </summary>
    public sealed class SomeMenus
    {
        internal static readonly Guid guidSomeProjectCmdSet = new Guid(GuidList.guidSomeProjectPackageCmdSetString);
        internal static readonly CommandID SetAsMain = new CommandID(guidSomeProjectCmdSet, 0x3001);
    }
}

