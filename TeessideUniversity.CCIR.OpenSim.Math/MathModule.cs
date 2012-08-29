/*
 * Copyright (c) Contributors, Teesside University Centre for Construction Innovation and Research
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Reflection;

using log4net;
using Nini.Config;
using Mono.Addins;
using OpenMetaverse;

using OpenSim.Framework;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.ScriptEngine.Shared;
using OpenSim.Region.ScriptEngine.Shared.ScriptBase;

using LSL_Float = OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLFloat;
using LSL_Integer = OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLInteger;
using LSL_Key = OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLString;
using LSL_List = OpenSim.Region.ScriptEngine.Shared.LSL_Types.list;
using LSL_Rotation = OpenSim.Region.ScriptEngine.Shared.LSL_Types.Quaternion;
using LSL_String = OpenSim.Region.ScriptEngine.Shared.LSL_Types.LSLString;
using LSL_Vector = OpenSim.Region.ScriptEngine.Shared.LSL_Types.Vector3;

[assembly: Addin("MathModule", "0.1")]
[assembly: AddinDependency("OpenSim", "0.5")]

namespace TeessideUniversity.CCIR.OpenSim
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule", Id = "MathModule")]
    class MathModule : INonSharedRegionModule
    {

        #region logging

        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        private Scene m_scene;
        private IScriptModuleComms m_scriptModuleComms;

        bool m_enabled = false;

        #region INonSharedRegionModule

        public string Name
        {
            get { return "MathModule"; }
        }

        public void Initialise(IConfigSource config)
        {
            IConfig conf = config.Configs[Name];

            m_enabled = (conf != null && conf.GetBoolean("Enabled", false));
            m_log.Info("[" + Name + "]: " + (m_enabled ? "Enabled" : "Disabled"));
        }

        public void AddRegion(Scene scene)
        {
        }

        public void RemoveRegion(Scene scene)
        {
        }

        public void RegionLoaded(Scene scene)
        {
            if (!m_enabled)
                return;

            m_scene = scene;

            m_scriptModuleComms = scene.RequestModuleInterface<IScriptModuleComms>();

            if (m_scriptModuleComms == null)
            {
                m_log.Error("IScriptModuleComms could not be found, cannot add script functions");
                return;
            }

            #region Constants

            // Defined as the "Golden Ratio", useful for plant life generation,
            // also for calculating fibonacci numbers via Binet's formula
            // http://en.wikipedia.org/wiki/Golden_ratio
            // http://en.wikipedia.org/wiki/Fibonacci_number#Relation_to_the_golden_ratio
            // Fibonacci sequence in plants http://youtu.be/ahXIMUkSXX0
            #region PHI

            double PHI = (1 + Math.Sqrt(5)) / 2.0;
            m_scriptModuleComms.RegisterConstant("OS_MATH_PHI", PHI);
            m_scriptModuleComms.RegisterConstant("OS_MATH_TWO_PHI", PHI * 2.0);
            m_scriptModuleComms.RegisterConstant("OS_MATH_PHI_BY_TWO", PHI / 2.0);

            #endregion

            // Defined as the ratio of a circle's circumference to its radius
            #region TAU

            m_scriptModuleComms.RegisterConstant("OS_MATH_TAU", Math.PI * 2.0);
            m_scriptModuleComms.RegisterConstant("OS_MATH_TWO_TAU", Math.PI * 4.0);
            m_scriptModuleComms.RegisterConstant("OS_MATH_TAU_BY_TWO", Math.PI);

            #endregion

            #endregion

            m_scriptModuleComms.RegisterScriptInvocation(this, new string[5]{
                "osMathVecMultiply",
                "osMathVecDivide",
                "osMathVecFloor",
                "osMathVecRound",
                "osMathVecCeil"
            });
        }

        public void Close()
        {
        }

        public Type ReplaceableInterface
        {
            get { return null; }
        }

        #endregion

        #region OSSL

        /// <summary>
        /// Multiplies one vector by another via libOMV to compensate for the
        /// lack of LSL syntax for multiplying two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>a * b</returns>
        public Vector3d osMathVecMultiply(UUID host, UUID script, Vector3d a, Vector3d b)
        {
            return a * b;
        }

        /// <summary>
        /// Divides one vector by antoher via libOMV to compensate for the lack
        /// of LSL syntax for dividing two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>a / b</returns>
        /// <remarks>
        /// libOMV will set axis to NaN or Infinity when dividing by zero, but
        /// we want zero in those cases because LSL does not have constants for
        /// NaN or Infinity.
        /// </remarks>
        public Vector3d osMathVecDivide(UUID host, UUID script, Vector3d a, Vector3d b)
        {
            Vector3d c = a / b;

            if (double.IsNaN(c.X) || double.IsInfinity(c.X))
                c.X = 0;

            if (double.IsNaN(c.Y) || double.IsInfinity(c.Y))
                c.Y = 0;

            if (double.IsNaN(c.Z) || double.IsInfinity(c.Z))
                c.Z = 0;

            return c;
        }

        /// <summary>
        /// Floors all axis in the vector.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3d osMathVecFloor(UUID host, UUID script, Vector3d a)
        {
            return new Vector3d(
                Math.Floor(a.X), Math.Floor(a.Y), Math.Floor(a.Z));
        }

        /// <summary>
        /// Rounds all axis in the vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3d osMathVecRound(UUID host, UUID script, Vector3d a)
        {
            return new Vector3d(
                Math.Round(a.X), Math.Round(a.Y), Math.Round(a.Z));
        }

        /// <summary>
        /// Ceils all axis in the vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3d osMathVecCeil(UUID host, UUID script, Vector3d a)
        {
            return new Vector3d(
                Math.Ceiling(a.X), Math.Ceiling(a.Y), Math.Ceiling(a.Z));
        }

        #endregion
    }
}
