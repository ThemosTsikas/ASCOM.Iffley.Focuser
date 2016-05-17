//tabs=4
// --------------------------------------------------------------------------------
//
// ASCOM Focuser driver for Iffley
//
// Description:	Driver for the Arduino-based focuser
//
// Implements:	ASCOM Focuser interface version: IFocuserV2 Interface
// Author:		(TTT) Themos Tsikas <themos.tsikas@gmail.com>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 13-May-2016	TTT	2.0.0	Initial edit, created from ASCOM driver template
// 14-May-2016	TTT	2.0.1	Added Tim Long's SettingsProvider hooks
// 15-May-2016  TTT 2.1.0   Added test program and SupportedAction ResetToZero
// 16-May-2016  TTT 2.1.1   refactored the ResetToZero function, in Dialog and via Action
// 16-May-2016  TTT 3.0.0   new protocol, "idIDz" until we figure out timings for larger steps
// --------------------------------------------------------------------------------
//



using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.IO.Ports;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;
using System.Text.RegularExpressions;

namespace ASCOM.Iffley
{
    //
    // Your driver's DeviceID is ASCOM.Iffley.Focuser
    //
    // The Guid attribute sets the CLSID for ASCOM.Iffley.Focuser
    // The ClassInterface/None addribute prevents an empty interface called
    // _Iffley from being created and used as the [default] interface
    //
    /// <summary>
    /// ASCOM Focuser Driver for Iffley.
    /// </summary>
    [Guid("c3aed112-260c-4de9-b329-4437984c6113")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Focuser : IFocuserV2
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        public const string driverID = "ASCOM.Iffley.Focuser";
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        public const string driverDescription = "Iffley Arduino unipolar stepper motor focuser";

        /// <summary>
        /// Themos's added variables. 
        /// Things I need to know in order to process requests
        public static bool m_Connected = false;        // am I connected to the Arduino
        private static SerialPort m_Port;              // the port the Arduino is on
        private static int m_Position;                 // the current position of the focuser
        private static ArrayList m_Actions;            // the supported actions
        private static int m_MaxIncrement;             // the end of travel position (begins at 0)
        private static int m_MaxStep;                  // the biggest step I can handle
        /// </summary>

        /// <summary>
        /// Private variable to hold an ASCOM Utilities object
        /// </summary>
        private Util utilities;

        /// <summary>
        /// Private variable to hold an ASCOM AstroUtilities object to provide the Range method
        /// </summary>
        private AstroUtils astroUtilities;

        /// <summary>
        /// Variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        internal static TraceLogger tl;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iffley"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Focuser()
        {
            tl = new TraceLogger("", "Iffley");
            tl.Enabled = Properties.Settings.Default.TraceEnabled;

            tl.LogMessage("Focuser", "Starting initialisation");

            m_Connected = false; // Initialise connected to false
            m_MaxIncrement = Convert.ToInt32(Properties.Settings.Default.MaxIncrement);
            m_MaxStep = Convert.ToInt32(Properties.Settings.Default.MaxStep);
            m_Actions = new ArrayList();
            m_Actions.Add("Focuser:ResetToZero");
            utilities = new Util(); //Initialise util object
            astroUtilities = new AstroUtils(); // Initialise astro utilities object

            tl.LogMessage("Focuser", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE IFocuserV2 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected

            using (SetupDialogForm F = new SetupDialogForm())
            {
                tl.LogMessage("SetupDialog", "");
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    // Persist device configuration values to the ASCOM Profile store
                    Properties.Settings.Default.Save();
                }
                else
                {
                    // revert to old values
                    Properties.Settings.Default.Reload();
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", m_Actions[0].ToString() + "...");
                return m_Actions;
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            if (actionName == "ResetToZero")
            {
                if (m_Connected)
                {
                    ResetToZero();
                    return "OK";
                }
                else
                {
                    tl.LogMessage("Action", "ResetToZero failed, not connected");
                    return "FAIL";
                }
            }
            else
            {
                FancyLogMessage("Action", "Action {0}, parameters {1} not implemented", actionName, actionParameters);
                throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
            }
        }

        public void CommandBlind(string command, bool raw)
        {
            tl.LogMessage("CommandBlind", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            tl.LogMessage("CommandBlindBool", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            tl.LogMessage("CommandString", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the tracelogger and util objects
            tl.Enabled = false;
            tl.Dispose();
            tl = null;
            utilities.Dispose();
            utilities = null;
            astroUtilities.Dispose();
            astroUtilities = null;
        }

        public bool Link
        {
            get
            {
                tl.LogMessage("Link Get", this.Connected.ToString());
                return this.Connected; // Direct function to the connected method, the Link method is just here for backwards compatibility
            }
            set
            {
                tl.LogMessage("Link Set", value.ToString());
                this.Connected = value; // Direct function to the connected method, the Link method is just here for backwards compatibility
            }
        }

        public string Description
        {
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                string driverInfo = "Iffley Arduino unipolar stepper motor focuser.";
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                string driverVersion = "3.0";
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                tl.LogMessage("InterfaceVersion Get", "2");
                return Convert.ToInt16("2");
            }
        }

        public string Name
        {
            get
            {
                string name = "Iffley";
                tl.LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region IFocuser Implementation

        public bool Absolute
        {
            get
            {
                tl.LogMessage("Absolute Get", Properties.Settings.Default.AbsoluteEnabled.ToString());
                return Properties.Settings.Default.AbsoluteEnabled; // from settings
            }
        }

        public void Halt()
        {
            tl.LogMessage("Halt", "Not implemented");
            throw new ASCOM.MethodNotImplementedException("Halt");
        }

        public bool IsMoving
        {
            get
            {
                // I don't envisage more than one client at a time for the focuser and 
                // all the moving is done while the Move function runs, so I am going to 
                // always say false, here.
                // It could be that a second client tries to execute one of these methods while the 
                // first client is in the middle of a Move, so maybe we should keep a variable for this.
                // care would be required to make sure there is no race conditions.
                tl.LogMessage("IsMoving Get", false.ToString());
                return false; // This focuser always moves instantaneously so no need for IsMoving ever to be True
            }
        }

        public bool Connected
        {
            get
            {
                // the easy part
                tl.LogMessage("Connected Get", m_Connected.ToString());
                return m_Connected;
            }
            set
            {
                tl.LogMessage("Connected Set", value.ToString());
                // we need to bring up the link or tear it down or do nothing
                if (m_Connected != value)
                {
                    // aha, we are asked to change the status of the link
                    if (value)
                    {
                        // Turn it on using the remembered name, I've set the Arduino to 19200 rate
                        m_Port = new SerialPort(Properties.Settings.Default.CommPortName, 19200);
                        tl.LogMessage("Connected Set", "Port " + Properties.Settings.Default.CommPortName + " created");

                        // This is high-ish because the Arduino can take its time to respond when it resets
                        m_Port.ReadTimeout = Convert.ToInt32(
                            Convert.ToDouble(Properties.Settings.Default.TimeoutSeconds) * (1000));
                        try
                        {
                            m_Port.Open();
                            tl.LogMessage("Connected Set", "Port Opened");
                        }
                        catch
                        {
                            throw new DriverException("Could not open port " + Properties.Settings.Default.CommPortName);
                        }
                        try
                        {
                            // The Arduino is programmed to report a version string 
                            string version = m_Port.ReadLine();
                            // and then the position. when it resets
                            string position = m_Port.ReadLine();
                            m_Position = Convert.ToInt32(position);
                            tl.LogMessage("Connected Set", "Confirmation " + Regex.Replace(version, @"\t|\r|\n", ""));
                        }
                        catch (TimeoutException)
                        {
                            // it took too long, assume that the Arduino is not physically connected
                            m_Port.Close();
                            m_Port = null;
                            m_Connected = false;
                            throw new DriverException("Iffley device did not respond in time");

                        }
                        // we got the version and the position so we assume all is well
                        // declare ourselves connected
                        m_Connected = true;
                    }
                    else
                    {
                        //Turn it off
                        m_Port.Close();
                        m_Port = null;
                        m_Connected = false;
                    }
                }

            }
        }


        public int MaxIncrement
        {
            get
            {
                tl.LogMessage("MaxIncrement Get", m_MaxIncrement.ToString());
                return m_MaxIncrement; // Maximum change in one move
            }
        }

        public int MaxStep
        {
            get
            {
                tl.LogMessage("MaxStep Get", m_MaxStep.ToString());
                return m_MaxStep; // Maximum extent of the focuser, so position range is 0 to this
            }
        }

        public void Move(int val)
        {

            int current, target;
            string position;
            tl.LogMessage("Move", val.ToString());
            current = m_Position;
            // The Arduino always reports an absolute position after each single-step 
            // command ("f" or "b", "F" or "B", "u" or "d" )
            if (Properties.Settings.Default.AbsoluteEnabled)
            {
                // we need to interpret val as an absolute position
                // so just check that it is within the limits
                if (val < 0) val = 0;
                if (val > m_MaxStep) val = m_MaxStep;
                // target is always an absolute position
                target = val;
            }
            else
            {
                // we need to interpret val as a relative move
                if (val >= 0)
                {
                    // make sure it's within limits
                    if (val > m_MaxIncrement) val = m_MaxIncrement;
                    // check that the resulting absolute position is within limits
                    // target is always an absolute position
                    if (current > (m_MaxStep - val)) target = m_MaxStep;
                    else target = current + val;
                }
                else
                {
                    // similar for negative relative moves
                    if (val < -m_MaxIncrement) val = -m_MaxIncrement;
                    if (current < -val) target = 0;
                    else target = current + val;
                }
            }
            // now that target is sensible we just do single steps until we get there 
            while (target != current)
            {
                if (false && (target > 100 + current))
                {
                    // we need to increase the position, ask the Arduino to turn the 
                    // stepper forward one giant step (100)
                    m_Port.Write("C");
                    // Arduino always reports the new position (as a string)
                    position = m_Port.ReadLine();
                    // convert it to a number
                    current = Convert.ToInt32(position);
                    m_Position = current;
                    tl.LogMessage("Move", "C step");
                }
                else if (target > 10 + current)
                {
                    // we need to increase the position, ask the Arduino to turn the 
                    // stepper forward one big step (10)
                    m_Port.Write("D");
                    // Arduino always reports the new position (as a string)
                    position = m_Port.ReadLine();
                    // convert it to a number
                    current = Convert.ToInt32(position);
                    m_Position = current;
                    tl.LogMessage("Move", "D step");
                }
                else if (target > current)
                {
                    // we need to increase the position, ask the Arduino to turn the stepper forward one step
                    m_Port.Write("I");
                    // Arduino always reports the new position (as a string)
                    position = m_Port.ReadLine();
                    // convert it to a number
                    current = Convert.ToInt32(position);
                    m_Position = current;
                    tl.LogMessage("Move", "I step");

                }
                else if (false && (target < current - 100))
                {
                    // we need to decrease the position, ask the Arduino to turn the 
                    // stepper backwards one giant step (100)
                    m_Port.Write("c");
                    position = m_Port.ReadLine();
                    current = Convert.ToInt32(position);
                    m_Position = current;
                    tl.LogMessage("Move", "c step");

                }
                else if (target < current - 10)
                {
                    // we need to decrease the position, ask the Arduino to turn the
                    // stepper backwards one big step (10)
                    m_Port.Write("d");
                    position = m_Port.ReadLine();
                    current = Convert.ToInt32(position);
                    m_Position = current;
                    tl.LogMessage("Move", "d step");

                }
                else if (target < current)
                {
                    // we need to decrease the position, ask the Arduino to turn the
                    // stepper backwards one step
                    m_Port.Write("i");
                    position = m_Port.ReadLine();
                    current = Convert.ToInt32(position);
                    m_Position = current;
                    tl.LogMessage("Move", "i step");
                }
            }
        }

        public int Position
        {
            get
            {
                if (Properties.Settings.Default.AbsoluteEnabled)
                {
                    // updated by Move, we don't ask the Arduino
                    tl.LogMessage("Position Get", m_Position.ToString());
                    return m_Position;
                }
                else
                {
                    throw new PropertyNotImplementedException("Position", false);
                }
            }
        }

        public double StepSize
        {
            get
            {
                tl.LogMessage("StepSize Get", Properties.Settings.Default.StepSize.ToString());
                return Convert.ToDouble(Properties.Settings.Default.StepSize);
            }
        }

        public bool TempComp
        {
            get
            {
                tl.LogMessage("TempComp Get", false.ToString());
                return false;
            }
            set
            {
                tl.LogMessage("TempComp Set", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("TempComp", false);
            }
        }

        public bool TempCompAvailable
        {
            get
            {
                tl.LogMessage("TempCompAvailable Get", false.ToString());
                return false; // Temperature compensation is not available in this driver
            }
        }

        public double Temperature
        {
            get
            {
                tl.LogMessage("Temperature Get", "Not implemented");
                throw new ASCOM.PropertyNotImplementedException("Temperature", false);
            }
        }

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Focuser";
                if (bRegister)
                {
                    P.Register(driverID, driverDescription);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion


        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!m_Connected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void FancyLogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            tl.LogMessage(identifier, msg);
        }
        internal static void ResetToZero()
        {
            m_Port.Write("z");
            // Arduino always reports the new position (as a string)
            // convert it to a number
            m_Position = Convert.ToInt32(m_Port.ReadLine());
            tl.LogMessage("Action", "ResetToZero successful");

        }
        #endregion
    }
}
