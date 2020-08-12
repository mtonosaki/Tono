// (c) 2020 Manabu Tonosaki
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Tono.Jit.Utils;
using ProcessKeyPath = System.String;

namespace Tono.Jit
{
    /// <summary>
    /// Just-in-time Object of Work Location
    /// </summary>
    [JacTarget(Name = "Location")]
    public class JitLocation
    {
        /// <summary>
        /// Target Stage
        /// </summary>
        public JitStage Stage { get; set; }

        /// <summary>
        /// Process Location ID of the target Stage (NOT include this JitLocation.Process name)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Process Location ID of the target Stage (INCLUDE this JitLocation.Process name)
        /// </summary>
        public string FullPath
        {
            get
            {
                if (Process != null)
                {
                    return CombinePath(Path, GetProcessKey(Process));
                }
                else
                {
                    return Path;
                }
            }
        }

        /// <summary>
        /// Subset of the Process
        /// </summary>
        public JitSubset SubsetCache { get; set; }

        /// <summary>
        /// Process(class) Instance
        /// </summary>
        public JitProcess Process { get; set; }

        /// <summary>
        /// Create new instance of Stage Root Path
        /// </summary>
        /// <param name="subset"></param>
        /// <param name="process"></param>
        /// <returns></returns>
        public static JitLocation CreateRoot(JitStage stage, JitProcess process = null)
        {
            return new JitLocation
            {
                Stage = stage,
                SubsetCache = stage,
                Path = "\\",
                Process = process,
            };
        }

        public static JitLocation Create(JitStage stage, string pathFromStage, JitProcess process = null)
        {
            JitLocation loc;
            if (pathFromStage == "\\")
            {
                return CreateRoot(stage, process);
            }
            else
            {
                loc = stage.FindSubsetProcess(CreateRoot(stage), pathFromStage);
                return new JitLocation
                {
                    Stage = stage,
                    SubsetCache = loc?.Process as JitSubset,
                    Path = pathFromStage,
                    Process = process,
                };
            }
        }

        public static string CombinePath(params string[] pathes)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < pathes.Length; i++)
            {
                var path = pathes[i];
                if (path.StartsWith("\\"))
                {
                    sb.Clear();
                }
                if (path.EndsWith("\\") == false)
                {
                    path = path + "\\";
                }
                sb.Append(path);
            }
            var ret = sb.ToString();
            if (ret == "\\")
            {
                return ret;
            }
            else
            {
                return StrUtil.Left(ret, ret.Length - 1);
            }
        }

        public static string Normalize(string path)
        {
            var pathes = path.Split('\\').ToList();
            var maxCount = pathes.Count;
            for (var i = 0; i < pathes.Count; i++)
            {
                switch (pathes[i])
                {
                    case ".":
                        pathes.RemoveAt(i--);
                        break;
                    case "..":
                        pathes.RemoveAt(i--);
                        if (string.IsNullOrEmpty(pathes[i]) == false)
                        {
                            pathes.RemoveAt(i--);
                        }
                        else
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                        break;
                }
            }
            var ret = string.Join("\\", pathes);
            if (maxCount > pathes.Count && ret == "")
            {
                ret = "\\";
            }
            return ret;
        }

        /// <summary>
        /// Remove last section (last ProcessKey)
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string GetPath(string fullPath)
        {
            var id = fullPath.LastIndexOf('\\');
            if (id >= 0)
            {
                var ret = fullPath.Substring(0, id);
                if (string.IsNullOrEmpty(ret))
                {
                    return "\\";
                }
                else
                {
                    return ret;
                }
            }
            else
            {
                return fullPath;
            }
        }

        /// <summary>
        /// Make a new instance of empty process / path
        /// </summary>
        /// <returns></returns>
        public JitLocation ToEmptyPath()
        {
            return new JitLocation
            {
                Stage = Stage,
                SubsetCache = null,
                Path = null,
                Process = null,
            };
        }

        /// <summary>
        /// Copy instance exception Process = null
        /// </summary>
        /// <returns></returns>
        public JitLocation ToEmptyProcess()
        {
            return new JitLocation
            {
                Stage = Stage,
                SubsetCache = SubsetCache,
                Path = Path,
                Process = null,
            };
        }
        public JitLocation ToChangeProcess(JitProcess newProcess)
        {
            return new JitLocation
            {
                Stage = Stage,
                SubsetCache = SubsetCache,
                Path = Path,
                Process = newProcess,
            };
        }
        public static bool operator ==(JitLocation a, JitLocation b) { return a?.Equals(b) ?? b?.Equals(a) ?? true; }
        public static bool operator !=(JitLocation a, JitLocation b) { return !a?.Equals(b) ?? !b?.Equals(a) ?? false; }

        public override bool Equals(object obj)
        {
            if (obj is JitLocation tar)
            {
                if (tar.Stage?.Equals(Stage) ?? false)
                {
                    if (tar.Path.Equals(Path))
                    {
                        return tar.Process?.Equals(Process) ?? false;
                    }
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            var sid = Stage?.ID ?? "";
            var pid = Process?.ID ?? "";
            return $"{sid}::{Path}::{pid}".GetHashCode();
        }
        public override string ToString()
        {
            return $"{GetType().Name} FullPath={FullPath}";
        }

        /// <summary>
        /// Bridge to Stage.FindSubsetProcess()
        /// </summary>
        /// <param name="procKeyPath"></param>
        /// <param name="isReturnNull"></param>
        /// <returns></returns>
        public JitLocation FindSubsetProcess(ProcessKeyPath procKeyPath, bool isReturnNull = false)
        {
            return Stage.FindSubsetProcess(ToEmptyProcess(), procKeyPath, isReturnNull);
        }

        /// <summary>
        /// Bridge to Stage.GetWorks()
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks()
        {
            return Stage.GetWorks(this);
        }

        /// <summary>
        /// Bridge to Stage.GetWorks()
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(JitWork Work, DateTime EnterTime)> GetWorks(JitProcess process)
        {
            return Stage.GetWorks(ToChangeProcess(process));
        }
    }
}
