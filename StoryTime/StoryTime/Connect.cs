using System;
using System.Linq;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Resources;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using StoryTime.UI;
using System.Diagnostics;

namespace StoryTime
{
	/// <summary>The object for implementing an Add-in.</summary>
	/// <seealso class='IDTExtensibility2' />
	public class Connect : IDTExtensibility2, IDTCommandTarget
	{
		/// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
		public Connect()
		{
		}

		/// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
		/// <param term='application'>Root object of the host application.</param>
		/// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
		/// <param term='addInInst'>Object representing this Add-in.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
		{

            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            // Only execute the startup code if the connection mode is a startup mode
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup || connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                try
                {
                    // Declare variables
                    EnvDTE80.Windows2 toolWins;
                    object objTemp = null;

                    // The Control ProgID for the user control
                    string ctrlProgID = "StoryTime.UI.UserStoryForm";
                    string ctrlProgName = "StoryTime";

                    // This guid must be unique for each different tool window,
                    // but you may use the same guid for the same tool window.
                    // This guid can be used for indexing the windows collection,
                    // for example: applicationObject.Windows.Item(guidstr)
                    string guidStr = UserStoryForm.GUID;

                    // Get the executing assembly...
                    System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

                    // Get Visual Studio's global collection of tool windows...
                    toolWins = (Windows2)_applicationObject.Windows;

                    // Create a new tool window, embedding the WorkSpaceWindow control inside it...
                    _windowToolWindow = toolWins.CreateToolWindow2(
                        _addInInstance, asm.Location, ctrlProgID, ctrlProgName, guidStr, ref objTemp);

                    // Pass the DTE object to the user control...
                    _userStoryWindow = (UserStoryForm)objTemp;
                    _userStoryWindow._dte = _applicationObject;

                    // and set the tool windows default size...
                    _windowToolWindow.Visible = true;		// MUST make tool window visible before using any methods or properties,
                    // otherwise exceptions will occurr.

                    //toolWin.Height = 400;
                    //toolWin.Width = 600;
                    _windowToolWindow.Height = 600;
                    _windowToolWindow.Width = 280;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
            }


            ////original:
            //if (connectMode == ext_ConnectMode.ext_cm_Startup)
            //{
            //    var bindings = new Dictionary<string, object>();
            //    for (var i = 1; i <= _applicationObject.Commands.Count; i++)
            //    {
            //        Command command = _applicationObject.Commands.Item(i);
            //        string name = command.Name;
            //        if (name.StartsWith("Project.Build", StringComparison.Ordinal))
            //        {
            //            name = name.Substring("Project.Build".Length);
            //            bindings.Add(name, command.Bindings);
            //        }
            //    }
            //    //foreach (var pair in bindings)
            //    //{
            //    //    _applicationObject.Commands.RemoveCommandBar(
            //    //}
            //}

            //_applicationObject = (DTE2)application;
            //_addInInstance = (AddIn)addInInst;
            //if(connectMode == ext_ConnectMode.ext_cm_UISetup)
            //{
            //    object []contextGUIDS = new object[] { };
            //    Commands2 commands = (Commands2)_applicationObject.Commands;
            //    string toolsMenuName;

            //    try
            //    {
            //        EnvDTE80.Windows2 toolWins;
            //        object objTemp = null;

            //        // The Control ProgID for the user control
            //        string ctrlProgID = "StoryTime.UI.UserStory";
            //        string ctrlProgName = "StoryTimeAddin";

            //        // This guid must be unique for each different tool window,
            //        // but you may use the same guid for the same tool window.
            //        // This guid can be used for indexing the windows collection,
            //        // for example: applicationObject.Windows.Item(guidstr)
            //        string guidStr = UserStory.GUID;

            //        // Get the executing assembly...
            //        System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();

            //        // Get Visual Studio's global collection of tool windows...
            //        toolWins = (Windows2)_applicationObject.Windows;

            //        // Create a new tool window, embedding the WorkSpaceWindow control inside it...
            //        _windowToolWindow = toolWins.CreateToolWindow2(
            //            _addInInstance, asm.Location, ctrlProgID, ctrlProgName, guidStr, ref objTemp);

            //        // Pass the DTE object to the user control...
            //        _userStoryWindow = (UserStory)objTemp;
            //        _userStoryWindow.DTE = _applicationObject;

            //        // and set the tool windows default size...
            //        _windowToolWindow.Visible = true;		// MUST make tool window visible before using any methods or properties,
            //        // otherwise exceptions will occurr.

            //        //toolWin.Height = 400;
            //        //toolWin.Width = 600;
            //        _windowToolWindow.Height = 400;
            //        _windowToolWindow.Width = 280;

            //        //If you would like to move the command to a different menu, change the word "Tools" to the 
            //        //  English version of the menu. This code will take the culture, append on the name of the menu
            //        //  then add the command to that menu. You can find a list of all the top-level menus in the file
            //        //  CommandBar.resx.
            //        string resourceName;
            //        ResourceManager resourceManager = new ResourceManager("StoryTime.CommandBar", Assembly.GetExecutingAssembly());
            //        CultureInfo cultureInfo = new CultureInfo(_applicationObject.LocaleID);
					
            //        if(cultureInfo.TwoLetterISOLanguageName == "zh")
            //        {
            //            System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
            //            resourceName = String.Concat(parentCultureInfo.Name, "Tools");
            //        }
            //        else
            //        {
            //            resourceName = String.Concat(cultureInfo.TwoLetterISOLanguageName, "Tools");
            //        }
            //        toolsMenuName = resourceManager.GetString(resourceName);
            //    }
            //    catch
            //    {
            //        //We tried to find a localized version of the word Tools, but one was not found.
            //        //  Default to the en-US word, which may work for the current culture.
            //        toolsMenuName = "Tools";
            //    }

            //    //Place the command on the tools menu.
            //    //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            //    Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

            //    //Find the Tools command bar on the MenuBar command bar:
            //    CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
            //    CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

            //    //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
            //    //  just make sure you also update the QueryStatus/Exec method to include the new command names.
            //    try
            //    {
            //        //Add a command to the Commands collection:
            //        Command command = commands.AddNamedCommand2(_addInInstance, "StoryTime", "StoryTime", "Executes the command for StoryTime", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported+(int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

            //        //Add a control for the command to the tools menu:
            //        if((command != null) && (toolsPopup != null))
            //        {
            //            command.AddControl(toolsPopup.CommandBar, 1);
            //        }
            //    }
            //    catch(System.ArgumentException)
            //    {
            //        //If we are here, then the exception is probably because a command with that name
            //        //  already exists. If so there is no need to recreate the command and we can 
            //        //  safely ignore the exception.
            //    }
            //}
		}

