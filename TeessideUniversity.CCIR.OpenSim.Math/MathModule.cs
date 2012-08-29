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
using System.Collections.Generic;

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

            m_scriptModuleComms.RegisterConstant("OS_MATH_PHI", (float)PHI);
            m_scriptModuleComms.RegisterConstant("OS_MATH_TWO_PHI",
                    (float)(PHI * 2.0));
            m_scriptModuleComms.RegisterConstant("OS_MATH_PHI_BY_TWO",
                    (float)(PHI / 2.0));

            #endregion

            // Defined as the ratio of a circle's circumference to its radius
            #region TAU

            m_scriptModuleComms.RegisterConstant("OS_MATH_TAU",
                    (float)(Math.PI * 2.0));
            m_scriptModuleComms.RegisterConstant("OS_MATH_TWO_TAU",
                    (float)(Math.PI * 4.0));
            m_scriptModuleComms.RegisterConstant("OS_MATH_TAU_BY_TWO",
                    (float)Math.PI);

            #endregion

            #endregion

            m_scriptModuleComms.RegisterScriptInvocation(this, new string[]{
                "osMathVecMultiply",
                "osMathVecDivide",
                "osMathVecFloor",
                "osMathVecRound",
                "osMathVecCeil",
                "osMathVecMin",
                "osMathVecMax",
                "osMathVecVolume"
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

        #region constants

        private static readonly double sqrt5 = Math.Sqrt(5);

        private static readonly double PHI = (1 + Math.Sqrt(5)) / 2.0;

        private static readonly double PSI = -1 / ((1 + Math.Sqrt(5)) / 2.0);

        #endregion

        #region Vectors

        /// <summary>
        /// Multiplies one vector by another via libOMV to compensate for the
        /// lack of LSL syntax for multiplying two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>a * b</returns>
        public Vector3 osMathVecMultiply(UUID host, UUID script, Vector3 a, Vector3 b)
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
        public Vector3 osMathVecDivide(UUID host, UUID script, Vector3 a, Vector3 b)
        {
            Vector3 c = a / b;

            if (float.IsNaN(c.X) || float.IsInfinity(c.X))
                c.X = 0;

            if (float.IsNaN(c.Y) || float.IsInfinity(c.Y))
                c.Y = 0;

            if (float.IsNaN(c.Z) || float.IsInfinity(c.Z))
                c.Z = 0;

            return c;
        }

        /// <summary>
        /// Floors all axis in the vector.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3 osMathVecFloor(UUID host, UUID script, Vector3 a)
        {
            return new Vector3(
                    (float)Math.Floor(a.X), (float)Math.Floor(a.Y),
                    (float)Math.Floor(a.Z));
        }

        /// <summary>
        /// Rounds all axis in the vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3 osMathVecRound(UUID host, UUID script, Vector3 a)
        {
            return new Vector3(
                    (float)Math.Round(a.X), (float)Math.Round(a.Y),
                    (float)Math.Round(a.Z));
        }

        /// <summary>
        /// Ceils all axis in the vector
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public Vector3 osMathVecCeil(UUID host, UUID script, Vector3 a)
        {
            return new Vector3(
                    (float)Math.Ceiling(a.X), (float)Math.Ceiling(a.Y),
                    (float)Math.Ceiling(a.Z));
        }

        /// <summary>
        /// Ensures that all axis in vector a are no larger than b
        /// </summary>
        /// <param name="host"></param>
        /// <param name="script"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Vector3 osMathVecMin(UUID host, UUID script, Vector3 a, float b)
        {
            return new Vector3(
                    Math.Min(b, a.X), Math.Min(b, a.Y), Math.Min(b, a.Z));
        }

        /// <summary>
        /// Ensures that all axis in vector a are no smaller than b
        /// </summary>
        /// <param name="host"></param>
        /// <param name="script"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public Vector3 osMathVecMax(UUID host, UUID script, Vector3 a, float b)
        {
            return new Vector3(
                    Math.Max(b, a.X), Math.Max(b, a.Y), Math.Max(b, a.Z));
        }

        /// <summary>
        /// Assumes the vector is the dimensions of a box and calculates it's volume
        /// </summary>
        /// <param name="host"></param>
        /// <param name="script"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public float osMathVecVolume(UUID host, UUID script, Vector3 a)
        {
            return a.X * a.Y * a.Z;
        }

        #endregion

        /// <summary>
        /// Returns a Fibonacci sequence from point n of the specified length
        /// </summary>
        /// <param name="host"></param>
        /// <param name="script"></param>
        /// <param name="n"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <remarks>
        /// Since we're dealing with arbitrary sections of the Fibonacci 
        /// sequence, we want to use Binet's formula to aid speed of execution.
        /// http://en.wikipedia.org/wiki/Binet%27s_formula
        /// </remarks>
        public object[] osMathFibonacci(UUID host, UUID script, int n, int length)
        {
            List<int> resp = new List<int>();

            int a = -1;
            int b = 1;
            int c;

            if (n != 0)
            {
                a = (int)Math.Round((Math.Pow(PHI, n - 2) - Math.Pow(PSI, n - 2)) / sqrt5);
                b = (int)Math.Round((Math.Pow(PHI, n - 1) - Math.Pow(PSI, n - 1)) / sqrt5);
            }

            int j = Math.Max(1, length);
            for (int i = 0; i < j; ++i)
            {
                c = a;
                a = b;
                b = c + a;
                resp.Add(b);
            }

            return resp.ConvertAll<object>(x =>
            {
                return (object)x;
            }).ToArray();
        }

        #endregion
    }
}
