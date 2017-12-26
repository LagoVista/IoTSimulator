﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LagoVista.Simulator.Core.Resources {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SimulatorCoreResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SimulatorCoreResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.Simulator.Core.Resources.SimulatorCoreResources", typeof(SimulatorCoreResources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Event Hub Name.
        /// </summary>
        internal static string EditSimulator_EventHubName {
            get {
                return ResourceManager.GetString("EditSimulator_EventHubName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry the key you have specified is already in use..
        /// </summary>
        internal static string Err_DuplicateKey {
            get {
                return ResourceManager.GetString("Err_DuplicateKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to We could not find your password or access key in the devices secure storage.
        ///
        ///It is possible that you (or someone else) created this simulator on a different device.
        ///
        ///To use this simulator you will need to re-enter the resource credentials..
        /// </summary>
        internal static string PasswordEntry_NotFound {
            get {
                return ResourceManager.GetString("PasswordEntry_NotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password and Confirm Password must match..
        /// </summary>
        internal static string SecureStorage_PasswordConfirmPasswordMisMatch {
            get {
                return ResourceManager.GetString("SecureStorage_PasswordConfirmPasswordMisMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password must be between 6 and 20 characters and include only include alpha numeric values and the special characters !@#$%^&amp;*.
        /// </summary>
        internal static string SecureStorage_PasswordFormat {
            get {
                return ResourceManager.GetString("SecureStorage_PasswordFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password is required.
        /// </summary>
        internal static string SecureStorage_PasswordIsRequired {
            get {
                return ResourceManager.GetString("SecureStorage_PasswordIsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, you have entered an incorrect password to unlock your password storage..
        /// </summary>
        internal static string SecureStorage_WrongPassword {
            get {
                return ResourceManager.GetString("SecureStorage_WrongPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could Not Send Message.
        /// </summary>
        internal static string SendMessage_CouldNotSendMessage {
            get {
                return ResourceManager.GetString("SendMessage_CouldNotSendMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ERROR - Please Check Output.
        /// </summary>
        internal static string SendMessage_Error {
            get {
                return ResourceManager.GetString("SendMessage_Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error Sending Message.
        /// </summary>
        internal static string SendMessage_ErrorSendingMessage {
            get {
                return ResourceManager.GetString("SendMessage_ErrorSendingMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid Binary Payload.
        /// </summary>
        internal static string SendMessage_InvalidBinaryPayload {
            get {
                return ResourceManager.GetString("SendMessage_InvalidBinaryPayload", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message Published.
        /// </summary>
        internal static string SendMessage_MessagePublished {
            get {
                return ResourceManager.GetString("SendMessage_MessagePublished", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Message Sent.
        /// </summary>
        internal static string SendMessage_MessageSent {
            get {
                return ResourceManager.GetString("SendMessage_MessageSent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access key is required..
        /// </summary>
        internal static string Simulator_AccessKeyIsRequired {
            get {
                return ResourceManager.GetString("Simulator_AccessKeyIsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connected!.
        /// </summary>
        internal static string Simulator_Connected {
            get {
                return ResourceManager.GetString("Simulator_Connected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Disconnected.
        /// </summary>
        internal static string Simulator_Disconnected {
            get {
                return ResourceManager.GetString("Simulator_Disconnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Editing will Disconnect this session.  Do you wish to continue?.
        /// </summary>
        internal static string Simulator_EditDisconnect {
            get {
                return ResourceManager.GetString("Simulator_EditDisconnect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error Connecting.
        /// </summary>
        internal static string Simulator_ErrorConnecting {
            get {
                return ResourceManager.GetString("Simulator_ErrorConnecting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password is required..
        /// </summary>
        internal static string Simulator_PasswordIsRequired {
            get {
                return ResourceManager.GetString("Simulator_PasswordIsRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter access key.
        /// </summary>
        internal static string Simulator_PromptAccessKey {
            get {
                return ResourceManager.GetString("Simulator_PromptAccessKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter password.
        /// </summary>
        internal static string Simulator_PromptPassword {
            get {
                return ResourceManager.GetString("Simulator_PromptPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to -edit access key-.
        /// </summary>
        internal static string SimulatorEdit_EditAccesKey_Link {
            get {
                return ResourceManager.GetString("SimulatorEdit_EditAccesKey_Link", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access Key.
        /// </summary>
        internal static string SimulatorEdit_EditAccessKey {
            get {
                return ResourceManager.GetString("SimulatorEdit_EditAccessKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Password.
        /// </summary>
        internal static string SimulatorEdit_EditPassword {
            get {
                return ResourceManager.GetString("SimulatorEdit_EditPassword", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to -edit password-.
        /// </summary>
        internal static string SimulatorEdit_EditPassword_Link {
            get {
                return ResourceManager.GetString("SimulatorEdit_EditPassword_Link", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You have selected to store passwords and/or access keys on your device.  You need to create or unlock your secure storage before these values can be stored..
        /// </summary>
        internal static string SimulatorEdit_UnlockRequired {
            get {
                return ResourceManager.GetString("SimulatorEdit_UnlockRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Are you sure you want to reset your secure storage password? 
        ///
        ///If you do so, any password or access keys for accessing server side resources with your simulators will need to be rentered. 
        ///
        ///This cannot be un-done.
        /// </summary>
        internal static string UnlockPassword_ResetPassword_Prompt {
            get {
                return ResourceManager.GetString("UnlockPassword_ResetPassword_Prompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reset Password?.
        /// </summary>
        internal static string UnlockPassword_ResetPassword_Title {
            get {
                return ResourceManager.GetString("UnlockPassword_ResetPassword_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sorry, you have entered an incorrect password to unlock your devices secure storage..
        /// </summary>
        internal static string UnlockPassword_WrongPassword {
            get {
                return ResourceManager.GetString("UnlockPassword_WrongPassword", resourceCulture);
            }
        }
    }
}