		/// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
		/// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
		{
		}

		/// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />		
		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref Array custom)
		{
		}

		/// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
		/// <param term='custom'>Array of parameters that are host application specific.</param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref Array custom)
		{
		}
		
		/// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
		/// <param term='commandName'>The name of the command to determine state for.</param>
		/// <param term='neededText'>Text that is needed for the command.</param>
		/// <param term='status'>The state of the command in the user interface.</param>
		/// <param term='commandText'>Text requested by the neededText parameter.</param>
		/// <seealso class='Exec' />
		public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
		{
			if(neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
			{
				if(commandName == "StoryTime.Connect.StoryTime")
				{
					status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported|vsCommandStatus.vsCommandStatusEnabled;
					return;
				}
			}
		}

		/// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
		/// <param term='commandName'>The name of the command to execute.</param>
		/// <param term='executeOption'>Describes how the command should be run.</param>
		/// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
		/// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
		/// <param term='handled'>Informs the caller if the command was handled or not.</param>
		/// <seealso class='Exec' />
		public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
		{
			handled = false;
			if(executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
			{
				if(commandName == "StoryTime.Connect.StoryTime")
				{
					handled = true;
					return;
				}
			}
		}
		private DTE2 _applicationObject;
		private AddIn _addInInstance;
        private UserStoryForm _userStoryWindow;
        private static Window _windowToolWindow;

	}
}