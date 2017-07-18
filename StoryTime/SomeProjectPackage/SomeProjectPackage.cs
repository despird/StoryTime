// VsPkg.cs : Implementation of SomeLanguageProject
//

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Project;
using SOME.SomeLanguageService.Library;

namespace SOME.SomeProjectPackage
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the registration utility (regpkg.exe) that this class needs
    // to be registered as package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideObject(typeof(GeneralPropertyPage))]
    [ProvideObject(typeof(SomeBuildPropertyPage))]

    // In order be loaded inside Visual Studio in a machine that has not the VS SDK installed, 
    // package needs to have a valid load key (it can be requested at 
    // http://msdn.microsoft.com/vstudio/extend/). This attributes tells the shell that this 
    //[ProvideService(typeof(SOME.SomeLanguageService.SomeLanguageService), ServiceName = "SOME Language Service")]
    //[ProvideLanguageExtension(typeof(SOME.SomeLanguageService.SomeLanguageService), SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_EXT1)]
    //[ProvideLanguageExtension(typeof(SOME.SomeLanguageService.SomeLanguageService), SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_EXT2)]

    [ProvideLanguageExtension(SOME.SomeLanguageService.GuidList.guidSomeLangServiceClassString, SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_EXT1)]
    [ProvideLanguageExtension(SOME.SomeLanguageService.GuidList.guidSomeLangServiceClassString, SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_EXT2)]
    [ProvideEditorExtension(typeof(EditorFactory), SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_EXT1, 32)]
    [ProvideEditorExtension(typeof(EditorFactory), SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_EXT2, 32)]

    [ProvideLanguageService(SOME.SomeLanguageService.GuidList.guidSomeLangServiceClassString,
        SOME.SomeLanguageService.SomeLanguageService.LANGUAGE_NAME,
        111,    //resource id for language name
        CodeSense = true,
        DefaultToInsertSpaces = true,
        EnableCommenting = true,
        MatchBraces = true,
        ShowCompletion = true,
        ShowMatchingBrace = true)]

    [ProvideProjectFactory(
        typeof(SomeProjectFactory),
        "Some",
        "Some Project Files (*.smproj);*.smproj",
        "smproj",
        "smproj",
        ".\\NullPath",
        LanguageVsTemplate = "Some"    )]

    [SingleFileGeneratorSupportRegistrationAttribute(typeof(SomeProjectFactory))]
    [RegisterMsBuildTargets("Some_1.1", @".\Some.targets")]
    [ProvideBindingPathAttribute]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(SomeToolWindow))]
    [Guid(GuidList.guidSomeProjectPackagePkgString)]
    public sealed class SomeProjectPackage : ProjectPackage, IVsInstalledProduct, IOleComponent
    {
        private SomeLibraryManager libraryManager;
        private uint componentID;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public SomeProjectPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }



        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidSomeProjectPackageCmdSet, (int)PkgCmdIDList.cmdidSetAsMain);
                MenuCommand menuItem = new MenuCommand(MenuItemCallback, menuCommandID);
                mcs.AddCommand(menuItem);
            }

            this.RegisterProjectFactory(new SomeProjectFactory(this));
            this.RegisterEditorFactory(new EditorFactory(this));

            IServiceContainer container = this as IServiceContainer;
            container.AddService(typeof(ISomeLibraryManager), CreateService, true);
        }
        #endregion

        private object CreateService(IServiceContainer container, Type serviceType)
        {
            object service = null;
            if (typeof(ISomeLibraryManager) == serviceType)
            {
                libraryManager = new SomeLibraryManager(this);
                service = libraryManager as ISomeLibraryManager;
                RegisterForIdleTime();
            }
            return service;
        }

        private void RegisterForIdleTime()
        {
            IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            if (componentID == 0 && mgr != null)
            {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime |
                                              (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal |
                                              (uint)_OLECADVF.olecadvfRedrawOff |
                                              (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 1000;
                int hr = mgr.FRegisterComponent(this, crinfo, out componentID);
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       "SomeLanguageProject",
                       string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.ToString()),
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }


        #region IVsInstalledProduct Members

        public int IdBmpSplash(out uint pIdBmp)
        {
            throw new NotImplementedException();
        }

        public int IdIcoLogoForAboutbox(out uint pIdIco)
        {
            throw new NotImplementedException();
        }

        public int OfficialName(out string pbstrName)
        {
            throw new NotImplementedException();
        }

        public int ProductDetails(out string pbstrProductDetails)
        {
            throw new NotImplementedException();
        }

        public int ProductID(out string pbstrPID)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override string ProductUserContext
        {
            get { throw new NotImplementedException(); }
        }

        public int FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return 1;
        }

        public int FDoIdle(uint grfidlef)
        {
            if (null != libraryManager)
            {
                libraryManager.OnIdle();
            }
            return 0;
        }

        public int FPreTranslateMessage(MSG[] pMsg)
        {
            return 0;
        }

        public int FQueryTerminate(int fPromptUser)
        {
            return 1;
        }

        public int FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        public IntPtr HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        public void OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved)
        {
        }

        public void OnAppActivate(int fActive, uint dwOtherThreadID)
        {
        }

        public void OnEnterState(uint uStateID, int fEnter)
        {
        }

        public void OnLoseActivation()
        {
        }

        public void Terminate()
        {
        }
    }
}