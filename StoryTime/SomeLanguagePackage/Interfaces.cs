using System;
using System.Runtime.InteropServices;

namespace SOME.SomeLanguageService
{
    /// <summary>
    /// Interface implemented by an engine object.
    /// </summary>
    [Guid(GuidList.guidSomeLangServiceEngineString)]
    [ComVisible(true)]
    public interface IEngine
    {
        /// <summary>Gets the copyright information about the engine.</summary>
        string Copyright { get; }
        /// <summary>Evaluates an expression.</summary>
        object Evaluate(string expression);
        /// <summary>Executes a command.</summary>
        void Execute(string text);
        /// <summary>Executes the content of a file.</summary>
        void ExecuteFile(string fileName);
        /// <summary>Executes the command in console mode.</summary>
        void ExecuteToConsole(string text);
        /// <summary>Gets the value of a variable.</summary>
        object GetVariable(string name);
        /// <summary>Parse the text and finds if it can be executed.</summary>
        bool ParseInteractiveInput(string text, bool allowIncompleteStatement);
        /// <summary>Executes the commands in the console.</summary>
        int RunInteractive();
        /// <summary>Sets the value of a variable.</summary>
        void SetVariable(string name, object value);
        /// <summary>Sets the standard error for the engine.</summary>
        System.IO.Stream StdErr { get; set; }
        /// <summary>Sets the standard input for the engine.</summary>
        System.IO.Stream StdIn { get; set; }
        /// <summary>Sets the standard output for the engine.</summary>
        System.IO.Stream StdOut { get; set; }
        /// <summary>Gets the version of the engine.</summary>
        Version Version { get; }
    }

    /// <summary>
    /// This is the definition of the interface exposed by the service defined by the
    /// IronPython console package.
    /// </summary>
    [Guid(GuidList.guidSomeLangServiceEngineProviderString)]
    [ComVisible(true)]
    public interface ISomeEngineProvider
    {
        /// <summary>Gets the instance of the engine shared between different components.</summary>
        IEngine GetSharedEngine();
        /// <summary>Creates a new instance of the engine.</summary>
        IEngine CreateNewEngine();
    }

    public interface IConsoleText
    {
        /// <summary>
        /// Returns the text inside a line in the console up to a specific colums.
        /// The skipReadOnly flag is used to specify if the text that is inside the
        /// read-only region should be skipped from the return value.
        /// </summary>
        string TextOfLine(int line, int endColumn, bool skipReadOnly);
    }

}